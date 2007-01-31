using System ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Controls.Primitives ;
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
			Client.Instance.Connect();

			Button buttonMessages = _statusBar.FindName( "_buttonMessages" ) as Button  ;

			if ( buttonMessages != null )
			{
				buttonMessages.Click += new RoutedEventHandler( buttonMessages_Click ) ;
			}
		}

		void buttonMessages_Click( object sender, RoutedEventArgs e )
		{
			MessageWindow.Instance.DisplayAllChats();
		}
	}
}