using System ;
using System.Net.Security ;
using System.Security.Cryptography.X509Certificates ;
using System.Threading ;
using System.Timers ;
using System.Windows.Controls ;
using agsXMPP ;
using agsXMPP.net ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.disco ;
using agsXMPP.protocol.iq.register ;
using agsXMPP.protocol.iq.roster ;
using agsXMPP.Xml.Dom ;
using Win32_API ;
using xeus.Properties ;
using Timer=System.Timers.Timer;

namespace xeus.Core
{
	internal class Client : NotifyInfoDispatcher, IDisposable
	{
		private static readonly Client _instance = new Client() ;

		private XmppClientConnection _xmppConnection = new XmppClientConnection() ;

		private Services _services = new Services() ;
		private Roster _roster = new Roster() ;
		private MessageCenter _messageCenter = new MessageCenter() ;
		private Presence _presence ;

		private Timer _discoTimer = new Timer( 1500 ) ;
		private Timer _idleTimer = new Timer( 1000 ) ;

		#region delegates

		public delegate void LoginHandler() ;

		public delegate void DiscoFinishHandler() ;

		public delegate void MessageHandler( Message msg ) ;

		#endregion

		#region events

		public event LoginHandler LoggedIn ;
		public event LoginHandler LoginError ;
		public event MessageHandler Message ;

		#endregion

		public static Client Instance
		{
			get
			{
				return _instance ;
			}
		}

		public Services Services
		{
			get
			{
				return _services ;
			}
		}

		public Roster Roster
		{
			get
			{
				return _roster ;
			}
		}

		/*
		public Agents Agents
		{
			get
			{
				return _agents ;
			}
		}*/

		private Client()
		{
		}

		public void Setup()
		{
			RegisterEvents() ;

			_xmppConnection.Username = Settings.Default.Client_UserName ;
			_xmppConnection.Password = Settings.Default.Client_Password ;
			_xmppConnection.Server = Settings.Default.Client_Server ;
			_xmppConnection.UseCompression = true ;
			_xmppConnection.UseSSL = true ;
			_xmppConnection.Priority = 10 ;
			_xmppConnection.AutoResolveConnectServer = true ;

			_xmppConnection.ConnectServer = null ;
			_xmppConnection.Resource = "xeus" ;
			_xmppConnection.SocketConnectionType = SocketConnectionType.Direct ;
			_xmppConnection.UseStartTLS = true ;
			_xmppConnection.AutoRoster = true ;
			_xmppConnection.AutoAgents = true ;

			_xmppConnection.OnRosterEnd += new ObjectHandler( _xmppConnection_OnRosterEnd ) ;
			_xmppConnection.OnMessage += new XmppClientConnection.MessageHandler( _xmppConnection_OnMessage ) ;
			_xmppConnection.OnXmppError += new OnXmppErrorHandler( _xmppConnection_OnXmppError ) ;
			_xmppConnection.OnAuthError += new OnXmppErrorHandler( _xmppConnection_OnAuthError ) ;
			_xmppConnection.ClientSocket.OnValidateCertificate +=
				new RemoteCertificateValidationCallback( ClientSocket_OnValidateCertificate ) ;
			_xmppConnection.OnSocketError += new ErrorHandler( _xmppConnection_OnSocketError ) ;

			_xmppConnection.OnXmppConnectionStateChanged +=
				new XmppConnection.XmppConnectionStateHandler( _xmppConnection_OnXmppConnectionStateChanged ) ;
			_messageCenter.RegisterEvent( _instance ) ;

			_discoTimer.AutoReset = false ;
			_discoTimer.Elapsed += new ElapsedEventHandler( _discoTimer_Elapsed ) ;

			_idleTimer.AutoReset = true ;
			_idleTimer.Elapsed += new ElapsedEventHandler( _idleTimer_Elapsed ) ;
			_idleTimer.Start() ;

			Log( "Setup finished" ) ;
		}

		private ShowType _nonIdlePresence = ShowType.NONE ;
		private bool _isIdle = false ;

		private void _idleTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			long idleTime = ( Win32.GetTickCount() - Win32.GetLastInputTime() ) ;
			TimeSpan timeSpan = new TimeSpan( 0, 0, 0, 0, ( int ) idleTime ) ;

