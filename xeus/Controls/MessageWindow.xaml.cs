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
			RosterItem rosterItem = Client.Instance.Roster.FindItem( jid ) ;

			if ( tab == null )
			{
				tab = new TabItem();
				tab.DataContext = rosterItem ;

				_tabs.Items.Add( tab ) ;
			}

			Client.Instance.MessageCenter.MoveUnreadMessagesToRosterItem( rosterItem );
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