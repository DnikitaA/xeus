using System.Collections.Generic ;
using System.Timers ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Threading ;
using agsXMPP.protocol.client ;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for MessageWindow.xaml
	/// </summary>
	public partial class MessageWindow : WindowBase
	{
		private static MessageWindow _instance = new MessageWindow() ;

		public MessageWindow()
		{
			InitializeComponent() ;
		}

		protected override void OnClosed( System.EventArgs e )
		{
			_instance = null ;
			base.OnClosed( e );
		}

		public static void CloseWindow()
		{
			if ( _instance != null )
			{
				_instance.Close();
			}
		}

		static TabItem FindTab( string jid )
		{
			foreach ( TabItem tab in _instance._tabs.Items )
			{
				if ( ( string )tab.Tag == jid )
				{
					return tab ;
				}
			}

			return null ;
		}

		public static void DisplayChat( string jid )
		{
			if ( _instance == null )
			{
				_instance = new MessageWindow();
			}

			TabItem tab = FindTab( jid ) ;
			RosterItem rosterItem = Client.Instance.Roster.FindItem( jid ) ;

			if ( tab == null )
			{
				tab = new TabItem();
				tab.Header = rosterItem ;
				tab.Content = rosterItem ;
				tab.Tag = jid ;

				_instance._tabs.Items.Add( tab ) ;
			}

			if ( rosterItem != null )
			{
				Client.Instance.MessageCenter.MoveUnreadMessagesToRosterItem( rosterItem );

				if ( !_instance.IsVisible )
				{
					_instance.Show() ;
				}
			}
		}

		public static void DisplayAllChats()
		{
			List< string > recievers = new List< string >( Client.Instance.MessageCenter.ChatMessages.Count );
			
			foreach ( ChatMessage message in Client.Instance.MessageCenter.ChatMessages )
			{
				recievers.Add( message.From );
			}

			foreach ( string jid in recievers )
			{
				DisplayChat( jid ) ;
			}
		}
	}
}