using System ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Input ;

namespace xeus.Controls
{
	partial class TabMessages
	{
		public void OnSendClick( object sender, RoutedEventArgs e )
		{
			MessageWindow.SendMessage();
			e.Handled = true ;
		}

		public void OnTextKeyDown( object sender, KeyEventArgs e )
		{
			MessageWindow.TextKeyPress( e ) ;
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