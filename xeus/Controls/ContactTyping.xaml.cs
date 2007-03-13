using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation ;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using agsXMPP.protocol.extensions.chatstates ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for ContactTyping.xaml
	/// </summary>

	public partial class ContactTyping : System.Windows.Controls.UserControl
	{
		public ContactTyping()
		{
			InitializeComponent();
		}

		public string UserName
		{
			set
			{
				_userName.Text = value ;
			}
		}

		public Chatstate Chatstate
		{
			set
			{
				switch ( value )
				{
					case Chatstate.active:
						{
							Opacity = 0.5 ;
							Visibility = Visibility.Visible ;
							break ;
						}
					case Chatstate.composing:
						{
							Opacity = 1.0 ;
							Visibility = Visibility.Visible ;
							break ;
						}
					default:
						{
							Visibility = Visibility.Hidden ;
							break ;
						}
				}
			}
		}
	}
}