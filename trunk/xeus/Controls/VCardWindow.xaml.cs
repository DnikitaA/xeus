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
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for VCardWindow.xaml
	/// </summary>

	public partial class VCardWindow
	{
		static List< VCardWindow > _windows = new List< VCardWindow >();

		public static void CloseAllWindows()
		{
			VCardWindow [] vCardWindows = new VCardWindow[ _windows.Count ];
			_windows.CopyTo( vCardWindows, 0 );

			foreach ( VCardWindow vCardWindow in vCardWindows )
			{
				vCardWindow.Close();
			}

			_windows.Clear();
		}

		public VCardWindow()
		{
			InitializeComponent();
		}

		internal static void ShowWindow( RosterItem rosterItem )
		{
			foreach ( VCardWindow vCardWindow in _windows )
			{
				if ( ( ( RosterItem )vCardWindow.DataContext ).Key == rosterItem.Key )
				{
					vCardWindow.Activate() ;
					return ;
				}
			}

			VCardWindow vcardWindow = new VCardWindow() ;

			vcardWindow.DataContext = rosterItem ;

			vcardWindow._image.Source = rosterItem.Image ;
			_windows.Add( vcardWindow );

			vcardWindow.Show();
		}

		protected override void OnClosed( EventArgs e )
		{
			base.OnClosed( e );

			_windows.Remove( this ) ;
		}

		protected void OnPublish( object sender, EventArgs e )
		{
		}

		protected void OnClose( object sender, EventArgs e )
		{
			 Close();
		}
	}
}