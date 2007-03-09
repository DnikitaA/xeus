using System.Collections.Generic ;
using System.Timers ;
using agsXMPP ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.roster ;
using agsXMPP.protocol.iq.vcard ;

namespace xeus.Core
{
	internal class Roster
	{
		private ObservableCollectionDisp< RosterItem > _items =
			new ObservableCollectionDisp< RosterItem >( App.DispatcherThread ) ;

		#region delegates

		public delegate bool PresenceSubscribeVetoHandler( Jid jid ) ;

		#endregion

		#region events

		public event PresenceSubscribeVetoHandler PresenceSubscribeVeto ;

		#endregion

		private Timer _reloadTime = new Timer( 5000 ) ;

		private Dictionary< string, Presence > _presences = new Dictionary< string, Presence >() ;

		public void ReadRosterFromDb()
		{
			_reloadTime.AutoReset = false ;
			_reloadTime.Elapsed += new ElapsedEventHandler( _reloadTime_Elapsed ) ;

			Database database = new Database() ;

			List< RosterItem > dbRosterItems = database.ReadRosterItems() ;

			lock ( _items._syncObject )
			{
				foreach ( RosterItem item in dbRosterItems )
				{
					Vcard vcard = Storage.GetVcard( item.Key ) ;
					item.SetVcard( vcard ) ;

					_items.Add( item ) ;
				}
			}
		}

		private void _reloadTime_Elapsed( object sender, ElapsedEventArgs e )
		{
			lock ( _items._syncObject )
			{
				foreach ( RosterItem rosterItem in _items )
				{
					if ( !rosterItem.HasVCardRecivied )
					{
						AskForVCard( rosterItem.Key ) ;
					}
				}
			}
		}

		public ObservableCollectionDisp< RosterItem > Items
		{
			get
			{
				return _items ;
			}
		}

		public void AddRemoveItem( RosterItem rosterItem )
		{
			lock ( _items._syncObject )
			{
				_items.Remove( rosterItem ) ;
				_items.Add( rosterItem ) ;
			}
		}

		public void RegisterEvents( XmppClientConnection xmppConnection )
		{
			xmppConnection.OnRosterItem += new XmppClientConnection.RosterHandler( xmppConnecion_OnRosterItem ) ;
			xmppConnection.OnPresence += new XmppClientConnection.PresenceHandler( xmppConnection_OnPresence ) ;
		}

		public RosterItem FindItem( string bare )
		{
			lock ( _items._syncObject )
			{
				foreach ( RosterItem rosterItem in _items )
				{
					if ( string.Compare( rosterItem.Key, bare, true ) == 0 )
					{
						return rosterItem ;
					}
				}
			}

			return null ;
		}

		private void ChangePresence( Presence presence )
		{
			// if it is already in roster, change status property
			RosterItem rosterItem = FindItem( presence.From.Bare ) ;

			if ( rosterItem != null && presence.Error == null )
			{
				rosterItem.Presence = presence ;

				if ( rosterItem.IsService )
				{
					if ( presence.Type == PresenceType.available )
					{
						_reloadTime.Start() ;
					}
					else
					{
						App.Instance.Window.AlertError( "Service problem",
						                                string.Format( "Service {0} became unavailable", rosterItem.Key ) ) ;
					}
				}

				if ( !rosterItem.HasVCardRecivied && presence.Type == PresenceType.available )
				{
					AskForVCard( rosterItem.Key ) ;
				}
			}
			else
			{
				lock ( _presences )
				{
					_presences[ presence.From.Bare ] = presence ;
				}
			}
		}

		private void xmppConnection_OnPresence( object sender, Presence pres )
		{
			switch ( pres.Type )
			{
				case PresenceType.subscribe:
					{
						Client.Instance.SubscribePresence( pres.From, OnSubscribePresenceVeto( pres.From ) ) ;
						break ;
					}
				case PresenceType.subscribed:
					{
						App.Instance.Window.AlertInfo( "Authorization", string.Format( "You were authorized by {0}", pres.From ) ) ;
						AskForVCard( pres.From.Bare ) ;
						break ;
					}
				case PresenceType.unsubscribe:
					{
						break ;
					}
				case PresenceType.unsubscribed:
					{
						App.Instance.Window.AlertInfo( "Authorization",
						                               string.Format( "{0} removed the authorization from you", pres.From ) ) ;
						break ;
					}
				default:
					{
						ChangePresence( pres ) ;
						break ;
					}
			}
		}

		protected virtual bool OnSubscribePresenceVeto( Jid jid )
		{
			if ( PresenceSubscribeVeto != null )
			{
				return PresenceSubscribeVeto( jid ) ;
			}

			return true ;
		}

		private void VcardResult( object sender, IQ iq, object data )
		{
			// if it is already in roster, change status property
			RosterItem rosterItem = FindItem( ( string ) data ) ;

			if ( rosterItem != null )
			{
				if ( iq.Type == IqType.error || iq.Error != null )
				{
					rosterItem.Errors.Add( string.Format( "{0}: {1}", iq.Error.Code, iq.Error.Message ) ) ;
				}
				else if ( iq.Type == IqType.result )
				{
					rosterItem.HasVCardRecivied = true ;
					rosterItem.SetVcard( iq.Vcard ) ;

					if ( iq.Vcard != null )
					{
						Storage.CacheVCard( iq.Vcard, rosterItem.Key ) ;
					}
				}
			}
		}

		private void xmppConnecion_OnRosterItem( object sender, agsXMPP.protocol.iq.roster.RosterItem item )
		{
			RosterItem existingRosterItem = FindItem( item.Jid.Bare ) ;

			lock ( _items._syncObject )
			{
				Presence presence = null ;

				lock ( _presences )
				{
					if ( _presences.ContainsKey( item.Jid.Bare ) )
					{
						presence = _presences[ item.Jid.Bare ] ;
						_presences.Remove( item.Jid.Bare ) ;
					}
				}


				if ( item.Subscription == SubscriptionType.remove )
				{
					if ( existingRosterItem != null )
					{
						_items.Remove( existingRosterItem ) ;
					}
				}
				else
				{
					if ( existingRosterItem != null )
					{
						existingRosterItem.XmppRosterItem = item ;

						if ( presence != null )
						{
							existingRosterItem.Presence = presence ;
						}
					}
					else
					{
						RosterItem rosterItem = new RosterItem( item ) ;

						if ( presence != null )
						{
							rosterItem.Presence = presence ;
						}

						_items.Add( rosterItem ) ;

						AskForVCard( rosterItem.Key ) ;
					}
				}
			}
		}

		private void AskForVCard( string jid )
		{
			// ask for VCard
			VcardIq viq = new VcardIq( IqType.get, new Jid( jid ) ) ;
			Client.Instance.SendIqGrabber( viq, new IqCB( VcardResult ), jid ) ;
		}

		public void DeleteRosterItem( RosterItem rosterItem )
		{
			if ( rosterItem.IsService )
			{
				Client.Instance.UnregisterService( new Jid( rosterItem.Key ) ) ;
			}

			if ( rosterItem.IsInitialized )
			{
				Client.Instance.RosterManager.RemoveRosterItem( new Jid( rosterItem.Key ) ) ;
			}
			else
			{
				lock ( _items._syncObject )
				{
					_items.Remove( rosterItem ) ;
				}
			}
		}
	}
}