			if ( timeSpan.TotalMinutes > Settings.Default.Client_IdleMinutesAway )
			{
				SetMyPresence( ShowType.xa, true ) ;
			}
			else if ( timeSpan.TotalMinutes > Settings.Default.Client_IdleMinutesXA )
			{
				SetMyPresence( ShowType.away, true ) ;
			}
			else
			{
				if ( _isIdle )
				{
					SetMyPresence( _nonIdlePresence, false ) ;
				}
			}
		}

		private void _discoTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			DiscoRequest() ;
		}

		private void _xmppConnection_OnXmppConnectionStateChanged( object sender, XmppConnectionState state )
		{
			App.Instance.Window.Status( state.ToString() ) ;
		}

		private void _xmppConnection_OnSocketError( object sender, Exception ex )
		{
			App.Instance.Window.AlertError( "Network error", ex.Message ) ;
		}

		private bool ClientSocket_OnValidateCertificate( object sender, X509Certificate certificate, X509Chain chain,
		                                                 SslPolicyErrors sslPolicyErrors )
		{
			return true ;
		}

		private void _xmppConnection_OnAuthError( object sender, Element e )
		{
			App.Instance.Window.AlertError( "Authorization error", e.ToString() ) ;

			if ( LoginError != null )
			{
				LoginError() ;
			}
		}

		private void _xmppConnection_OnXmppError( object sender, Element e )
		{
			App.Instance.Window.AlertError( "Protocol error", e.ToString() ) ;
		}

		private void _xmppConnection_OnMessage( object sender, Message msg )
		{
			OnMessage( msg ) ;
		}

		protected virtual void OnMessage( Message msg )
		{
			if ( Message != null )
			{
				Message( msg ) ;
			}
		}

		private void _xmppConnection_OnRosterEnd( object sender )
		{
			SetMyPresence( ShowType.NONE, false ) ;
		}

		public void Connect( bool registerNewAccount, string newPassword )
		{
			if ( _xmppConnection.XmppConnectionState != XmppConnectionState.Connected )
			{
				Log( "Opening connection" ) ;

				if ( newPassword != null )
				{
					_xmppConnection.Password = newPassword ;
				}

				_xmppConnection.RegisterAccount = registerNewAccount ;
				_xmppConnection.Open() ;
			}
		}

		public void Disconnect()
		{
			if ( _xmppConnection.XmppConnectionState != XmppConnectionState.Disconnected )
			{
				Log( "Disconnecting" ) ;

				_xmppConnection.Close() ;
			}
		}

		public void SendChatMessage( RosterItem rosterItem, string text )
		{
			if ( rosterItem.IsInitialized )
			{
				Message message = new Message() ;

				message.Type = MessageType.chat ;
				message.To = rosterItem.XmppRosterItem.Jid ;
				message.Body = text ;
				message.From = MyJid ;

				_xmppConnection.Send( message ) ;

				lock ( rosterItem.Messages._syncObject )
				{
					rosterItem.Messages.Add( new ChatMessage( message, rosterItem, DateTime.Now ) ) ;
				}
			}
		}

		public void SetMyPresence( ShowType showType, bool isIdle )
		{
			Connect( false, null ) ;

			if ( _xmppConnection.Authenticated )
			{
				if ( isIdle && !_isIdle )
				{
					// coming to idle state
					_nonIdlePresence = _xmppConnection.Show ;
					_isIdle = true ;

					App.Instance.Window.Status( "Coming to Idle State" ) ;
				}

				if ( !isIdle && _isIdle )
				{
					// coming from idle state
					_isIdle = false ;

					App.Instance.Window.Status( "Coming from Idle State" ) ;
				}

				_xmppConnection.Show = showType ;
				_xmppConnection.SendMyPresence() ;

				_presence = new Presence( _xmppConnection.Show, _xmppConnection.Status, _xmppConnection.Priority ) ;

				NotifyPropertyChanged( "MyPresence" ) ;
				NotifyPropertyChanged( "StatusTemplate" ) ;
				NotifyPropertyChanged( "IsAvailable" ) ;
			}
			else
			{
				if ( LoginError != null )
				{
					LoginError() ;
				}
			}
		}

		public void SubscribePresence( Jid jid, bool approve )
		{
			PresenceManager presenceManager = new PresenceManager( _xmppConnection ) ;

			if ( approve )
			{
				presenceManager.ApproveSubscriptionRequest( jid ) ;
				App.Instance.Window.AlertInfo( "Authorization", string.Format( "You just authorized {0}", jid.Bare ) ) ;
			}
			else
			{
				presenceManager.RefuseSubscriptionRequest( jid ) ;
			}
		}

		public void DiscoverServer()
		{
			lock ( _services.Items._syncObject )
			{
				_services.Items.Clear() ;
			}

			DiscoManager discoManager = new DiscoManager( _xmppConnection ) ;
			discoManager.DisoverItems( new Jid( _xmppConnection.Server ), new IqCB( OnDiscoServerResult ), null ) ;
		}

		#region disco server events

		private void OnDiscoServerResult( object sender, IQ iq, object data )
		{
			Log( "Server disco started" ) ;

			if ( iq.Type == IqType.result )
			{
				Element query = iq.Query ;
				if ( query != null && query.GetType() == typeof ( DiscoItems ) )
				{
					DiscoItems items = query as DiscoItems ;

					if ( items != null )
					{
						DiscoItem[] itms = items.GetDiscoItems() ;

						lock ( _services.Items._syncObject )
						{
							foreach ( DiscoItem itm in itms )
							{
								if ( itm.Jid != null )
								{
									_services.Items.Add( new ServiceItem( itm.Jid.Bare, itm.Jid ) ) ;
								}
							}
						}

						_discoTimer.Start() ;
					}
				}
			}

			Log( "Server disco finished" ) ;
		}

		private void OnUnregisterService( object sender, IQ iq, object data )
		{
			if ( iq.Error != null )
			{
				App.Instance.Window.AlertError( "Unregistration error", iq.Error.ToString() ) ;
				return ;
			}

			RegisterIq registerIq = ( RegisterIq ) data ;

			ServiceItem serviceItem = _services.FindItem( registerIq.To.Bare ) ;

			if ( serviceItem != null )
			{
				serviceItem.IsRegistered = false ;
			}
		}

		private void OnRegisterService( object sender, IQ iq, object data )
		{
			if ( iq.Error != null )
			{
				App.Instance.Window.AlertError( "Registration error", iq.Error.ToString() ) ;
				return ;
			}

			RegisterIq registerIq = ( RegisterIq ) data ;

			ServiceItem serviceItem = _services.FindItem( registerIq.To.Bare ) ;

			if ( serviceItem != null )
			{
				serviceItem.IsRegistered = true ;
			}
		}

		public void UnregisterService( Jid jid )
		{
			RegisterIq registerIq = new RegisterIq( IqType.get, jid ) ;
			registerIq.Query.AddTag( "remove" ) ;

			_xmppConnection.IqGrabber.SendIq( registerIq, OnUnregisterService, registerIq ) ;
		}

		private string _idRegisterServiceToken = String.Empty ;

		private void OnRegisterServiceGet( object sender, IQ iq, object data )
		{
			_idRegisterServiceToken = String.Empty ;

			Register register = iq.Query as Register ;

			if ( register != null )
			{
				App.Instance.Window.OpenRegisterDialog( iq, register ) ;
			}
			else
			{
				App.Instance.Window.AlertError( "Service", "This is not supported" ) ;
			}
		}

		public void RegisterService( Jid jid )
		{
			RegisterIq registerIq = new RegisterIq( IqType.get, jid ) ;

			if ( _idRegisterServiceToken == String.Empty )
			{
				_xmppConnection.IqGrabber.Remove( _idRegisterServiceToken ) ;
				_idRegisterServiceToken = String.Empty ;
			}

			_idRegisterServiceToken = registerIq.Id ;
			_xmppConnection.IqGrabber.SendIq( registerIq, OnRegisterServiceGet, _idRegisterServiceToken ) ;
		}

		public void FinishRegisterService( Jid jid, string userName, string password )
		{
			RegisterIq registerIq = new RegisterIq( IqType.set, jid ) ;
			registerIq.Query.Username = userName ;
			registerIq.Query.Password = password ;

			_xmppConnection.IqGrabber.SendIq( registerIq, OnRegisterService, registerIq ) ;
		}

		public void DiscoRequest( ServiceItem serviceItem )
		{
			DiscoManager dm = new DiscoManager( _xmppConnection ) ;
			dm.DisoverInformation( serviceItem.Jid, new IqCB( OnDiscoInfoResult ), serviceItem ) ;
		}

		public void DiscoRequest()
		{
			Thread discoThread = new Thread( new ThreadStart( AskForDiscoInfo ) ) ;
			discoThread.Priority = ThreadPriority.Lowest ;
			discoThread.Start() ;
		}

		private void AskForDiscoInfo()
		{
			ServiceItem[] serviceItems ;

			lock ( _services.Items._syncObject )
			{
				serviceItems = new ServiceItem[_services.Items.Count] ;
				_services.Items.CopyTo( serviceItems, 0 ) ;
			}

			DiscoManager dm = new DiscoManager( _xmppConnection ) ;

			foreach ( ServiceItem itm in serviceItems )
			{
				Thread.Sleep( 50 ) ;

				if ( itm.Jid != null )
				{
					dm.DisoverInformation( itm.Jid, new IqCB( OnDiscoInfoResult ), itm ) ;
				}
			}
		}

		private void OnDiscoInfoResult( object sender, IQ iq, object data )
		{
			ServiceItem item = ( ServiceItem ) data ;

			if ( iq.Type == IqType.result && iq.Query is DiscoInfo )
			{
				DiscoInfo di = iq.Query as DiscoInfo ;
				item.Disco = di ;
			}
		}

		#endregion

		private void RegisterEvents()
		{
			Log( "Registering events" ) ;

			_xmppConnection.OnLogin += new ObjectHandler( _xmppConnecion_OnLogin ) ;

			_roster.RegisterEvents( _xmppConnection ) ;
			//_agents.RegisterEvents( _xmppConnection );
		}

		public void RequestAgents()
		{
			_xmppConnection.RequestAgents() ;
		}

		public void SendIqGrabber( IQ iq, IqCB iqCallback, object cbArgument )
		{
			if ( _xmppConnection.Binded )
			{
				_xmppConnection.IqGrabber.SendIq( iq, iqCallback, cbArgument ) ;
			}
		}

		public RosterManager RosterManager
		{
			get
			{
				return _xmppConnection.RosterManager ;
			}
		}

		public MessageCenter MessageCenter
		{
			get
			{
				return _messageCenter ;
			}
		}

		private void _xmppConnecion_OnLogin( object sender )
		{
			OnLogin() ;
		}

		protected virtual void OnLogin()
		{
			if ( LoggedIn != null )
			{
				LoggedIn() ;
			}
		}

		public void Log( string text, params object[] parameters )
		{
			Console.WriteLine( text, parameters ) ;
		}

		public void Dispose()
		{
			Disconnect() ;
		}

		public ControlTemplate StatusTemplate
		{
			get
			{
				return PresenceTemplate.GetStatusTemplate( _presence ) ;
			}
		}

		public Presence MyPresence
		{
			get
			{
				return _presence ;
			}
		}

		public Jid MyJid
		{
			get
			{
				if ( _xmppConnection != null )
				{
					return _xmppConnection.MyJID ;
				}

				return null ;
			}
		}

		public bool IsAvailable
		{
			get
			{
				return ( MyPresence != null && MyPresence.Type == PresenceType.available ) ;
			}
		}

		public void AddUser( string name )
		{
			if ( name.Length > 0 )
			{
				Jid jid = new Jid( name ) ;

				_xmppConnection.RosterManager.AddRosterItem( jid ) ;

				// Ask for subscription now
				_xmppConnection.PresenceManager.Subcribe( jid ) ;
			}
		}

		public void SetRosterGropup( RosterItem rosterItem, string group )
		{
			if ( rosterItem.IsInitialized && !rosterItem.IsService )
			{
				_xmppConnection.RosterManager.UpdateRosterItem( rosterItem.XmppRosterItem.Jid,
				                                                rosterItem.NickName, group ) ;
			}
		}

		public PresenceManager PresenceManager
		{
			get
			{
				return _xmppConnection.PresenceManager ;
			}
		}
	}
}