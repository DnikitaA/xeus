using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives ;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using agsXMPP.protocol.client ;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for StatusControl.xaml
	/// </summary>

	public partial class StatusControl : System.Windows.Controls.UserControl
	{
		public StatusControl()
		{
			InitializeComponent();
		}

		public void OnStatusOnline( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.NONE, false );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		private void CloseParentPopup()
		{
			System.Windows.Controls.Primitives.Popup popup = Parent as System.Windows.Controls.Primitives.Popup ;

			if ( popup != null )
			{
				popup.IsOpen = false ;
			}
		}

		public void OnStatusOffline( object sender, RoutedEventArgs e )
		{
			Client.Instance.Disconnect();
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusDnd( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.dnd, false );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusAway( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.away, false );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusXAway( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.xa, false );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusFreeForChat( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.chat, false );
			e.Handled = true ;
			CloseParentPopup() ;
		}
	}
}