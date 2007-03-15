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
	/// Interaction logic for AddUser.xaml
	/// </summary>

	public partial class SingleValueDialog : WindowBase
	{
		public static string AddUserDialog( Window owner, string text )
		{
			return Dialog( owner, "New User Name", Storage.GetDefaultAvatar(), text ) ;
		}

		public static string AddGroupDialog( Window owner, string text )
		{
			return Dialog( owner, "Name of the new Group", null, text ) ;
		}

		public static string ContactNameDialog( Window owner, BitmapImage image, string text )
		{
			return Dialog( owner, "Contact Custom Name", image, text ) ;
		}

		static string Dialog( Window owner, string text, BitmapImage image, string textToTextBox )
		{
			SingleValueDialog singleValueDialog = new SingleValueDialog( text ) ;

			singleValueDialog.Image.Source = image ;
			singleValueDialog.Owner = owner ;
			singleValueDialog._value.Text = textToTextBox ;

			if ( singleValueDialog._value.Text.Length > 0 )
			{
				singleValueDialog._value.SelectAll();
			}

			singleValueDialog.ShowDialog() ;

			if ( singleValueDialog.DialogResult.HasValue && singleValueDialog.DialogResult.Value )
			{
				return singleValueDialog.Value ;
			}
			else
			{
				return null ;
			}
		}

		public SingleValueDialog( string text )
		{
			InitializeComponent();

			_titleAdd.Text = text ;
			_value.Focus() ;
		}

		public string Value
		{
			get
			{
				return _value.Text ;
			}
		}

		public Image Image
		{
			get
			{
				return _image ;
			}
		}

		protected void Ok( object sender, EventArgs e )
		{
			 DialogResult = true ;
		}
	}
}