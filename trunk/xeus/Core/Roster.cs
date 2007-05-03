using System ;
using System.Collections.Generic ;
using System.ComponentModel ;
using System.Drawing ;
using System.Drawing.Imaging ;
using System.IO ;
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
			new ObservableCollectionDisp< RosterItem >( App.Current.Dispatcher ) ;

		private readonly DateTime _start = DateTime.Now ;

		#region delegates

		public delegate void PresenceSubscribeHandler( Jid jid ) ;

		#endregion

		#region events

		public event PresenceSubscribeHandler PresenceSubscribe ;

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
			xmppConnection.OnPresence += new PresenceHandler( xmppConnection_OnPresence );
			Client.Instance.PropertyChanged += new PropertyChangedEventHandler( Instance_PropertyChanged ) ;
		}

		private void Instance_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			if ( e.PropertyName == "IsAvailable" && !Client.Instance.IsAvailable )
			{
				lock ( _items._syncObject )
				{
					List< RosterItem > items = new List< RosterItem >( _items.Count ) ;

					foreach ( RosterItem item in _items )
					{
						items.Add( item ) ;
					}

					foreach ( RosterItem item in items )
					{
						item.Presence = null ;
					}
				}
			}
		}

		public void ClearMesssages()
		{
			lock ( _items._syncObject )
			{
				foreach ( RosterItem rosterItem in _items )
				{
					lock ( rosterItem.Messages._syncObject )
					{
						if ( rosterItem.MessagesPreloaded )
						{
							rosterItem.Messages.Clear() ;
							rosterItem.MessagesPreloaded = false ;
						}
					}
				}
			}
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
				string oldText = rosterItem.StatusText ;

				Presence oldPresence = rosterItem.Presence ;

				rosterItem.Presence = presence ;

				if ( rosterItem.StatusText != oldText
				     && _start.AddSeconds( 5.0 ) < DateTime.Now )
				{
					Client.Instance.Event.AddEvent( new EventContactStatusChanged( rosterItem, oldPresence ) ) ;
				}

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
				else if ( presence.Type == PresenceType.available )
				{
					Client.Instance.DiscoRequest( rosterItem ) ;
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

		private object _lockPresence = new object();

		private void xmppConnection_OnPresence( object sender, Presence pres )
		{
			lock ( _lockPresence )
			{
				RosterItem rosterItem = FindItem( pres.From.Bare ) ;

				string name = ( rosterItem != null ) ? rosterItem.DisplayName : pres.From.Bare ;

				switch ( pres.Type )
				{
					case PresenceType.subscribe:
						{
							OnSubscribePresence( pres.From ) ;
							break ;
						}
					case PresenceType.subscribed:
						{
							App.Instance.Window.AlertInfo( "Authorization", string.Format( "You were authorized by {0}", name ) ) ;
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
							                               string.Format( "{0} removed the authorization from you", name ) ) ;
							break ;
						}
					default:
						{
							ChangePresence( pres ) ;
							break ;
						}
				}
			}
		}

		protected virtual void OnSubscribePresence( Jid jid )
		{
			if ( PresenceSubscribe != null )
			{
				PresenceSubscribe( jid ) ;
			}
		}

		public void PublishMyVCard()
		{
			RosterItem item = Client.Instance.MyRosterItem ;
			VcardIq viq = new VcardIq( IqType.set, Client.Instance.MyJid ) ;

			if ( !string.IsNullOrEmpty( item.EmailPreferred ) )
			{
				viq.Vcard.AddEmailAddress( new Email( EmailType.INTERNET, item.EmailPreferred, true ) ) ;
			}

			viq.Vcard.Url = item.Url ;
			viq.Vcard.Title = item.Title ;
			viq.Vcard.Birthday = item.Birthday ;
			viq.Vcard.Role = item.Role ;
			viq.Vcard.Description = item.Description ;
			viq.Vcard.Fullname = item.FullName ;
			viq.Vcard.Nickname = item.NickName ;

			if ( !string.IsNullOrEmpty( item.Organization ) )
			{
				viq.Vcard.Organization = new Organization( item.Organization, String.Empty ) ;
			}

			if ( !string.IsNullOrEmpty( item.ImageFileName ) )
			{
				SetPicture( item.ImageFileName, viq.Vcard, item.RemoveTemporaryImage ) ;
			}

			Client.Instance.Send( viq ) ;
		}

		private void SetPicture( string filename, Vcard vcard, bool deleteImage )
		{
			try
			{
				Image img = Image.FromFile( filename ) ;
				ImageFormat imageFormat = null ;

				vcard.Photo = new Photo( img, imageFormat ) ;

				img.Dispose() ;

				if ( deleteImage )
				{
					File.Delete( filename ) ;
				}
			}

			catch ( Exception )
			{
			}
		}

		private void VcardResult( object sender, IQ iq, object data )
		{
			// if it is already in roster, change status property
			RosterItem rosterItem = FindItem( ( string ) data ) ;

			if ( ( string ) data == Client.Instance.MyJid.Bare )
			{
				if ( Client.Instance.MyRosterItem == null )
				{
					rosterItem = new RosterItem( new agsXMPP.protocol.iq.roster.RosterItem( Client.Instance.MyJid ) ) ;
					Client.Instance.MyRosterItem = rosterItem ;
				}
				else
				{
					rosterItem = Client.Instance.MyRosterItem ;
				}
			}
			else if ( rosterItem == null )
			{
				rosterItem = new RosterItem( new agsXMPP.protocol.iq.roster.RosterItem( iq.From ) ) ;

				lock ( _items._syncObject )
				{
					_items.Add( rosterItem );
				}
			}

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

		private object _lockRosterItem = new object();

		private void xmppConnecion_OnRosterItem( object sender, agsXMPP.protocol.iq.roster.RosterItem item )
		{
			lock ( _lockRosterItem )
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
						if ( item.Ask == AskType.NONE )
						{
							RosterItem rosterItemComing = null ;

							if ( existingRosterItem != null )
							{
								existingRosterItem.XmppRosterItem = item ;

								if ( presence != null )
								{
									existingRosterItem.Presence = presence ;
								}

								rosterItemComing = existingRosterItem ;
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

								rosterItemComing = rosterItem ;
							}

							if ( rosterItemComing.Key == Client.Instance.MyJid.Bare )
							{
								Client.Instance.MyRosterItem = rosterItemComing ;
							}

							if ( rosterItemComing.IsInitialized && !rosterItemComing.IsService )
							{
								SetServiceType( rosterItemComing ) ;
							}
						}
					}
				}
			}
		}

		public static void SetServiceType( RosterItem rosterItem )
		{
			lock ( Client.Instance.Services.Items._syncObject )
			{
				foreach ( ServiceItem serviceItem in Client.Instance.Services.Items )
				{
					if ( rosterItem.XmppRosterItem.Jid.Server
					     == serviceItem.Jid.Server )
					{
						rosterItem.Transport = serviceItem.Type ;
						break ;
					}
				}
			}
		}

		public void AskForVCard( string jid )
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

			Database database = new Database() ;
			database.DeleteRosterItem( rosterItem ) ;

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