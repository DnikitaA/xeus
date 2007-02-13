using System ;
using System.Windows ;
using System.Windows.Forms ;
using System.Windows.Input ;
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

		NotifyIcon _notifyIcon = new NotifyIcon();
 
		public MessengerWindow()
		{
			InitializeComponent() ;
		}

		public void Alert( string text )
		{
			_notifyIcon.ShowBalloonTip( 500, "Connection error", text, new ToolTipIcon() );
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

		private void buttonMessages_Click( object sender, RoutedEventArgs e )
		{
			MessageWindow.DisplayAllChats() ;
		}

		/*
		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			IInputElement iie = InputHitTest( e.GetPosition( this ) ) ;
			if ( iie is Canvas )
			base.OnMouseLeftButtonDown( e ) ;
			DragMove() ;
		}*/

		protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
		{
			base.OnClosing( e );

			MessageWindow.CloseWindow();
		}
	}
}