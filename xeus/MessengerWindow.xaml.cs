using System ;
using System.Windows ;
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
		}
	}
}