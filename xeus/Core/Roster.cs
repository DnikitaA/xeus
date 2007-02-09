using System.Collections.Generic ;
using System.Timers ;
using System.Windows.Media.Imaging ;
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

		private Timer _rosterItemTimer = new Timer( 200 ) ;
		private Queue< RosterItem > _rosterItemsToRecieveVCard = new Queue< RosterItem >( 128 ) ;
		private object _lockRosterItems = new object() ;

		private Dictionary< string, Presence > _presences = new Dictionary< string, Presence >( 128 ) ;

		#region delegates

		private delegate void PresenceCallback( Presence presence ) ;

		private delegate void VcardResultCallback( object sender, IQ iq, object data ) ;

		public delegate bool PresenceSubscribeVetoHandler( Jid jid ) ;

		#endregion

		#region events

		public event PresenceSubscribeVetoHandler PresenceSubscribeVeto ;

		#endregion

		public Roster()
		{
			_rosterItemTimer.AutoReset = true ;
			_rosterItemTimer.Start() ;
			_rosterItemTimer.Elapsed += new ElapsedEventHandler( _rosterItemTimer_Elapsed ) ;
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
			foreach ( RosterItem rosterItem in _items )
			{
				if ( rosterItem.XmppRosterItem.Jid.Bare == bare )
				{
					return rosterItem ;
				}
			}

			return null ;
		}

		private void ChangePresence( Presence presence )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				// presence info can arrive before the user item comes into the roster
				// so presence info has to be kept separately
				_presences[ presence.From.Bare ] = presence ;

				// if it is already in roster, change status property
				RosterItem rosterItem = FindItem( presence.From.Bare ) ;

				if ( rosterItem != null )
				{
					if ( presence.From.User == null )
					{
						// this is the server 
						foreach ( RosterItem rosterItemOfService in _items )
						{
							if ( rosterItemOfService.XmppRosterItem.Jid.Server == presence.From.Server
								&& ( rosterItemOfService.Errors.Count > 0 
										|| rosterItemOfService.Presence == null ) )
							{
								rosterItemOfService.Errors.Clear();
								rosterItemOfService.HasVCardRecivied = false ;
								_rosterItemsToRecieveVCard.Enqueue( rosterItemOfService ) ;
							}
						}

					}
					rosterItem.Presence = presence ;
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
							Storage.CacheVCard( iq.Vcard, rosterItem.Key );
						}
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.ApplicationIdle,
				                                  new VcardResultCallback( VcardResult ), sender, new object[] { iq, data } ) ;
			}
		}

		private void xmppConnecion_OnRosterItem( object sender, agsXMPP.protocol.iq.roster.RosterItem item )
		{
			RosterItem rosterItem = new RosterItem( item ) ;

			if ( item.Subscription == SubscriptionType.remove )
			{
				RosterItem exisitngRosterItem = FindItem( item.Jid.Bare ) ;

				if ( exisitngRosterItem != null )
				{
					_items.Remove( exisitngRosterItem ) ;
				}
			}
			else
			{
				lock ( _lockRosterItems )
				{
					// using timer frees the UI - on roster item are called synchronously for all items on startup
					_rosterItemsToRecieveVCard.Enqueue( rosterItem ) ;
				}
			}
		}

		private void _rosterItemTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			RosterItem rosterItem = null ;

			_rosterItemTimer.Stop();
			_rosterItemTimer.Start();

			if ( _rosterItemsToRecieveVCard.Count > 0 )
			{
				lock ( _lockRosterItems )
				{
					rosterItem = _rosterItemsToRecieveVCard.Dequeue() ;

					if ( rosterItem.HasVCardRecivied )
					{
						return ;
					}
					else
					{
						_rosterItemsToRecieveVCard.Enqueue( rosterItem ) ; // push to the end of the queue
					}
				}
			}

			if ( rosterItem != null )
			{
				// check if presence info is already there
				Presence presence ;
				_presences.TryGetValue( rosterItem.Key, out presence ) ;

				if ( presence != null )
				{
					rosterItem.Presence = presence ;
				}

				Vcard vcard = Storage.GetVcard( rosterItem.Key ) ;
				rosterItem.SetVcard( vcard );
				
				if ( FindItem( rosterItem.Key ) == null )
				{
					_items.Add( rosterItem ) ;
				}

				// ask for VCard
				VcardIq viq = new VcardIq( IqType.get, new Jid( rosterItem.Key ) ) ;
				Client.Instance.SendIqGrabber( viq, new IqCB( VcardResult ), rosterItem.Key ) ;
			}
		}

		public void DeleteRosterItem( RosterItem rosterItem )
		{
			Client.Instance.RosterManager.RemoveRosterItem( new Jid( rosterItem.Key ) ) ;
		}
	}
}