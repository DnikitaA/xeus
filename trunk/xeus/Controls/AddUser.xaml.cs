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

	public partial class AskForSingleValue : WindowBase
	{

		public AskForSingleValue( string title, string text )
		{
			InitializeComponent();

			_title.Text = title ;
			_titleAdd.Text = text ;
			_jid.Focus() ;
		}

		public string Value
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