using System ;
using System.Diagnostics ;
using System.Net.Security ;
using System.Security.Cryptography.X509Certificates ;
using System.Threading ;
using System.Timers ;
using System.Windows.Controls ;
using agsXMPP ;
using agsXMPP.net ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.extensions.chatstates ;
using agsXMPP.protocol.iq.disco ;
using agsXMPP.protocol.iq.register ;
using agsXMPP.protocol.iq.roster ;
using agsXMPP.protocol.sasl ;
using agsXMPP.Xml.Dom ;
using Win32_API ;
using xeus.Controls ;
using xeus.Properties ;
using Timer=System.Timers.Timer;

namespace xeus.Core
{
	internal class Client : NotifyInfoDispatcher, IDisposable
	{
		private static readonly Client _instance = new Client() ;

		private XmppClientConnection _xmppConnection ;

		private Services _services ;
		private Roster _roster ;
		private MessageCenter _messageCenter ;
		private Presence _presence ;
		private Event _event = new Event() ;

		private Timer _discoTimer ;
		private Timer _idleTimer ;

		#region delegates

		public delegate void LoginHandler() ;

		public delegate void DiscoFinishHandler() ;

		public delegate void MessageHandler( Message msg ) ;

		#endregion

		#region events

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
				if ( _roster == null )
				{
					_roster = new Roster() ;
				}

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
			_xmppConnection = new XmppClientConnection() ;

			_services = new Services() ;

			_discoTimer = new Timer( 1500 ) ;
			_idleTimer = new Timer( 1000 ) ;

			RegisterEvents() ;

			XmppConnection.UseCompression = true ;
			// XmppConnection.UseSSL = true ;
			XmppConnection.Priority = 10 ;
			XmppConnection.AutoResolveConnectServer = true ;

			XmppConnection.ConnectServer = null ;
			XmppConnection.Resource = "xeus" ;
			XmppConnection.SocketConnectionType = SocketConnectionType.Direct ;
			XmppConnection.UseStartTLS = true ;
			XmppConnection.AutoRoster = true ;
			XmppConnection.AutoAgents = true ;

			XmppConnection.OnRosterEnd += new ObjectHandler( _xmppConnection_OnRosterEnd ) ;
			XmppConnection.OnMessage += new agsXMPP.protocol.client.MessageHandler( XmppConnection_OnMessage );
			XmppConnection.OnXmppError += new XmppElementHandler( XmppConnection_OnXmppError );
			XmppConnection.OnAuthError += new XmppElementHandler( XmppConnection_OnAuthError );
			XmppConnection.ClientSocket.OnValidateCertificate +=
				new RemoteCertificateValidationCallback( ClientSocket_OnValidateCertificate ) ;
			XmppConnection.OnSocketError += new ErrorHandler( _xmppConnection_OnSocketError ) ;
			XmppConnection.OnClose += new ObjectHandler( _xmppConnection_OnClose );

			XmppConnection.OnXmppConnectionStateChanged += new XmppConnectionStateHandler( XmppConnection_OnXmppConnectionStateChanged );

			XmppConnection.OnRegisterInformation += new RegisterEventHandler( _xmppConnection_OnRegisterInformation );
			XmppConnection.OnRegistered += new ObjectHandler( _xmppConnection_OnRegistered );
			XmppConnection.OnIq += new IqHandler( XmppConnection_OnIq );

			_messageCenter.RegisterEvent( _instance ) ;

			_discoTimer.AutoReset = false ;
			_discoTimer.Elapsed += new ElapsedEventHandler( _discoTimer_Elapsed ) ;

			_idleTimer.AutoReset = true ;
			_idleTimer.Elapsed += new ElapsedEventHandler( _idleTimer_Elapsed ) ;
			_idleTimer.Start() ;

			Log( "Setup finished" ) ;
		}

