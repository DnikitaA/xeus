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
			new ObservableCollectionDisp< RosterItem >( App.DispatcherThred ) ;

		private Timer _rosterItemTimer = new Timer( 180 );
		Stack< RosterItem > _rosterItemsToBeAdded = new Stack< RosterItem >( 128 );
		object _lockRosterItems = new object();

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
			_rosterItemTimer.Elapsed += new ElapsedEventHandler( _rosterItemTimer_Elapsed );
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
			if ( App.DispatcherThred.CheckAccess() )
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
				App.DispatcherThred.BeginInvoke( DispatcherPriority.Send,
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
			if ( App.DispatcherThred.CheckAccess() )
			{
				if ( iq.Type == IqType.result && iq.Vcard != null )
				{
					Vcard vcard = iq.Vcard ;

					// if it is already in roster, change status property
					RosterItem rosterItem = FindItem( ( string )data ) ;

					if ( rosterItem != null )
					{
						rosterItem.Birthday = vcard.Birthday ;
						rosterItem.Description = vcard.Description ;
						rosterItem.EmailPreferred = vcard.GetPreferedEmailAddress() ;
						rosterItem.FullName = vcard.Fullname ;
						rosterItem.NickName = vcard.Nickname ;
						rosterItem.Organization = vcard.Organization ;
						rosterItem.Role = vcard.Role ;
						rosterItem.Title = vcard.Title ;
						rosterItem.Url = vcard.Url ;
						rosterItem.SetPhoto( vcard.Photo );
					}
				}
			}
			else
			{
				App.DispatcherThred.BeginInvoke( DispatcherPriority.Send,
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
				// using timer frees the UI - on roster item are called synchronously for all items on startup
				_rosterItemsToBeAdded.Push( rosterItem ) ;
				_rosterItemTimer.Start() ;
			}
		}

		void _rosterItemTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			RosterItem rosterItem = null ;

			lock ( _lockRosterItems )
			{
				if ( _rosterItemsToBeAdded.Count > 0 )
				{
					rosterItem = _rosterItemsToBeAdded.Pop() ;
				}
				else
				{
					_rosterItemTimer.Stop() ;				
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

				_items.Add( rosterItem ) ;

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