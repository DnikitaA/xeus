using System.Windows ;
using System.Windows.Controls ;
using agsXMPP.protocol.client ;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for MessageWindow.xaml
	/// </summary>
	public partial class MessageWindow : Window
	{
		private static MessageWindow _instance = new MessageWindow() ;

		public MessageWindow()
		{
			InitializeComponent() ;
		}

		public static MessageWindow Instance
		{
			get
			{
				return _instance ;
			}
		}

		TabItem FindTab( string jid )
		{
			foreach ( TabItem tab in _tabs.Items )
			{
				if ( ( string )tab.Tag == jid )
				{
					return tab ;
				}
			}

			return null ;
		}

		public void DisplayChat( string jid )
		{
			TabItem tab =  FindTab( jid ) ;

			if ( tab == null )
			{
				tab = new TabItem();
				tab.Tag = jid ;
				tab.DataContext = Client.Instance.Roster.FindItem( jid ) ;

				_tabs.Items.Add( tab ) ;
			}
		}

		public void DisplayAllChats()
		{
			foreach ( Message message in Client.Instance.MessageCenter.ChatMessages )
			{
				DisplayChat( message.From.Bare ) ;
			}

			if ( !IsVisible )
			{
				Show();
			}
		}
	}
}