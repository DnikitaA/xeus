using System ;
using System.ComponentModel ;
using System.Windows ;
using System.Windows.Controls.Primitives ;
using System.Windows.Forms ;
using System.Windows.Threading ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.register ;
using xeus.Controls ;
using xeus.Core ;
using xeus.Properties ;
using Button=System.Windows.Controls.Button;
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

		private delegate void ManualLoginCallback() ;
		private delegate void SetStatusCallback( string text ) ;
		private delegate void OnRegisterCallback( IQ iq, Register register ) ;

		public MessengerWindow()
		{
			Initialized += new EventHandler( MessengerWindow_Initialized );

			InitializeComponent() ;

			_trayIcon.NotifyIcon.MouseClick += new MouseEventHandler( _notifyIcon_MouseClick ) ;
		}

		void MessengerWindow_Initialized( object sender, EventArgs e )
		{
			Client.Instance.MessageCenter.ChatMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( ChatMessages_CollectionChanged );
			Client.Instance.MessageCenter.HedlineMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( HedlineMessages_CollectionChanged );
			Client.Instance.LoginError += new Client.LoginHandler( OnLoginError );
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
			ManualLogin() ;
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
			_trayIcon.AlertError( title, text ) ;
		}

		public void AlertInfo( string title, string text )
		{
			_trayIcon.AlertInfo( title, text ) ;
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

		public void AddUser()
		{
			if ( Client.Instance.IsAvailable )
			{
				string userName = SingleValueDialog.AddUserDialog( this ) ;

				if ( !string.IsNullOrEmpty( userName ) )
				{
					Client.Instance.AddUser( userName ) ;
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

			base.OnClosing( e ) ;

			MessageWindow.CloseWindow() ;
			ServicesWindow.CloseWindow();
		}

		private void SaveData()
		{
			Database database =  new Database();
			
			lock ( Client.Instance.Roster.Items._syncObject )
			{
				database.StoreRosterItems( Client.Instance.Roster.Items ) ;
			}

			database.StoreGroups( _roster.ExpanderStates ) ;
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e ) ;

			_trayIcon.Dispose() ;
		}
	}
}