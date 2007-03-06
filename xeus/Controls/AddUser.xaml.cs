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
	/// Interaction logic for AddUser.xaml
	/// </summary>

	public partial class AddUser : WindowBase
	{

		public AddUser()
		{
			InitializeComponent();
		}

		public string Jid
		{
			get
			{
				return _jid.Text ;
			}
		}

		protected void Ok( object sender, EventArgs e )
		{
			 DialogResult = true ;
		}
	}
}