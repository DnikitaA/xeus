using System ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Controls.Primitives ;
using System.Windows.Input ;
using xeus.Controls ;
using xeus.Core ;

namespace xeus
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class MessengerWindow : Window
	{
		public MessengerWindow()
		{
			InitializeComponent() ;
		}

		protected override void OnInitialized( EventArgs e )
		{
			base.OnInitialized( e ) ;

			DataContext = Client.Instance ;

			Client.Instance.Setup() ;
			Client.Instance.Connect() ;

			Button buttonMessages = _statusBar.FindName( "_buttonMessages" ) as Button ;

			buttonMessages.Click += new RoutedEventHandler( buttonMessages_Click ) ;
		}

		public void DisplayPopup( object sender, RoutedEventArgs e )
		{
			_statusPopup.IsOpen = true ;
		}

		private void buttonMessages_Click( object sender, RoutedEventArgs e )
		{
			MessageWindow.Instance.DisplayAllChats() ;
		}

		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			/*IInputElement iie = InputHitTest( e.GetPosition( this ) ) ;
			if ( iie is Canvas )*/
			base.OnMouseLeftButtonDown( e ) ;
			DragMove() ;
		}
	}
}