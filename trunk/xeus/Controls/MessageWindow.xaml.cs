using System.Collections.Generic ;
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

		protected override void OnClosing( System.ComponentModel.CancelEventArgs e )
		{
			Hide() ;
			e.Cancel = true ;
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
			TabItem tab = FindTab( jid ) ;
			RosterItem rosterItem = Client.Instance.Roster.FindItem( jid ) ;

			if ( tab == null )
			{
				tab = new TabItem();
				tab.Content = rosterItem ;
				tab.Tag = jid ;

				_tabs.Items.Add( tab ) ;
			}

			Client.Instance.MessageCenter.MoveUnreadMessagesToRosterItem( rosterItem );
		}

		public void DisplayAllChats()
		{
			List< string > recievers = new List< string >( Client.Instance.MessageCenter.ChatMessages.Count );
			
			foreach ( Message message in Client.Instance.MessageCenter.ChatMessages )
			{
				recievers.Add( message.From.Bare );
			}

			foreach ( string jid in recievers )
			{
				DisplayChat( jid ) ;
			}

			if ( !IsVisible )
			{
				Show();
			}
		}
	}
}