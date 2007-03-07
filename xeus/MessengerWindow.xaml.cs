using System ;
using System.ComponentModel ;
using System.Windows ;
using System.Windows.Controls.Primitives ;
using System.Windows.Forms ;
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

		public MessengerWindow()
		{
			InitializeComponent() ;

			_trayIcon.NotifyIcon.MouseClick += new MouseEventHandler( _notifyIcon_MouseClick ) ;

			Client.Instance.MessageCenter.ChatMessages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( ChatMessages_CollectionChanged );
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
			
		}

		public void AlertError( string text )
		{
			_trayIcon.AlertError( text ) ;
		}

		public void AlertInfo( string text )
		{
			_trayIcon.AlertInfo( text ) ;
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

		public void AddUser( object sender, RoutedEventArgs e )
		{
			AddUser addUser = new AddUser();

			addUser.Owner = this ;
			addUser.ShowDialog() ;

			if ( addUser.DialogResult.HasValue && addUser.DialogResult.Value )
			{
				Client.Instance.AddUser( addUser.Jid ) ;
			}
		}

		private void buttonMessages_Click( object sender, RoutedEventArgs e )
		{
			MessageWindow.DisplayChatWindow( null, false ) ;
		}

		protected override void OnClosing( CancelEventArgs e )
		{
			SaveData() ;

			base.OnClosing( e ) ;

			MessageWindow.CloseWindow() ;
			ServicesWindow.CloseWindow();
		}

		private void SaveData()
		{
			Database database =  new Database();
			database.StoreRosterItems( Client.Instance.Roster.Items ) ;
			database.Save() ;
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e ) ;

			Settings.Default.Save();

			_trayIcon.Dispose() ;
		}
	}
}