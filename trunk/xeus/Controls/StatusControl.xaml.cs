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
			Client.Instance.SetMyPresence( ShowType.NONE );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		private void CloseParentPopup()
		{
			Popup popup = Parent as Popup ;

			if ( popup != null )
			{
				popup.IsOpen = false ;
			}
		}

		public void OnStatusOffline( object sender, RoutedEventArgs e )
		{
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusDnd( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.dnd );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusAway( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.away );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusXAway( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.xa );
			e.Handled = true ;
			CloseParentPopup() ;
		}

		public void OnStatusFreeForChat( object sender, RoutedEventArgs e )
		{
			Client.Instance.SetMyPresence( ShowType.chat );
			e.Handled = true ;
			CloseParentPopup() ;
		}
	}
}