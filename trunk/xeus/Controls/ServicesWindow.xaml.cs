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
using xeus.Core ;

namespace xeus.Controls
{
	public partial class ServicesWindow : WindowBase
	{
		private static ServicesWindow _instance ;

		private delegate void DisplayServicesCallback() ;

		public ServicesWindow()
		{
			InitializeComponent();

			DataContext = Client.Instance  ;
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
			if ( App.DispatcherThread.CheckAccess() )
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
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new DisplayServicesCallback( DisplayServices ) ) ;
			}
		}
	}
}