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

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for VCardWindow.xaml
	/// </summary>

	public partial class VCardWindow
	{
		public VCardWindow()
		{
			InitializeComponent();
		}

		protected void Ok( object sender, EventArgs e )
		{
			 DialogResult = true ;
		}
	}
}