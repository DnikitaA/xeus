using System ;
using agsXMPP ;
using agsXMPP.net ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.disco ;
using agsXMPP.protocol.iq.roster ;
using agsXMPP.Xml.Dom ;
using xeus.Core ;

namespace xeus.Core
{
	internal class Client : IDisposable
	{
		private static Client _instance = new Client() ;

		XmppClientConnection _xmppConnection = new XmppClientConnection() ;
		
		private Services _services = new Services();
		private Roster _roster = new Roster(); 
		private Agent _agent = new Agent();

		#region delegates

		public delegate void LoginHandler() ;
		public delegate void DiscoFinishHandler() ;

		#endregion

		#region events

		public event LoginHandler LoggedIn ;

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

		public Agent Agent
		{
			get
			{
				return _agent ;
			}
		}

		private Client()
		{
			RegisterEvents() ;
		}

		public void Setup()
		{
			_xmppConnection.Username = "testovani" ;
			_xmppConnection.Password = "hardcore" ;
			_xmppConnection.Server = "jabber.cz" ;
			_xmppConnection.UseCompression = true ;
			_xmppConnection.UseSSL = true ;
			_xmppConnection.AutoResolveConnectServer = true ;

			_xmppConnection.ConnectServer = null ;
			_xmppConnection.Resource = "xeus" ;
			_xmppConnection.SocketConnectionType = SocketConnectionType.Direct ;
			_xmppConnection.UseStartTLS = true ;
			_xmppConnection.AutoRoster = true ;


			_xmppConnection.OnRosterEnd += new ObjectHandler( _xmppConnection_OnRosterEnd );

			Log( "Setup finished" ) ;
		}

		void _xmppConnection_OnRosterEnd( object sender )
		{
			Status = "online" ;
		}

		public void Connect()
		{
			if ( !_xmppConnection.Binded )
			{
				Log( "Opening connection" ) ;

				_xmppConnection.Open() ;
			}
		}

		public void Disconnect()
		{
			if ( _xmppConnection.Binded )
			{
				Log( "Disconnecting" ) ;

				_xmppConnection.Close() ;
			}
		}

		public string Status
		{
			set
			{
				switch ( value )
				{
					case "online":
						{
							_xmppConnection.Show = ShowType.NONE ;
							_xmppConnection.SendMyPresence();
							break ;
						}

					default:
						{
							break ;
						}
				}
			}
		}

		public void SubscribePresence( Jid jid, bool approve )
		{
			PresenceManager presenceManager = new PresenceManager( _xmppConnection );

			if ( approve )
			{
				presenceManager.ApproveSubscriptionRequest( jid ) ;
			}
			else
			{
				presenceManager.RefuseSubscriptionRequest( jid ) ;
			}
		}

		void DiscoverServer()
		{
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

						DiscoManager dm = new DiscoManager( _xmppConnection ) ;

						foreach ( DiscoItem itm in itms )
						{
							if ( itm.Jid != null )
							{
								dm.DisoverInformation( itm.Jid, new IqCB( OnDiscoInfoResult ), itm ) ;
							}
						}
					}
				}
			}

			Log( "Server disco finished" ) ;
		}

		private void OnDiscoInfoResult( object sender, IQ iq, object data )
		{
			if ( iq.Type == IqType.result && iq.Query is DiscoInfo )
			{
				DiscoInfo di = iq.Query as DiscoInfo ;

				if ( di != null )
				{
					Services.Items.Add( new ServiceItem( iq.From.ToString(), iq.From, di ) );
				}
			}
		}

		#endregion

		private void RegisterEvents()
		{
			Log( "Registering events" ) ;

			_xmppConnection.OnLogin += new ObjectHandler( _xmppConnecion_OnLogin ) ;

			_roster.RegisterEvents( _xmppConnection );
			_agent.RegisterEvents( _xmppConnection );
		}

		public void RequestRooster()
		{
			_xmppConnection.RequestRoster();
		}

		public void SendIqGrabber( IQ iq, IqCB iqCallback, object cbArgument )
		{
			_xmppConnection.IqGrabber.SendIq( iq, iqCallback, cbArgument ) ;
		}

		public RosterManager RosterManager
		{
			get
			{
				return _xmppConnection.RosterManager ;
			}
		}

		private void _xmppConnecion_OnLogin( object sender )
		{
			DiscoverServer() ;

			OnLogin() ;
		}

		protected virtual void OnLogin()
		{
			if ( LoggedIn != null )
			{
				LoggedIn() ;
			}
		}

		private void Log( string text, params object [] parameters )
		{
			Console.WriteLine( text, parameters );
		}

		public void Dispose()
		{
			Disconnect() ;
		}
	}
}