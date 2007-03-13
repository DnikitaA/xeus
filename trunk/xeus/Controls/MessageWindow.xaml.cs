using System ;
using System.Collections.Generic ;
using System.Diagnostics ;
using System.Timers ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Input ;
using System.Windows.Media ;
using System.Windows.Threading ;
using agsXMPP.protocol.extensions.chatstates ;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for MessageWindow.xaml
	/// </summary>
	public partial class MessageWindow : WindowBase
	{
		private static MessageWindow _instance ;
		private static ListBox _listBox ;
		private static TextBox _textBox ;

		private Timer _listRefreshTimer = new Timer( 150 ) ;
		private Timer _timeRefreshTimer = new Timer( 10000 ) ;
		private Timer _timerNoTyping = new Timer( 10000 ) ;

		private Chatstate _chatstate = Chatstate.None ;

		private delegate void ScrollToLastItemCallback( ListBox listBox ) ;

		private delegate void DisplayChatCallback( string jid, bool activate ) ;

		private delegate void RefreshTimeCallback() ;

		private delegate void SendChatStateCallback( Chatstate chatstate ) ;

		public MessageWindow()
		{
			InitializeComponent() ;

			_tabs.SelectionChanged += new SelectionChangedEventHandler( _tabs_SelectionChanged ) ;
			_tabs.MouseDoubleClick += new MouseButtonEventHandler( _tabs_MouseDoubleClick );
			_listRefreshTimer.Elapsed += new ElapsedEventHandler( _listRefreshTimer_Elapsed ) ;
			_timeRefreshTimer.Elapsed += new ElapsedEventHandler( _timeRefreshTimer_Elapsed ) ;
			_timerNoTyping.Elapsed += new ElapsedEventHandler( _timerNoTyping_Elapsed );

			_listRefreshTimer.AutoReset = false ;
			_listRefreshTimer.Start() ;

			KeyDown += new KeyEventHandler( MessageWindow_KeyDown );
		}

		void _timerNoTyping_Elapsed( object sender, ElapsedEventArgs e )
		{
			ChangeChatState( Chatstate.paused ) ;
		}

		void ChangeChatState( Chatstate chatstate )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( chatstate == Chatstate.composing )
				{
					_timerNoTyping.Start();					
				}

				if ( _chatstate == chatstate )
				{
					return;
				}

				switch ( chatstate )
				{
					case Chatstate.paused:
					case Chatstate.gone:
						{
							_timerNoTyping.Stop();
							break;
						}
				}

				SendChatState( chatstate );

				Trace.WriteLine( chatstate ) ;

				_chatstate = chatstate;
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
												  new SendChatStateCallback( ChangeChatState ), chatstate );
			}
		}

		void MessageWindow_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Escape )
			{
				_instance.RemoveCurrentTab();
			}
		}

		void RemoveCurrentTab()
		{
			TabItem selectedItem = ( TabItem ) _instance._tabs.SelectedItem ;

			_tabs.Items.Remove( selectedItem );

			if ( _tabs.Items.Count == 0 )
			{
				CloseWindow() ;
			}
		}

		void _tabs_MouseDoubleClick( object sender, MouseButtonEventArgs e )
		{
			RemoveCurrentTab() ;
		}

		private void _timeRefreshTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			RefreshTime() ;
		}

		private void RefreshTime()
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( _instance != null && _instance._tabs != null )
				{
					TabItem selectedItem = ( TabItem ) _instance._tabs.SelectedItem ;
					RosterItem rosterItem = selectedItem.Content as RosterItem ;

					if ( rosterItem != null )
					{
						lock ( rosterItem.Messages._syncObject )
						{
							foreach ( ChatMessage chatMessage in rosterItem.Messages )
							{
								chatMessage.RelativeTime = TimeUtilities.FormatRelativeTime( chatMessage.Time ) ;
							}
						}
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new RefreshTimeCallback( RefreshTime ) ) ;
			}
		}

		public static ListBox MessageListBox
		{
			get
			{
				return _listBox ;
			}
			set
			{
				_listBox = value ;

				if ( _listBox != null )
				{
					_listBox.DataContextChanged += new DependencyPropertyChangedEventHandler( _listBox_DataContextChanged ) ;
				}
			}
		}

		public static TextBox MessageTextBox
		{
			get
			{
				return _textBox ;
			}

			set
			{
				_textBox = value ;
			}
		}

		private void _listRefreshTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			ScrollToLastItem( MessageListBox ) ;
		}

		private void _tabs_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( e.RemovedItems.Count > 0 )
			{
				TabItem unselectedItem = ( TabItem ) e.RemovedItems[ 0 ] ;

				RosterItem rosterItem = unselectedItem.Content as RosterItem ;

				if ( rosterItem != null )
				{
					ChangeChatState( Chatstate.gone );
				}
			}	
			
			if ( e.AddedItems.Count > 0 )
			{
				TabItem selectedItem = ( TabItem ) e.AddedItems[ 0 ] ;

				RosterItem rosterItem = selectedItem.Content as RosterItem ;

				if ( rosterItem != null )
				{
					if ( !rosterItem.MessagesPreloaded )
					{
						Database database = new Database() ;

						List< ChatMessage > messages = database.ReadMessages( rosterItem ) ;

						int i = 0 ;

						foreach ( ChatMessage chatMessage in messages )
						{
							lock ( rosterItem.Messages._syncObject )
							{
								rosterItem.Messages.Insert( i, chatMessage ) ;
							}

							i++ ;
						}

						rosterItem.MessagesPreloaded = true ;
					}

					rosterItem.HasUnreadMessages = false ;

					ChangeChatState( Chatstate.active ) ;
				}
			}

		}

		protected override void OnClosed( EventArgs e )
		{
			ChangeChatState( Chatstate.gone ) ;

			_timeRefreshTimer.Stop() ;
			_listRefreshTimer.Stop() ;

			_instance = null ;

			base.OnClosed( e ) ;
		}

		public static bool IsOpen()
		{
			return ( _instance != null ) ;
		}

		public static void CloseWindow()
		{
			if ( _instance != null )
			{
				_instance.Close() ;
			}
		}

		private static TabItem FindTab( string jid )
		{
			foreach ( TabItem tab in _instance._tabs.Items )
			{
				RosterItem item = tab.Content as RosterItem ;

				if ( item.Key == jid )
				{
					return tab ;
				}
			}

			return null ;
		}

		private static void DisplayChat( string jid, bool activateTab )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( _instance == null )
				{
					_instance = new MessageWindow() ;
					_instance.Activate() ;
				}

				TabItem tab = FindTab( jid ) ;
				RosterItem rosterItem ;

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

					_instance._tabs.Items.Add( tab ) ;
				}
				else
				{
					rosterItem = tab.Content as RosterItem ;

					TabItem tabItemSelected = ( TabItem ) _instance._tabs.SelectedItem ;

					RosterItem selectedItem = tabItemSelected.Content as RosterItem ;

					if ( rosterItem != null && selectedItem != null
					     && string.Compare( selectedItem.Key, rosterItem.Key, true ) == 0 )
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

				_instance._listRefreshTimer.Start() ;
				_instance._timeRefreshTimer.Start() ;

				if ( MessageTextBox != null )
				{
					MessageTextBox.Focus() ;
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new DisplayChatCallback( DisplayChat ), jid, false ) ;
			}
		}

		public static void SendChatState( Chatstate chatstate )
		{
			if ( MessageTextBox != null && _instance != null && _instance._tabs != null
					&& Client.Instance != null && Client.Instance.IsAvailable )
			{
				TabItem selectedItem = ( TabItem ) _instance._tabs.SelectedItem ;
				RosterItem rosterItem = _instance._tabs.SelectedContent as RosterItem ;

				if ( rosterItem != null && selectedItem != null )
				{
					Client.Instance.SendChatState( rosterItem, chatstate, ( string ) selectedItem.Tag ) ;
				}
			}
		}

		public static void SendMessage()
		{
			if ( MessageTextBox != null )
			{
				TabItem selectedItem = ( TabItem ) _instance._tabs.SelectedItem ;
				RosterItem rosterItem = _instance._tabs.SelectedContent as RosterItem ;

				if ( rosterItem != null )
				{
					if ( selectedItem.Tag == null )
					{
						selectedItem.Tag = rosterItem.GenerateChatThreadId() ;
					}

					Client.Instance.SendChatMessage( rosterItem, MessageTextBox.Text, ( string )selectedItem.Tag ) ;

					_instance.ChangeChatState( Chatstate.paused ) ;

					MessageTextBox.Text = String.Empty ;
					_instance._listRefreshTimer.Start() ;
				}
			}
		}

		public static void TextKeyPress( KeyEventArgs e )
		{
			if ( MessageTextBox != null )
			{
				RosterItem rosterItem = _instance._tabs.SelectedContent as RosterItem ;

				if ( rosterItem != null )
				{
					if ( e.Key == Key.Return && Keyboard.IsKeyDown( Key.LeftCtrl ) )
					{
						SendMessage() ;
					}
					else if ( e.Key >= Key.D0 && e.Key <= Key.Z )
					{
						_instance.ChangeChatState( Chatstate.composing ) ;
					}
				}
			}
		}

		private static void _listBox_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			_instance._listRefreshTimer.Start() ;
		}

		public static void ScrollToLastItem( ListBox listBox )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( listBox != null && listBox.Items.Count > 0 )
				{
					ChatMessage chatMessage = listBox.Items[ listBox.Items.Count - 1 ] as ChatMessage ;

					TabItem selectedItem = ( TabItem ) _instance._tabs.SelectedItem ;

					if ( !string.IsNullOrEmpty( chatMessage.ThreadId ) )
					{
						selectedItem.Tag = chatMessage.ThreadId ;
					}

					listBox.ScrollIntoView( chatMessage ) ;
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new ScrollToLastItemCallback( ScrollToLastItem ), listBox ) ;
			}
		}

		public static void DisplayChatWindow( string activateJid, bool activate )
		{
			List< string > recievers = new List< string >( Client.Instance.MessageCenter.ChatMessages.Count ) ;

			foreach ( ChatMessage message in Client.Instance.MessageCenter.ChatMessages )
			{
				recievers.Add( message.From ) ;
			}

			foreach ( string jid in recievers )
			{
				DisplayChat( jid, false ) ;
			}

			DisplayChat( activateJid, activate ) ;
		}
	}
}