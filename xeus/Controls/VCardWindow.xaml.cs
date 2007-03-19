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
			VCardWindow vcardWindow = new VCardWindow() ;

			vcardWindow.DataContext = rosterItem ;

			vcardWindow._image.Source = rosterItem.Image ;
			_windows.Add( vcardWindow );

			vcardWindow.Show();
		}

		protected void Ok( object sender, EventArgs e )
		{
			 DialogResult = true ;
		}
	}
}