		void XmppConnection_OnIq( object sender, IQ iq )
		{
			if ( iq != null )
			{
				// No Iq with query
				if ( iq.HasTag( typeof ( agsXMPP.protocol.extensions.si.SI ) ) )
				{
					if ( iq.Type == IqType.set )
					{
						agsXMPP.protocol.extensions.si.SI si =
							iq.SelectSingleElement( typeof ( agsXMPP.protocol.extensions.si.SI ) ) as agsXMPP.protocol.extensions.si.SI ;

						agsXMPP.protocol.extensions.filetransfer.File file = si.File ;

						if ( file != null )
						{
							TransferWindow.Transfer( XmppConnection, iq ) ;
						}
					}
					else if ( iq.Type == IqType.result )
					{
						
					}
				}
			}

			if ( iq != null && iq.Type == IqType.get )
			{
				Element query = iq.Query ;

                if ( query != null )
                {
                    if ( query.GetType() == typeof( agsXMPP.protocol.iq.version.Version ) )
                    {
						// its a version IQ VersionIQ
						agsXMPP.protocol.iq.version.Version version = query as agsXMPP.protocol.iq.version.Version;
						
						// Somebody wants to know our client version, so send it back
						iq.SwitchDirection();
						iq.Type = IqType.result;

						version.Name = "xeus";
						version.Ver = "1.0 alpha";
						version.Os = Environment.OSVersion.ToString();

						XmppConnection.Send( iq );
                    }                	
					else if ( query.GetType() == typeof( DiscoInfo ) )
                    {
						DiscoInfo disco = query as DiscoInfo;
						
						iq.SwitchDirection();
						iq.Type = IqType.result;

						disco.AddFeature( new DiscoFeature( agsXMPP.Uri.CLIENT ) ) ;
						disco.AddFeature( new DiscoFeature( agsXMPP.Uri.VCARD ) ) ;
						disco.AddFeature( new DiscoFeature( agsXMPP.Uri.CHATSTATES ) ) ;
						disco.AddFeature( new DiscoFeature( agsXMPP.Uri.COMMANDS ) ) ;
						
						XmppConnection.Send( iq );
                    }                	

                }
			}
		}

		void _xmppConnection_OnRegistered( object sender )
		{
		}

		void _xmppConnection_OnRegisterInformation( object sender, RegisterEventArgs args )
		{
		}

		private ShowType _nonIdlePresence = ShowType.NONE ;
		private bool _isIdle = false ;

		private void _idleTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			TimeSpan timeSpan = new TimeSpan( 0, 0, 0, 0, ( int )Win32.GetIdleTime() ) ;

