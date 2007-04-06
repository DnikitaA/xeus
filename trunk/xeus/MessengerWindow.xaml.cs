using System ;
using System.ComponentModel ;
using System.Windows ;
using System.Windows.Controls.Primitives ;
using System.Windows.Forms ;
using System.Windows.Input ;
using System.Windows.Threading ;
using agsXMPP ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.register ;
using GeoCore.Win32 ;
using xeus.Controls ;
using xeus.Core ;
using xeus.Properties ;
using Button=System.Windows.Controls.Button;
using Keys=GeoCore.Win32.Keys;
using MouseEventArgs=System.Windows.Forms.MouseEventArgs;
using MouseEventHandler=System.Windows.Forms.MouseEventHandler;
using Point=System.Drawing.Point;
using ToolTip=System.Windows.Controls.ToolTip;

namespace xeus
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MessengerWindow : WindowBase
	{
		private TrayIcon _trayIcon = new TrayIcon() ;

		private WPFHotkeyManager _hotkeyManager ;
		private Hotkey _hotkeyShowMainWindow ;

		xeus.Controls.Popup _popup;

		private delegate void ManualLoginCallback() ;
		private delegate void SetStatusCallback( string text ) ;
		private delegate void OnRegisterCallback( IQ iq, Register register ) ;
		private delegate void PresenceSubscribeCallback( Jid jid ) ;

		public MessengerWindow()
		{
			Database.OpenDatabase() ;

			Initialized += new EventHandler( MessengerWindow_Initialized );
			Loaded += new RoutedEventHandler( MessengerWindow_Loaded );

			InitializeComponent() ;

			_trayIcon.NotifyIcon.MouseClick += new MouseEventHandler( _notifyIcon_MouseClick ) ;
		}

		void MessengerWindow_Loaded( object sender, RoutedEventArgs e )
		{
			_hotkeyManager.Register( _hotkeyShowMainWindow );
		}

		void MessengerWindow_Initialized( object sender, EventArgs e )
		{
			_hotkeyManager = new WPFHotkeyManager( this );
			_hotkeyManager.HotkeyPressed += new WPFHotkeyManager.HotkeyPressedEvent( _hotkeyManager_HotkeyPressed );
			_hotkeyShowMainWindow = new Hotkey( ModifierKeys.Windows, Keys.A );

			_popup = new xeus.Controls.Popup();

			Client.Instance.MessageCenter.ChatMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( ChatMessages_CollectionChanged );
			Client.Instance.MessageCenter.HedlineMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( HedlineMessages_CollectionChanged );
			Client.Instance.LoginError += new Client.LoginHandler( OnLoginError );

			Client.Instance.PropertyChanged += new PropertyChangedEventHandler( Instance_PropertyChanged );
			Client.Instance.Roster.PresenceSubscribe += new Roster.PresenceSubscribeHandler( Roster_PresenceSubscribe );

			_trayIcon.State = TrayIcon.TrayState.Pending ;
			_roster.Focus() ;

			_statusBar.Loaded += new RoutedEventHandler( _statusBar_Loaded );
		}

		void _hotkeyManager_HotkeyPressed( Window window, Hotkey hotkey )
		{
			if ( hotkey.Equals( _hotkeyShowMainWindow ) )
			{
				ShowHide() ;
			}
		}

		void _statusBar_Loaded( object sender, RoutedEventArgs e )
		{
			_inlineSearch.Visibility = Visibility.Collapsed ;
		}

		void Roster_PresenceSubscribe( Jid jid )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				RosterItem rosterItem = Client.Instance.Roster.FindItem( jid.Bare ) ;

				if ( rosterItem != null )
				{
					AuthorizeWindow.ShowWindow( rosterItem ) ;
				}
				else
				{
					AuthorizeWindow.ShowWindow( jid ) ;
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new PresenceSubscribeCallback( Roster_PresenceSubscribe ), jid ) ;
			}
		}

		void Instance_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			if ( e.PropertyName == "IsAvailable" )
			{
				if ( Client.Instance.IsAvailable )
				{
					_trayIcon.State = TrayIcon.TrayState.Normal ;
				}
				else
				{
					_trayIcon.State = TrayIcon.TrayState.Pending ;
				}
			}
		}

		void HedlineMessages_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			if ( e.NewItems.Count > 0 )
			{
				_headlines.Visibility = Visibility.Visible ;
			}
		}

		void CloseHeadlines( object sender, RoutedEventArgs e )
		{
			_headlines.Visibility = Visibility.Collapsed ;
		}

		void ManualLogin()
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				Client.Instance.Disconnect();

				LoginDialog loginDialog = new LoginDialog();

				loginDialog.Owner = this ;
				loginDialog.ShowDialog() ;

				if ( loginDialog.DialogResult.HasValue && loginDialog.DialogResult.Value )
				{
					Settings.Default.Client_Password = loginDialog.Password ;
					Client.Instance.Connect( loginDialog.RegisterAccount ) ;
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new ManualLoginCallback( ManualLogin ) ) ;
			}
		}

		void OnLoginError()
		{
			//ManualLogin() ;
		}

		void ChatMessages_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			if ( Client.Instance.MessageCenter.ChatMessages.Count > 0 )
			{
				_trayIcon.State = TrayIcon.TrayState.NewMessage ;
			}
			else
			{
				_trayIcon.State = TrayIcon.TrayState.Normal ;
			}
		}

		private void _notifyIcon_MouseClick( object sender, MouseEventArgs e )
		{
			switch ( _trayIcon.State )
			{
				case TrayIcon.TrayState.NewMessage:
					{
						MessageWindow.DisplayChatWindow( null, false ) ;

						break ;
					}
				case TrayIcon.TrayState.Normal:
					{
						if ( e.Button == MouseButtons.Left )
						{
							ShowHide() ;
						}

						break ;
					}
			}

		}

		public void Status( string text )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				_statusStatus.Text = text ;
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new SetStatusCallback( Status ), text ) ;
			}
		}

		public void AlertError( string title, string text )
		{
			Client.Instance.Event.AddEvent( new EventError( text, title ) ) ;
		}

		public void AlertInfo( string title, string text )
		{
			Client.Instance.Event.AddEvent( new EventInfo( text, title ) ) ;
		}

		protected override void OnInitialized( EventArgs e )
		{
			base.OnInitialized( e ) ;

			_roster.InlineSearch = _inlineSearch ;

			DataContext = Client.Instance ;

			Button buttonMessages = _statusBar.FindName( "_buttonMessages" ) as Button ;

			buttonMessages.Click += new RoutedEventHandler( buttonMessages_Click ) ;
		}

		public void DisplayPopup( object sender, RoutedEventArgs e )
		{
			_statusPopup.IsOpen = true ;
		}

		public void OpenServices( object sender, RoutedEventArgs e )
		{
			ServicesWindow.DisplayServices() ;
		}

		public void StatusRightClick( object sender, RoutedEventArgs e )
		{
			ManualLogin();
		}

		public void AddUser()
		{
			if ( Client.Instance.IsAvailable )
			{
				string userName = SingleValueDialog.AddUserDialog( this, String.Empty ) ;

				if ( !string.IsNullOrEmpty( userName ) )
				{
					Client.Instance.AddUser( userName ) ;
				}
			}
		}

		void ShowHide()
		{
			if ( WindowState == WindowState.Minimized )
			{
				if ( !ShowInTaskbar )
				{
					Show();
				}

				WindowState = WindowState.Normal;
			}
			else
			{
				WindowState = WindowState.Minimized;

				if ( !ShowInTaskbar )
				{
					Hide();
				}
			}
		}

		public void OpenRegisterDialog( IQ iq, Register register )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( register != null )
				{
					RegisterWindow registerWindow = new RegisterWindow() ;

					registerWindow.ShowDialog() ;

					if ( registerWindow.DialogResult.HasValue && registerWindow.DialogResult.Value )
					{
						Client.Instance.FinishRegisterService( iq.From, registerWindow.UserName, registerWindow.Password ) ;
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new OnRegisterCallback( OpenRegisterDialog ), iq, register ) ;
			}
		}

		private void buttonMessages_Click( object sender, RoutedEventArgs e )
		{
			MessageWindow.DisplayChatWindow( null, false ) ;
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			Settings.Default.Save();
			SaveData() ;

			Database.CloseDatabase();

			base.OnClosing( e ) ;

			_popup.Close();
			AuthorizeWindow.CloseAllWindows();
			MessageWindow.CloseWindow() ;
			ServicesWindow.CloseWindow();
			VCardWindow.CloseAllWindows();
		}

		private void SaveData()
		{
			Database database =  new Database();
			
			database.StoreRosterItems( Client.Instance.Roster.Items ) ;

			database.StoreGroups( _roster.ExpanderStates ) ;
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e ) ;

			_trayIcon.Dispose() ;
		}
	}
}