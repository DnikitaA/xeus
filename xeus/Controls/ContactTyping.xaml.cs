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
		private Storyboard _storyboard = null ;

		public ContactTyping()
		{
			InitializeComponent();

			_storyboard = Resources[ "TimeLineWriteText" ] as Storyboard ;
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
							_storyboard.Stop( this );
							Visibility = Visibility.Visible ;
							break ;
						}
					case Chatstate.composing:
						{
							_storyboard.Begin( this, true );
							break ;
						}
					default:
						{
							_storyboard.Stop( this );
							Visibility = Visibility.Hidden ;
							break ;
						}
				}
			}
		}
	}
}