			if ( timeSpan.TotalMinutes > Settings.Default.Client_IdleMinutesAway )
			{
				SetMyPresence( ShowType.away, true ) ;
			}
			else if ( timeSpan.TotalMinutes > Settings.Default.Client_IdleMinutesXA )
			{
				SetMyPresence( ShowType.xa, true ) ;
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

		void _xmppConnection_OnClose( object sender )
		{
			_presence = null ;
			_myRosterItem = null ;

			NotifyPropertyChanged( "MyPresence" ) ;
			NotifyPropertyChanged( "StatusTemplate" ) ;
			NotifyPropertyChanged( "IsAvailable" ) ;
			NotifyPropertyChanged( "MyRosterItem" ) ;
		}

		void XmppConnection_OnXmppConnectionStateChanged( object sender, XmppConnectionState state )
		{
			App.Instance.Window.Status( state.ToString() ) ;
		}

		private void _xmppConnection_OnSocketError( object sender, Exception ex )
		{
			App.Instance.Window.AlertError( "Network error", ex.Message ) ;

			if ( string.IsNullOrEmpty( Settings.Default.Client_Server ) )
			{
				if ( LoginError != null )
				{
					LoginError() ;
				}
			}
		}

		private bool ClientSocket_OnValidateCertificate( object sender, X509Certificate certificate, X509Chain chain,
		                                                 SslPolicyErrors sslPolicyErrors )
		{
			return true ;
		}

		void XmppConnection_OnAuthError( object sender, Element e )
		{
			App.Instance.Window.AlertError( "Authorization failure", "Check your user name and password" ) ;

			if ( XmppConnection.XmppConnectionState != XmppConnectionState.Disconnected )
			{
				XmppConnection.Close() ;
			}

			if ( LoginError != null )
			{
				LoginError() ;
			}
		}

		void XmppConnection_OnXmppError( object sender, Element e )
		{
			App.Instance.Window.AlertError( "Protocol error", e.ToString() ) ;
		}

		void XmppConnection_OnMessage( object sender, Message msg )
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

		public void Connect( bool registerNewAccount )
		{
			if ( XmppConnection.XmppConnectionState == XmppConnectionState.Disconnected )
			{
				Log( "Opening connection" ) ;
				
				XmppConnection.RegisterAccount = registerNewAccount ;

				XmppConnection.Username = Settings.Default.Client_UserName ;
				XmppConnection.Password = Settings.Default.Client_Password ;
				XmppConnection.Server = Settings.Default.Client_Server ;

				Settings.Default.Save();

				XmppConnection.Open() ;
			}
		}

		public void Disconnect()
		{
			if ( XmppConnection.XmppConnectionState != XmppConnectionState.Disconnected )
			{
				Log( "Disconnecting" ) ;

				XmppConnection.Close() ;
			}
		}

		public void SendChatState( RosterItem rosterItem, Chatstate chatState )
		{
			if ( rosterItem.IsInitialized && rosterItem.SupportsChatNotification )
			{
				Message message = new Message() ;

				message.Type = MessageType.chat ;
				message.To = rosterItem.XmppRosterItem.Jid ;
				message.From = MyJid ;
				message.Chatstate = chatState ;

				Trace.WriteLine( message.Thread );

				XmppConnection.Send( message ) ;
			}
		}

		private RosterItem _myRosterItem = null ;

		public RosterItem MyRosterItem
		{
			get
			{
				return _myRosterItem ;
			}

			set
			{
				_myRosterItem = value ;
				NotifyPropertyChanged( "MyRosterItem" ) ;
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
				message.Chatstate = Chatstate.active ;

				Trace.WriteLine( message.Thread );

				XmppConnection.Send( message ) ;

				ChatMessage chatMessage = new ChatMessage( message, rosterItem, DateTime.Now );

				Database database = new Database();
				chatMessage.Id = database.InsertMessage( chatMessage ) ;

				lock ( rosterItem.Messages._syncObject )
				{
					rosterItem.Messages.Add( chatMessage ) ;
				}
			}
		}

		public void SetMyPresence( ShowType showType, bool isIdle )
		{
			if ( !IsAvailable && isIdle )
			{
				return ;
			}

			Connect( false ) ;

			if ( XmppConnection.Authenticated )
			{
				if ( isIdle && !_isIdle )
				{
					// coming to idle state
					_nonIdlePresence = XmppConnection.Show ;
					_isIdle = true ;

					App.Instance.Window.Status( "Coming to Idle State" ) ;
				}

				if ( !isIdle && _isIdle )
				{
					// coming from idle state
					_isIdle = false ;

					App.Instance.Window.Status( "Coming from Idle State" ) ;
				}

				XmppConnection.Show = showType ;
				XmppConnection.SendMyPresence() ;

				_presence = new Presence( XmppConnection.Show, XmppConnection.Status, XmppConnection.Priority ) ;

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
			if ( IsAvailable )
			{
				PresenceManager presenceManager = new PresenceManager( XmppConnection ) ;

				if ( approve )
				{
					presenceManager.ApproveSubscriptionRequest( jid ) ;
				}
				else
				{
					presenceManager.RefuseSubscriptionRequest( jid ) ;
				}
			}
		}

		public void DiscoverServer()
		{
			lock ( _services.Items._syncObject )
			{
				_services.Items.Clear() ;
			}

			DiscoManager discoManager = new DiscoManager( XmppConnection ) ;
			discoManager.DisoverItems( new Jid( XmppConnection.Server ), new IqCB( OnDiscoServerResult ), null ) ;
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

			XmppConnection.IqGrabber.SendIq( registerIq, OnUnregisterService, registerIq ) ;
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
				XmppConnection.IqGrabber.Remove( _idRegisterServiceToken ) ;
				_idRegisterServiceToken = String.Empty ;
			}

			_idRegisterServiceToken = registerIq.Id ;
			XmppConnection.IqGrabber.SendIq( registerIq, OnRegisterServiceGet, _idRegisterServiceToken ) ;
		}

		public void FinishRegisterService( Jid jid, string userName, string password )
		{
			RegisterIq registerIq = new RegisterIq( IqType.set, jid ) ;
			registerIq.Query.Username = userName ;
			registerIq.Query.Password = password ;

			XmppConnection.IqGrabber.SendIq( registerIq, OnRegisterService, registerIq ) ;
		}

		public void DiscoRequest( ServiceItem serviceItem )
		{
			DiscoManager dm = new DiscoManager( XmppConnection ) ;
			dm.DisoverInformation( serviceItem.Jid, new IqCB( OnDiscoInfoResult ), serviceItem ) ;
		}

		public void DiscoRequest( RosterItem rosterItem )
		{
			DiscoManager dm = new DiscoManager( XmppConnection ) ;
			dm.DisoverInformation( new Jid( rosterItem.Key ), new IqCB( OnDiscoInfoResult ), rosterItem ) ;
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

			DiscoManager dm = new DiscoManager( XmppConnection ) ;

			foreach ( ServiceItem itm in serviceItems )
			{
				Thread.Sleep( 50 ) ;

				if ( itm.Jid != null )
				{
					if ( !IsAvailable )
					{
						return ;
					}

					try
					{
						dm.DisoverInformation( itm.Jid, new IqCB( OnDiscoInfoResult ), itm ) ;
					}

					catch ( Exception e )
					{
						Log( "Error discovering roster items: {0}", e.Message ) ;
					}
				}
			}
		}

		private void OnDiscoInfoResult( object sender, IQ iq, object data )
		{
			ServiceItem item = data as ServiceItem ;

			if ( item != null )
			{
				if ( iq.Type == IqType.result && iq.Query is DiscoInfo )
				{
					DiscoInfo di = iq.Query as DiscoInfo ;
					item.Disco = di ;

					lock ( Roster.Items._syncObject )
					{
						foreach ( RosterItem contactRosterItem in Roster.Items )
						{
							if ( contactRosterItem.IsInitialized
									&& !contactRosterItem.IsService
									&& contactRosterItem.XmppRosterItem.Jid.Server
										== item.Jid.Server )
							{
								contactRosterItem.Transport = item.Type ;
							}
						}
					}

					if ( di.HasFeature( agsXMPP.Uri.BYTESTREAMS ) )
					{
						
					}
				}
			}

			RosterItem rosterItem = data as RosterItem ;
			
			if ( rosterItem != null )
			{
				if ( iq.Type == IqType.result && iq.Query is DiscoInfo )
				{
					DiscoInfo di = iq.Query as DiscoInfo ;
					rosterItem.Disco = di ;
				}
			}
		}

		#endregion

		private void RegisterEvents()
		{
			Log( "Registering events" ) ;

			XmppConnection.OnLogin += new ObjectHandler( _xmppConnecion_OnLogin ) ;

			_roster.RegisterEvents( XmppConnection ) ;
		}

		public void RequestAgents()
		{
			XmppConnection.RequestAgents() ;
		}

		public void Send( IQ iq )
		{
			if ( XmppConnection.Binded )
			{
				XmppConnection.Send( iq ) ;
			}
		}

		public void SendIqGrabber( IQ iq, IqCB iqCallback, object cbArgument )
		{
			if ( XmppConnection.Binded )
			{
				XmppConnection.IqGrabber.SendIq( iq, iqCallback, cbArgument ) ;
			}
		}

		public RosterManager RosterManager
		{
			get
			{
				return XmppConnection.RosterManager ;
			}
		}

		public MessageCenter MessageCenter
		{
			get
			{
				if ( _messageCenter == null )
				{
					_messageCenter = new MessageCenter() ;
				}

				return _messageCenter ;
			}
		}

		private void _xmppConnecion_OnLogin( object sender )
		{
			Roster.AskForVCard( MyJid.Bare ) ;

			DiscoverServer() ;
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
				if ( XmppConnection != null )
				{
					return XmppConnection.MyJID ;
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

				XmppConnection.RosterManager.AddRosterItem( jid ) ;

				// Ask for subscription now
				XmppConnection.PresenceManager.Subcribe( jid ) ;
			}
		}

		public void SetRosterGropup( RosterItem rosterItem, string group )
		{
			if ( rosterItem.IsInitialized && !rosterItem.IsService )
			{
				XmppConnection.RosterManager.UpdateRosterItem( rosterItem.XmppRosterItem.Jid,
				                                                rosterItem.NickName, group ) ;
			}
		}

		public PresenceManager PresenceManager
		{
			get
			{
				return XmppConnection.PresenceManager ;
			}
		}

		public Event Event
		{
			get
			{
				return _event ;
			}
		}

		public XmppClientConnection XmppConnection
		{
			get
			{
				return _xmppConnection ;
			}
		}
	}
}