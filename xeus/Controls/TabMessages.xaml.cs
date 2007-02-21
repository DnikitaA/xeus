using System ;
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


		public void TextBoxInitialized( object sender, EventArgs e )
		{
			MessageWindow.MessageTextBox = sender as TextBox ;
		}

		public void ListBoxInitialized( object sender, EventArgs e )
		{
			MessageWindow.MessageListBox = sender as ListBox ;
		}

	}
}