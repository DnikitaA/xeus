using System.Windows ;
using System.Windows.Controls ;

namespace xeus.Controls
{
	partial class TabMessages
	{
		public void OnSendClick(object sender, RoutedEventArgs e)
		{
			MessageWindow.SendMessage();
			e.Handled = true ;
		}
	}
}