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
		private NotifyIcon _notifyIcon ;
		private ToolTip _toolTip = new ToolTip() ;

		public MessengerWindow()
		{
			_notifyIcon = new NotifyIcon();
			InitializeComponent() ;

			_notifyIcon.Icon = Properties.Resources.xeus ;
			_notifyIcon.Visible = true ;
			_notifyIcon.MouseClick += new MouseEventHandler( _notifyIcon_MouseClick ) ;
		}

		private void _notifyIcon_MouseClick( object sender, MouseEventArgs e )
		{
			if ( e.Button == MouseButtons.Left )
			{
				if ( WindowState == WindowState.Minimized )
				{
					if ( !ShowInTaskbar )
					{
						Show();
					}

					WindowState = WindowState.Normal ;
				}
				else
				{
					WindowState = WindowState.Minimized ;

					if ( !ShowInTaskbar )
					{
						Hide();
					}
				}
			}
		}

		public void Status( string text)
		{
			
		}

		public void Alert( string text )
		{
			_notifyIcon.ShowBalloonTip( 500, "Connection error", text, ToolTipIcon.Error ) ;
		}

		protected override void OnInitialized( EventArgs e )
		{
			base.OnInitialized( e ) ;

			_roster.InlineSearch = _inlineSearch ;

			DataContext = Client.Instance ;

			Button buttonMessages = _statusBar.FindName( "_buttonMessages" ) as Button ;

			buttonMessages.Click += new RoutedEventHandler( buttonMessages_Click ) ;

			_notifyIcon.Visible = true ;
			_notifyIcon.Text = "xeus" ;
		}

		public void DisplayPopup( object sender, RoutedEventArgs e )
		{
			_statusPopup.IsOpen = true ;
		}

		public void OpenServices( object sender, RoutedEventArgs e )
		{
			ServicesWindow.DisplayServices() ;
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

			_notifyIcon.Dispose() ;
		}
	}
}