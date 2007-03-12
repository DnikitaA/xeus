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
		public static string AddUserDialog( Window owner )
		{
			return Dialog( owner, "New User Name", Storage.GetDefaultAvatar() ) ;
		}

		public static string AddGroupDialog( Window owner )
		{
			return Dialog( owner, "Name of the new Group", null ) ;
		}

		public static string ContactNameDialog( Window owner, BitmapImage image )
		{
			return Dialog( owner, "Contact Custom Name", image ) ;
		}

		static string Dialog( Window owner, string text, BitmapImage image )
		{
			SingleValueDialog singleValueDialog = new SingleValueDialog( text ) ;

			singleValueDialog.Image.Source = image ;
			singleValueDialog.Owner = owner ;
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