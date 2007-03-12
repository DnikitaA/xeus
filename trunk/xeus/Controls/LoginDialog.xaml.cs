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
	/// Interaction logic for LoginDialog.xaml
	/// </summary>

	public partial class LoginDialog
	{
		public LoginDialog()
		{
			InitializeComponent();

			EnableOk() ;
		}
	
		protected void Ok( object sender, EventArgs e )
		{
			 DialogResult = true ;
		}

		public bool RegisterAccount
		{
			get
			{
				return _expanderNewAccount.IsExpanded ;
			}
		}

		public string Password
		{
			get
			{
				return _password.Password ;
			}
		}

		void EnableOk()
		{
			if ( _expanderNewAccount.IsExpanded
					&& ( _password.Password.Trim() == String.Empty
						|| _password.Password != _confirmPassword.Password ) )
			{
				_ok.IsEnabled = false ;
				return ;
			}

			_ok.IsEnabled = true ;
		}

		protected void OnPasswordCheck( object sender, RoutedEventArgs eventArgs )
		{
			EnableOk() ;
		}
	}
}