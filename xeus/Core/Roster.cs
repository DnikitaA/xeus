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

		private Timer _rosterItemTimer = new Timer( 100 ) ;
		private Queue< RosterItem > _rosterItemsWithNoVCard = new Queue< RosterItem >( 128 ) ;
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
					rosterItem.HasVCard = true ;

					if ( iq.Type == IqType.error || iq.Error != null )
					{
						rosterItem.Errors.Add( string.Format( "{0}: {1}", iq.Error.Code, iq.Error.Message ) ) ;
					}
					else if ( iq.Type == IqType.result && iq.Vcard != null )
					{
						Vcard vcard = iq.Vcard ;

						rosterItem.Birthday = vcard.Birthday ;
						rosterItem.Description = vcard.Description ;
						rosterItem.EmailPreferred = vcard.GetPreferedEmailAddress() ;
						rosterItem.FullName = vcard.Fullname ;
						rosterItem.NickName = vcard.Nickname ;
						rosterItem.Organization = vcard.Organization ;
						rosterItem.Role = vcard.Role ;
						rosterItem.Title = vcard.Title ;
						rosterItem.Url = vcard.Url ;

						BitmapImage image = Storage.ImageFromPhoto( vcard.Photo ) ;

						if ( image != null )
						{
							Storage.CacheAvatar( rosterItem.Key, vcard.Photo ) ;
						}
						else
						{
							image = Storage.GetAvatar( rosterItem.Key ) ;
						}

						rosterItem.Image = image ;
					}

					if ( rosterItem.Image == null )
					{
						rosterItem.Image = Storage.GetAvatar( rosterItem.Key ) ;
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
					_rosterItemsWithNoVCard.Enqueue( rosterItem ) ;
				}
			}
		}

		private void _rosterItemTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			RosterItem rosterItem ;

			if ( _rosterItemsWithNoVCard.Count > 0 )
			{
				rosterItem = _rosterItemsWithNoVCard.Dequeue() ;
			}
			else
			{
				foreach ( RosterItem item in _items )
				{
					if ( !item.HasVCard )
					{
						lock ( _lockRosterItems )
						{
							if ( !_rosterItemsWithNoVCard.Contains( item ) )
							{
								// using timer frees the UI - on roster item are called synchronously for all items on startup
								_rosterItemsWithNoVCard.Enqueue( item ) ;
								break ;
							}
						}
					}
				}

				return ;
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