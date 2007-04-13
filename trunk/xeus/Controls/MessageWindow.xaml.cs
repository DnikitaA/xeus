using System ;
using System.Collections.Generic ;
using System.Diagnostics ;
using System.Timers ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Documents ;
using System.Windows.Input ;
using System.Windows.Media ;
using System.Windows.Threading ;
using agsXMPP.protocol.extensions.chatstates ;
using xeus.Core ;
using xeus.Properties ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for MessageWindow.xaml
	/// </summary>
	public partial class MessageWindow : WindowBase, IDisposable
	{
		private static MessageWindow _instance ;
		private static TextBox _textBox ;

		private Timer _scrollTimer = new Timer( 10 ) ;
		private Timer _timeRefreshTimer = new Timer( 10000 ) ;
		private Timer _timerNoTyping = new Timer( 5000 ) ;
		private Timer _timerNoTyping2 = new Timer( 20000 ) ;

		private Chatstate _chatstate = Chatstate.None ;

		private InlineMethod _inlineMethod = new InlineMethod() ;

		private delegate void ScrollToLastItemCallback() ;

		private delegate void DisplayChatCallback( string jid, bool activate ) ;

		private delegate void RefreshTimeCallback() ;

		private delegate void ContactIsTypingCallback( string userName, Chatstate chatstate ) ;

		private delegate void SendChatStateCallback( Chatstate chatstate ) ;

		private delegate void SelectItemCallback( ChatMessage item ) ;

		public MessageWindow()
		{
			InitializeComponent() ;

			_tabs.SelectionChanged += new SelectionChangedEventHandler( _tabs_SelectionChanged ) ;
			_tabs.MouseDoubleClick += new MouseButtonEventHandler( _tabs_MouseDoubleClick );
			_timeRefreshTimer.Elapsed += new ElapsedEventHandler( _timeRefreshTimer_Elapsed ) ;
			_timerNoTyping.Elapsed += new ElapsedEventHandler( _timerNoTyping_Elapsed );
			_timerNoTyping2.Elapsed += new ElapsedEventHandler( _timerNoTyping2_Elapsed );

			_scrollTimer.AutoReset = false ;
			_scrollTimer.Elapsed += new ElapsedEventHandler( _scrollTimer_Elapsed );

			_inlineMethod.Finished += new InlineMethod.InlineResultHandler( _inlineMethod_Finished );
			_inlineSearch.TextChanged += new TextChangedEventHandler( _inlineSearch_TextChanged );
			_inlineSearch.Closed += new InlineSearch.ClosedHandler( _inlineSearch_Closed );

			KeyDown += new KeyEventHandler( MessageWindow_KeyDown );

			_statusBar.Loaded += new RoutedEventHandler( _statusBar_Loaded );
		}

		void _scrollTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			ScrollToLastItem2() ;
		}

		void _statusBar_Loaded( object sender, RoutedEventArgs e )
		{
			_inlineSearch.Visibility = Visibility.Collapsed ;
		}

		object _textsLock = new object();
		private List< KeyValuePair< string, ChatMessage > > _texts = null ;
		private string _lastSearch = String.Empty ;
		private string _textToSearch = String.Empty ;
		private ChatMessage _lastFoundItem = null ;

		private object SearchInList( ref bool stop, object param )
		{
			lock ( _textsLock )
			{
				if ( _texts == null )
				{
					_texts = new List< KeyValuePair< string, ChatMessage > >() ;

					lock ( _rosterItem.Messages._syncObject )
					{
						foreach ( ChatMessage chatMessage in _rosterItem.Messages )
						{
							_texts.Add( new KeyValuePair< string, ChatMessage >( chatMessage.Body.ToUpper().Trim(), chatMessage ) ) ;
						}
					}
				}
			}

			ChatMessage found = null ;

			_textToSearch = ( string ) param ;
			
			string toFound = ( ( string ) param ).ToUpper() ;

			bool searchNext = ( _lastSearch == toFound ) ;

			_lastSearch = toFound ;

			if ( searchNext && _lastFoundItem != null )
			{
				bool fromHere = false ;

				foreach ( KeyValuePair< string, ChatMessage > body in _texts )
				{
					if ( stop )
					{
						return null ;
					}

					if ( fromHere && body.Key.Contains( toFound ) )
					{
						found = body.Value ;
						break ;
					}

					if ( _lastFoundItem == body.Value )
					{
						fromHere = true ;
					}
				}
			}
			else
			{
				foreach ( KeyValuePair< string, ChatMessage > body in _texts )
				{
					if ( stop )
					{
						return null ;
					}

					if ( ( ( string ) param ) == String.Empty )
					{
						return null ;
					}

					if ( body.Key.Contains( toFound ) )
					{
						found = body.Value ;
						break ;
					}
				}
			}

			_lastFoundItem = found ;
			return found ;
		}


		void _inlineSearch_Closed( bool isEnter )
		{
			lock ( _textsLock )
			{
				_texts = null ;
				CleanSelection();
			}
		}

		void _inlineSearch_TextChanged( object sender, TextChangedEventArgs e )
		{
			_inlineMethod.Go( new InlineParam( SearchInList, _inlineSearch.Text ) ) ;
		}

		void _inlineMethod_Finished( object result )
		{
			ChatMessage rosterItem = ( ChatMessage ) result ;
			SelectItem( rosterItem ) ;
		}

		private List< TextRange > _previousTextRanges = new List< TextRange >() ;

		void CleanSelection()
		{
			foreach ( TextRange range in _previousTextRanges )
			{
				range.ApplyPropertyValue( Inline.BackgroundProperty, null );
			}

			_previousTextRanges.Clear();
		}

		void SelectText( Paragraph paragraph, string text )
		{
			CleanSelection();

			for ( Inline inline = paragraph.Inlines.FirstInline; inline != null; inline = inline.NextInline )
			{
				Run run = null ;

				Hyperlink hyperlink = inline as Hyperlink ;

				if ( hyperlink != null )
				{
					run = hyperlink.Inlines.FirstInline as Run ;
				}
				else
				{
					run = inline as Run ;
				}

				if ( run != null )
				{
					int firstStart = 0 ;

					while ( true )
					{
						if ( firstStart > run.Text.Length - 1 )
						{
							break ;
						}

						int start = run.Text.IndexOf( text, firstStart, StringComparison.InvariantCultureIgnoreCase ) ;
						int end = start + text.Length ;

						firstStart = start + 1 ;

						if ( start >= 0 )
						{
							TextRange textRange ;

							textRange = new TextRange( run.ContentStart.GetPositionAtOffset( start ),
							                           run.ContentStart.GetPositionAtOffset( end ) ) ;

							textRange.ApplyPropertyValue( Run.BackgroundProperty, Brushes.DarkRed ) ;

							_previousTextRanges.Add( textRange ) ;
						}

						else
						{
							break ;
						}
					}
				}
			}
		}

		private void SelectItem( ChatMessage item )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				_inlineSearch.NotFound = true ;

				if ( item != null )
				{
					foreach ( Block block in _flowDocumentViewer.Document.Blocks )
					{
						Section section = block as Section ;

						if ( section != null )
						{
							foreach ( Block sectionBlock in section.Blocks )
							{
								if ( sectionBlock.DataContext == item )
								{
									sectionBlock.BringIntoView();

									SelectText( sectionBlock as Paragraph, _textToSearch ) ;

									_inlineSearch.NotFound = false ;

									break ;
								}
							}
						}

						if ( !_inlineSearch.NotFound )
						{
							break ;
						}
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new SelectItemCallback( SelectItem ), item ) ;
			}
		}


		void _timerNoTyping2_Elapsed( object sender, ElapsedEventArgs e )
		{
			ChangeChatState( Chatstate.inactive ) ;
		}

		void _timerNoTyping_Elapsed( object sender, ElapsedEventArgs e )
		{
			ChangeChatState( Chatstate.paused ) ;
		}

		public static void ContactIsTyping( string userName, Chatstate chatstate )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( _instance != null && _instance._statusTyping != null )
				{
					_instance._typing.UserName = userName ;
					_instance._typing.Chatstate = chatstate ;
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
												  new ContactIsTypingCallback( ContactIsTyping ), userName, chatstate );
			}		
		}

		void ChangeChatState( Chatstate chatstate )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( chatstate == Chatstate.composing )
				{
					_timerNoTyping.Start();					
					_timerNoTyping2.Stop();
				}

				if ( _chatstate == chatstate )
				{
					return;
				}

				switch ( chatstate )
				{
					case Chatstate.paused:
						{
							_timerNoTyping.Stop();
							_timerNoTyping2.Start();
							break;
						}
					case Chatstate.inactive:
					case Chatstate.gone:
						{
							_timerNoTyping.Stop();
							_timerNoTyping2.Stop();
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

		void OnTypingUnchecked( object sender, RoutedEventArgs e )
		{
			ChangeChatState( Chatstate.inactive ) ;
			ChangeChatState( Chatstate.gone ) ;
		}
		
		void OnTypingChecked( object sender, RoutedEventArgs e )
		{
			ChangeChatState( Chatstate.active ) ;
		}

		void MessageWindow_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Escape )
			{
				if ( _inlineSearch.Visibility == Visibility.Visible )
				{
					_inlineSearch.SendKey( e.Key ) ;
				}
				else
				{
					_instance.RemoveCurrentTab() ;
				}
			}
			else if ( _inlineSearch != null && FocusManager.GetFocusedElement( this ) != _textBox )
			{
				_inlineSearch.SendKey( e.Key ) ;
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

		public static FlowDocumentScrollViewer FlowDocumentViewer
		{
			get
			{
				return _flowDocumentViewer ;
			}

			set
			{
				_flowDocumentViewer = value ;
				_scrollViewer = null ;

				_flowDocumentViewer.DataContextChanged += new DependencyPropertyChangedEventHandler( _flowDocumentViewer_DataContextChanged );
			}
		}

		static void _flowDocumentViewer_DataContextChanged( object sender, DependencyPropertyChangedEventArgs e )
		{
			ScrollToLastItem();
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

		RosterItem _rosterItem = null ;
		private static bool _isActivated = false ;
		private static FlowDocumentScrollViewer _flowDocumentViewer ;

		private void _tabs_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			if ( e.RemovedItems.Count > 0 )
			{
				TabItem unselectedItem = e.RemovedItems[ 0 ] as TabItem ;

				if ( unselectedItem != null )
				{
					RosterItem rosterItem = unselectedItem.Content as RosterItem ;

					if ( rosterItem != null )
					{
						rosterItem.DraftMessage = _textBox.Text ;
						ChangeChatState( Chatstate.gone ) ;
					}
				}
			}	
			
			if ( e.AddedItems.Count > 0 )
			{
				TabItem selectedItem = e.AddedItems[ 0 ] as TabItem ;

				if ( selectedItem != null )
				{
					_rosterItem = selectedItem.Content as RosterItem ;

					if ( _rosterItem != null )
					{
						if ( !_rosterItem.MessagesPreloaded )
						{
							Database database = new Database() ;

							List< ChatMessage > messages = database.ReadMessages( _rosterItem ) ;

							int i = 0 ;

							foreach ( ChatMessage chatMessage in messages )
							{
								bool exists = false ;

								lock ( _rosterItem.Messages._syncObject )
								{
									foreach ( ChatMessage existingMessage in _rosterItem.Messages )
									{
										if ( existingMessage.Id == chatMessage.Id )
										{
											exists = true ;
											break ;
										}
									}

									if ( !exists )
									{
										lock ( _rosterItem.Messages._syncObject )
										{
											_rosterItem.Messages.Insert( i, chatMessage ) ;
										}

										i++ ;
									}
								}
							}

							_rosterItem.MessagesPreloaded = true ;
						}

						_rosterItem.HasUnreadMessages = false ;

						ChangeChatState( Chatstate.active ) ;
					}

					ScrollToLastItem();
				}
			}
		}

		protected override void OnClosed( EventArgs e )
		{
			ChangeChatState( Chatstate.inactive ) ;
			ChangeChatState( Chatstate.gone ) ;

			_timeRefreshTimer.Stop() ;
			_timerNoTyping.Stop();
			_timerNoTyping2.Stop();

			_tabs.Items.Clear();

			base.OnClosed( e ) ;

			Client.Instance.Roster.ClearMesssages() ;

			_timeTextStyle = null ;
			_instance.Activated -= _instance_Activated ;
			_instance.Deactivated -= _instance_Deactivated ;
			_instance.Dispose();
			_instance = null ;
		}

		public static bool IsOpen()
		{
			return ( _instance != null ) ;
		}

		public static bool IsChatActive()
		{
			return ( _instance != null && _isActivated ) ;
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

		private static void Displ( string jid, bool activateTab )
		{
				if ( _instance == null )
				{
					_instance = new MessageWindow() ;

					_instance.Activated += new EventHandler( _instance_Activated );
					_instance.Deactivated += new EventHandler( _instance_Deactivated );
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
					_instance.Activate() ;
					_instance._tabs.SelectedItem = tab ;
				}

				if ( rosterItem != null )
				{
					Client.Instance.MessageCenter.RemoveMoveUnreadMessages() ;

					if ( !_instance.IsVisible )
					{
						_instance.Show() ;
						_instance.Activate() ;
					}
				}

				ScrollToLastItem2() ;

				_instance._timeRefreshTimer.Start() ;
		}

		private static void DisplayChat( string jid, bool activateTab )
		{
			App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
			                                  new DisplayChatCallback( Displ ), jid, activateTab ) ;
		}

		static void _instance_Deactivated( object sender, EventArgs e )
		{
			_isActivated = false ;
		}

		static void _instance_Activated( object sender, EventArgs e )
		{
			_isActivated = true ;

			Client.Instance.Event.MessageWindowActivated() ;

			if ( MessageTextBox != null )
			{
				MessageTextBox.Focus() ;
			}
		}

		public static void SendChatState( Chatstate chatstate )
		{
			if ( Settings.Default.Client_SendTyping )
			{
				if ( MessageTextBox != null && _instance != null && _instance._tabs != null
				     && Client.Instance != null && Client.Instance.IsAvailable )
				{
					RosterItem rosterItem = _instance._tabs.SelectedContent as RosterItem ;

					if ( rosterItem != null )
					{
						Client.Instance.SendChatState( rosterItem, chatstate ) ;
					}
				}
			}
		}

		public static void SendMessage()
		{
			if ( MessageTextBox != null )
			{
				RosterItem rosterItem = _instance._tabs.SelectedContent as RosterItem ;

				if ( rosterItem != null )
				{
					Client.Instance.SendChatMessage( rosterItem, MessageTextBox.Text ) ;

					_instance.ChangeChatState( Chatstate.inactive ) ;

					rosterItem.DraftMessage = MessageTextBox.Text = String.Empty ;

					ScrollToLastItem2();
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
					if ( e.Key == Key.Return && 
						( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) ) )
					{
						SendMessage() ;
					}
					else if ( ( e.Key >= Key.D0 && e.Key <= Key.Z )
							|| ( e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9 ) )

					{
						_instance.ChangeChatState( Chatstate.composing ) ;
					}
				}
			}
		}

		static ScrollViewer _scrollViewer = null;
		
		public static void ScrollToLastItem()
		{
			_instance._scrollTimer.Start();
		}

		protected static void ScrollToLastItem2()
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( _scrollViewer == null )
				{
					DependencyObject dependencyObject = _flowDocumentViewer ;

					while ( true )
					{
						dependencyObject = VisualTreeHelper.GetChild( dependencyObject, 0 ) ;

						if ( dependencyObject is ScrollViewer )
						{
							_scrollViewer = dependencyObject as ScrollViewer ;
							break ;
						}
					}
				}

				_scrollViewer.ScrollToBottom();
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new ScrollToLastItemCallback( ScrollToLastItem2 ) ) ;
			}
		}

		static Style _timeTextStyle = null ;

		public static Style GetTimeTextBlockStyle()
		{
			if ( _timeTextStyle == null )
			{
				_timeTextStyle = _instance.FindResource( "TimeText" ) as Style ;
			}

			return _timeTextStyle ;
		}

		public static void DisplayChatWindow( string activateJid, bool activate )
		{
			lock ( Client.Instance.MessageCenter.ChatMessages._syncObject )
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


		public void Dispose()
		{
			_timeRefreshTimer.Dispose();
			_timerNoTyping.Dispose();
			_timerNoTyping2.Dispose();
		}
	}
}