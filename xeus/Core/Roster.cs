using System.Collections.Generic ;
using System.Timers ;
using System.Windows.Threading ;
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

		private delegate void PresenceCallback( Presence presence ) ;

		private delegate void VcardResultCallback( object sender, IQ iq, object data ) ;

		public delegate bool PresenceSubscribeVetoHandler( Jid jid ) ;

		#endregion

		#region events

		public event PresenceSubscribeVetoHandler PresenceSubscribeVeto ;

		#endregion

		Timer _reloadTime = new Timer( 20000 );

		public void ReadRosterFromDb()
		{
			_reloadTime.AutoReset = false ;
			_reloadTime.Elapsed += new ElapsedEventHandler( _reloadTime_Elapsed );

			Database database =  new Database();

			List< RosterItem > dbRosterItems = database.ReadRosterItems() ;

			foreach ( RosterItem item in dbRosterItems )
			{
				Vcard vcard = Storage.GetVcard( item.Key ) ;
				item.SetVcard( vcard ) ;

				_items.Add( item ) ;
			}
		}

		void _reloadTime_Elapsed( object sender, ElapsedEventArgs e )
		{
			
		}

		public ObservableCollectionDisp< RosterItem > Items
		{
			get
			{
				return _items ;
			}
		}

		public void RegisterEvents( XmppClientConnection xmppConnection )
		{
			xmppConnection.OnRosterItem += new XmppClientConnection.RosterHandler( xmppConnecion_OnRosterItem ) ;
			xmppConnection.OnPresence += new XmppClientConnection.PresenceHandler( xmppConnection_OnPresence ) ;
		}

		public RosterItem FindItem( string bare )
		{
			lock ( _items )
			{
				foreach ( RosterItem rosterItem in _items )
				{
					if ( rosterItem.Key == bare )
					{
						return rosterItem ;
					}
				}
			}

			return null ;
		}

		private void ChangePresence( Presence presence )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				// if it is already in roster, change status property
				RosterItem rosterItem = FindItem( presence.From.Bare ) ;

				if ( rosterItem != null && presence.Error == null )
				{
					rosterItem.Presence = presence ;

					if ( !rosterItem.HasVCardRecivied && presence.Type == PresenceType.available )
					{
						AskForVCard( rosterItem.Key ) ;
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new PresenceCallback( ChangePresence ), presence, new object[] { } ) ;
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
						break ;
					}
				case PresenceType.unsubscribe:
					{
						break ;
					}
				case PresenceType.unsubscribed:
					{
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
			if ( App.DispatcherThread.CheckAccess() )
			{
				// if it is already in roster, change status property
				RosterItem rosterItem = FindItem( ( string ) data ) ;

				if ( rosterItem != null )
				{
					rosterItem.HasVCardRecivied = true ;

					if ( iq.Type == IqType.error || iq.Error != null )
					{
						rosterItem.Errors.Add( string.Format( "{0}: {1}", iq.Error.Code, iq.Error.Message ) ) ;
					}
					else if ( iq.Type == IqType.result )
					{
						rosterItem.SetVcard( iq.Vcard ) ;

						if ( iq.Vcard != null )
						{
							Storage.CacheVCard( iq.Vcard, rosterItem.Key ) ;
						}
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new VcardResultCallback( VcardResult ), sender, new object[] { iq, data } ) ;
			}
		}

		private void xmppConnecion_OnRosterItem( object sender, agsXMPP.protocol.iq.roster.RosterItem item )
		{
			RosterItem existingRosterItem = FindItem( item.Jid.Bare ) ;

			if ( item.Subscription == SubscriptionType.remove )
			{
				if ( existingRosterItem != null )
				{
					lock ( _items )
					{
						_items.Remove( existingRosterItem ) ;
					}
				}
			}
			else
			{
				if ( existingRosterItem != null )
				{
					existingRosterItem.XmppRosterItem = item ;
				}
				else
				{
					RosterItem rosterItem = new RosterItem( item ) ;

					lock ( _items )
					{
						_items.Add( rosterItem ) ;
					}

					AskForVCard( rosterItem.Key );
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
			Client.Instance.RosterManager.RemoveRosterItem( new Jid( rosterItem.Key ) ) ;
		}
	}
}