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
		private static MessageWindow _instance ;

		private delegate void DisplayChatCallback( string jid, bool activate ) ;

		public MessageWindow()
		{
			InitializeComponent() ;

			_tabs.SelectionChanged += new SelectionChangedEventHandler( _tabs_SelectionChanged );
		}

		void _tabs_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			RosterItem rosterItem = ( ( TabItem )_tabs.SelectedItem ).Content as RosterItem ;

			if ( rosterItem != null )
			{
				rosterItem.HasUnreadMessages = false ;
			}
		}
	
		protected override void OnClosed( System.EventArgs e )
		{
			_instance = null ;
			base.OnClosed( e );
		}

		public static bool IsOpen()
		{
			return ( _instance != null ) ;
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

		static void DisplayChat( string jid, bool activateTab )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( _instance == null )
				{
					_instance = new MessageWindow();
				}

				TabItem tab = FindTab( jid ) ;
				RosterItem rosterItem  ;

				if ( tab == null )
				{
					rosterItem = Client.Instance.Roster.FindItem( jid ) ;

					if ( rosterItem == null )
					{
						// not yet in the roster
						return ;
					}

					tab = new TabItem() ;
					tab.Header = rosterItem ;
					tab.Content = rosterItem ;
					tab.Tag = jid ;

					_instance._tabs.Items.Add( tab ) ;
				}
				else
				{
					rosterItem = tab.Content as RosterItem ;

					TabItem tabItemSelected = ( TabItem )_instance._tabs.SelectedItem ;

					RosterItem selectedItem = tabItemSelected.Content as RosterItem ;

					if ( rosterItem != null && selectedItem != null
					     && selectedItem.Key == rosterItem.Key )
					{
						rosterItem.HasUnreadMessages = false ;
					}
				}

				if ( activateTab )
				{
					_instance._tabs.SelectedItem = tab ;
					_instance.Activate() ;
				}

				if ( rosterItem != null )
				{
					Client.Instance.MessageCenter.MoveUnreadMessagesToRosterItem( rosterItem ) ;

					if ( !_instance.IsVisible )
					{
						_instance.Show() ;
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new DisplayChatCallback( DisplayChat ), jid, false ) ;				
			}
		}

		public static void DisplayChatWindow( string activateJid, bool activate )
		{
			List< string > recievers = new List< string >( Client.Instance.MessageCenter.ChatMessages.Count );
			
			foreach ( ChatMessage message in Client.Instance.MessageCenter.ChatMessages )
			{
				recievers.Add( message.From );
			}

			foreach ( string jid in recievers )
			{
				DisplayChat( jid, false ) ;
			}

			DisplayChat( activateJid, activate ) ;
		}
	}
}