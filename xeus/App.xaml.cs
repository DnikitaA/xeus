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
		private MessengerWindow _window ;

		public MessengerWindow Window
		{
			get
			{
				return _window ;
			}
		}

		public static Dispatcher DispatcherThread
		{
			get
			{
				if ( _theApp == null )
				{
					return null ;
				}

				return _theApp.Dispatcher ;
			}
		}

		protected static App _theApp ;

		public App()
		{
			_theApp = this ;
		}

		public static App Instance
		{
			get
			{
				return _theApp ;
			}
		}

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e ) ;

			_window = new MessengerWindow() ;
			_window.Show() ;

			Client.Instance.Setup() ;
			Client.Instance.Connect( false ) ;
		}

		protected override void OnExit( ExitEventArgs e )
		{
			Client.Instance.Disconnect() ;

			base.OnExit( e ) ;
		}
	}
}