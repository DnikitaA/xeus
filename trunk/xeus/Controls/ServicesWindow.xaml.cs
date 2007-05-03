using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading ;
using agsXMPP.protocol.iq.disco ;
using xeus.Core ;

namespace xeus.Controls
{
	public partial class ServicesWindow : WindowBase
	{
		private static ServicesWindow _instance ;

		private delegate void DisplayCallback() ;

		public ServicesWindow()
		{
			InitializeComponent();

			DataContext = Client.Instance  ;

			_services.SelectionChanged += new SelectionChangedEventHandler( _services_SelectionChanged );
		}

		void _services_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			ServiceItem serviceItem = _services.SelectedItem as ServiceItem ;

			if ( serviceItem != null && serviceItem.Disco == null )
			{
				Client.Instance.DiscoRequest( serviceItem );
			}
		}

		public static ServicesWindow Instance
		{
			get
			{
				return _instance ;
			}
		}

		protected override void OnClosed( EventArgs e )
		{
			_instance = null ;

			base.OnClosed( e ) ;
		}

		public static bool IsOpen()
		{
			return ( _instance != null ) ;
		}

		public static void CloseWindow()
		{
			if ( _instance != null )
			{
				_instance.Close() ;
			}
		}

		public static void DisplayServices()
		{
			if ( App.Current.Dispatcher.CheckAccess() )
			{
				if ( _instance == null )
				{
					_instance = new ServicesWindow() ;
				}

				Client.Instance.DiscoverServer() ;

				_instance.Show() ;
			}
			else
			{
				App.Current.Dispatcher.BeginInvoke( DispatcherPriority.Normal,
				                                  new DisplayCallback( DisplayServices ) ) ;
			}
		}
	}
}