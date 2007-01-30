using System.Windows ;
using System.Windows.Threading ;
using xeus.Core ;

namespace xeus
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public static Dispatcher DispatcherThred
		{
			get
			{
				return _theApp.Dispatcher ;
			}
		}

		protected static App _theApp ;

		public static App Instance
		{
			get
			{
				return _theApp ;
			}
		}

		protected override void OnStartup( StartupEventArgs e )
		{
			_theApp = this ;

			base.OnStartup( e ) ;

			MessengerWindow messengerWindow = new MessengerWindow() ;
			messengerWindow.Show() ;
		}

		protected override void OnExit( ExitEventArgs e )
		{
			Client.Instance.Disconnect() ;

			base.OnExit( e ) ;
		}
	}
}