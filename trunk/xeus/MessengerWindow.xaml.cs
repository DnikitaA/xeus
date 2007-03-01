using System ;
using System.ComponentModel ;
using System.Drawing ;
using System.Windows ;
using System.Windows.Forms ;
using xeus.Controls ;
using xeus.Core ;
using Button=System.Windows.Controls.Button;

namespace xeus
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MessengerWindow : WindowBase
	{
		private NotifyIcon _notifyIcon = new NotifyIcon() ;

		public MessengerWindow()
		{
			InitializeComponent() ;

			_notifyIcon.Icon = Properties.Resources.xeus ;
			_notifyIcon.Visible = true ;

			_notifyIcon.MouseClick += new MouseEventHandler( _notifyIcon_MouseClick );
		}

		void _notifyIcon_MouseClick( object sender, MouseEventArgs e )
		{
			if ( e.Button == MouseButtons.Left )
			{
				if ( WindowState == WindowState.Minimized )
				{
					WindowState = WindowState.Normal ;
				}
				else
				{
					WindowState = WindowState.Minimized ;
				}
			}
		}

		public void Alert( string text )
		{
			_notifyIcon.ShowBalloonTip( 500, "Connection error", text, new ToolTipIcon() ) ;
		}

		protected override void OnInitialized( EventArgs e )
		{
			base.OnInitialized( e ) ;

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
			ServicesWindow.DisplayServices();
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
		}

		private void SaveData()
		{
			Database.Instance.StoreRosterItems( Client.Instance.Roster.Items ) ;
			Database.Instance.Save() ;
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			_notifyIcon.Dispose();
		}
	}
}