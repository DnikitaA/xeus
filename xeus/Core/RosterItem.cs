using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Collections.Specialized ;
using System.Data.Common ;
using System.Text.RegularExpressions ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Data ;
using System.Windows.Documents ;
using System.Windows.Input ;
using System.Windows.Media ;
using System.Windows.Media.Imaging ;
using System.Windows.Threading ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.disco ;
using agsXMPP.protocol.iq.roster ;
using agsXMPP.protocol.iq.vcard ;
using xeus.Controls ;
using xeus.Properties ;
using Brushes=System.Windows.Media.Brushes;
using Color=System.Drawing.Color;
using Group=agsXMPP.protocol.Base.Group;
using Image=System.Windows.Controls.Image;

namespace xeus.Core
{
	internal class RosterItem : NotifyInfoDispatcher, IDisposable
	{
		private ObservableCollectionDisp< ChatMessage > _messages =
			new ObservableCollectionDisp< ChatMessage >( App.DispatcherThread ) ;

		private ObservableCollectionDisp< string > _errors =
			new ObservableCollectionDisp< string >( App.DispatcherThread ) ;

		public delegate void VcardHandler( Vcard vcard ) ;

		private agsXMPP.protocol.iq.roster.RosterItem _rosterItem ;
		private string _statusText = "Unavailable" ;

		private string _key ;
		private DateTime _birthday = DateTime.MinValue ;
		private string _url = String.Empty ;
		private string _title = String.Empty ;
		private string _role = String.Empty ;
		private string _fullName = String.Empty ;
		private string _nickName = String.Empty ;
		private string _description = String.Empty ;
		private string _name = String.Empty ;
		private string _organization ;
		private string _emailPreferred ;
		private BitmapImage _image ;
		private bool _hasVCardRecivied = false ;
		private bool _hasUnreadMessages = false ;
		private bool _messagesPreloaded = false ;

		private string _draftMessage = String.Empty ;

		private ChatMessage _lastMessageFrom ;
		private ChatMessage _lastMessageTo ;

		private SubscriptionType _subscriptionType = SubscriptionType.none ;
		private string _customName = String.Empty ;

		private Presence _presence ;
		private string _statusDescription = "Unavailable" ;
		private DiscoInfo _disco ;

		private bool _isInDatabase = false ;
		private bool _isDirty = false ;
		private string _imagefileName ;

		private bool _removeTemporaryImage ;

		private string _transport = String.Empty ;

		private FlowDocument _messagesDocument = null ;

		private RosterItem()
		{
			_messages.CollectionChanged += new NotifyCollectionChangedEventHandler( _messages_CollectionChanged ) ;
		}

		public bool IsVcardReadOnly
		{
			get
			{
				return ( _key != Client.Instance.MyJid.Bare ) ;
			}
		}

		public RosterItem( DbDataReader reader ) : this()
		{
			_isInDatabase = true ;
			_key = reader[ "Key" ] as string ;
			_subscriptionType = ( SubscriptionType ) Enum.Parse( typeof ( SubscriptionType ),
			                                                     reader[ "SubscriptionType" ] as string, false ) ;
			_fullName = reader[ "FullName" ] as string ;
			_nickName = reader[ "NickName" ] as string ;
			_customName = reader[ "CustomName" ] as string ;

			Database database = new Database();

			if ( !reader.IsDBNull( reader.GetOrdinal( "IdLastMessageFrom" ) ) )
			{
				Int64 idLastMessageFrom = ( Int64 )reader[ "IdLastMessageFrom" ] ;

				_lastMessageFrom = database.GetChatMessage( idLastMessageFrom, this ) ;
			}
			
			if ( !reader.IsDBNull( reader.GetOrdinal( "IdLastMessageTo" ) ) )
			{
				Int64 idLastMessageTo = ( Int64 )reader[ "IdLastMessageTo" ] ;

				_lastMessageTo = database.GetChatMessage( idLastMessageTo, this ) ;
			}
		}

		public DiscoInfo Disco
		{
			get
			{
				return _disco ;
			}

			set
			{
				_disco = value ;

				NotifyPropertyChanged( "DiscoInfo" ) ;
				NotifyPropertyChanged( "SupportsChatNotification" ) ;
			}
		}

		Int64 IdLastMessageTo
		{
			get
			{
				if ( _lastMessageTo == null )
				{
					return 0 ;
				}
				else
				{
					return _lastMessageTo.Id ;
				}
			}
		}

		Int64 IdLastMessageFrom
		{
			get
			{
				if ( _lastMessageFrom == null )
				{
					return 0 ;
				}
				else
				{
					return _lastMessageFrom.Id ;
				}
			}
		}
		public Dictionary< string, object > GetData()
		{
			Dictionary< string, object > data = new Dictionary< string, object >();

			data.Add( "Key", Key ) ;
			data.Add( "IdLastMessageFrom", IdLastMessageFrom ) ;
			data.Add( "IdLastMessageTo", IdLastMessageTo ) ;
			data.Add( "SubscriptionType", SubscriptionType.ToString() ) ;
			data.Add( "FullName", FullName ) ;
			data.Add( "NickName", NickName ) ;
			data.Add( "CustomName", CustomName ) ;

			return data ;
		}

		public RosterItem( agsXMPP.protocol.iq.roster.RosterItem rosterItem ) : this()
		{
			_rosterItem = rosterItem ;
			_key = _rosterItem.Jid.Bare ;
			_subscriptionType = rosterItem.Subscription ;
		}

		private void _messages_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Replace:
					{
						if ( SetLastMessages( e.NewItems ) )
						{
							HasUnreadMessages = true ;
						}

						GenerateMessagesDocument( e.NewItems );

						NotifyPropertyChanged( "MessagesDocument" );

						break ;
					}
				case NotifyCollectionChangedAction.Reset:
					{
						HasUnreadMessages = false ;

						_messagesDocument = null ;

						NotifyPropertyChanged( "MessagesDocument" );

						break ;
					}
			}
		}

		private bool SetLastMessages( IList newMessages )
		{
			DateTime maxFrom = DateTime.MinValue ;
			DateTime maxTo = DateTime.MinValue ;
			ChatMessage fromMe = null ;
			ChatMessage toMe = null ;

			bool newMessagesCame = false ;

			foreach ( ChatMessage message in newMessages )
			{
				if ( message.SentByMe )
				{
					if ( fromMe == null || maxFrom < message.Time )
					{
						fromMe = message ;
						maxFrom = message.Time ;
					}
				}
				else
				{
					newMessagesCame = true ;

					if ( toMe == null || maxTo < message.Time )
					{
						toMe = message ;
						maxTo = message.Time ;
					}
				}
			}

			if ( fromMe != null )
			{
				LastMessageFrom = fromMe ;
			}

			if ( toMe != null )
			{
				LastMessageTo = toMe ;
			}

			return newMessagesCame ;
		}

		public bool IsInitialized
		{
			get
			{
				return ( XmppRosterItem != null ) ;
			}
		}

		public agsXMPP.protocol.iq.roster.RosterItem XmppRosterItem
		{
			get
			{
				return _rosterItem ;
			}

			set
			{
				string group = Group ;

				_rosterItem = value ;

				Name = _rosterItem.Name ;
				SubscriptionType = _rosterItem.Subscription ;

				if ( group != Group )
				{
					NotifyPropertyChanged( "Group" ) ;
				}
			}
		}

		public string Key
		{
			get
			{
				return _key ;
			}

			set
			{
				_key = value ;
			}
		}

		private bool IsTrimmedEmpty( string text )
		{
			if ( text == null )
			{
				return true ;
			}
			else
			{
				return text.Trim() == String.Empty ;
			}
		}

		public string DisplayName
		{
			get
			{
				if ( !IsTrimmedEmpty( CustomName ) )
				{
					return CustomName ;
				}
				if ( !IsTrimmedEmpty( FullName ) )
				{
					return FullName ;
				}
				else if ( !IsTrimmedEmpty( NickName ) )
				{
					return NickName ;
				}

				return ( !IsTrimmedEmpty( Name ) ) ? Name : Key ;
			}
		}

		public bool HasSpecialStatus
		{
			get
			{
				return ( ( _presence != null ) && _presence.Show != ShowType.NONE ) ;
			}
		}

		public ControlTemplate StatusTemplate
		{
			get
			{
				return PresenceTemplate.GetStatusTemplate( _presence ) ;
			}
		}

		public static bool IsSystemGroup( string group )
		{
			switch ( group )
			{
				case "Services":
				case "Offline":
				case "Ungrouped":
					{
						return true ;
					}
				default:
					{
						return false ;
					}
			}
		}

		public string Group
		{
			get
			{
				string group ;

				if ( IsService )
				{
					group = "Services" ;
				}
				else if ( !IsInitialized && _presence != null )
				{
					group = "Out of Server Roster" ;
				}
				else if ( !IsInitialized || _presence == null || _presence.Type == PresenceType.unavailable )
				{
					group = "Offline" ;
				}
				else if ( _rosterItem.GetGroups().Count > 0 )
				{
					Group rosterGroup = ( Group ) _rosterItem.GetGroups().Item( 0 ) ;
					group = rosterGroup.Name ;
				}
				else
				{
					group = "Ungrouped" ;
				}

				return group ;
			}
		}

		public string StatusDescription
		{
			get
			{
				return _statusDescription ;
			}
		}

		public Presence Presence
		{
			get
			{
				return _presence ;
			}

			set
			{
				string group = Group ;

				_presence = value ;
				_statusDescription = "Unavailable" ;

				if ( _presence == null || _presence.Type != PresenceType.available )
				{
					_statusText = "Unavailable" ;
				}
				else
				{
					if ( _presence.Type == PresenceType.error )
					{
						_statusText = "Error" ;
					}
					else
					{
						switch ( _presence.Show )
						{
							case ShowType.away:
								{
									_statusText = "Away" ;
									break ;
								}
							case ShowType.dnd:
								{
									_statusText = "Do not Disturb" ;
									break ;
								}
							case ShowType.chat:
								{
									_statusText = "Free for Chat" ;
									break ;
								}
							case ShowType.xa:
								{
									_statusText = "Extended Away" ;
									break ;
								}
							default:
								{
									_statusText = "Online" ;
									break ;
								}
						}

						_errors.Clear() ;

						if ( _presence.Status != null && _presence.Status != String.Empty )
						{
							_statusDescription = _presence.Status ;
						}
						else
						{
							_statusDescription = _statusText ;
						}
					}
				}

				NotifyPropertyChanged( "Presence" ) ;
				NotifyPropertyChanged( "StatusText" ) ;
				NotifyPropertyChanged( "StatusDescription" ) ;
				NotifyPropertyChanged( "StatusTemplate" ) ;
				NotifyPropertyChanged( "HasSpecialStatus" ) ;
				NotifyPropertyChanged( "IsExnhancedStatusText" ) ;

				if ( group != Group )
				{
					NotifyPropertyChanged( "Group" ) ;
				}
			}
		}

		public bool IsExnhancedStatusText
		{
			get
			{
				return ( _presence != null && _presence.Status != null && _presence.Status != String.Empty ) ;
			}
		}

		// image has to be always created in Dispatcher thread!!!
		public void SetVcard( Vcard vcard )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( vcard != null )
				{
					Birthday = vcard.Birthday ;
					Description = vcard.Description ;

					Email email = vcard.GetPreferedEmailAddress() ;
					if ( email != null )
					{
						EmailPreferred = email.UserId ;
					}

					FullName = vcard.Fullname ;
					NickName = vcard.Nickname ;

					Organization organization = vcard.Organization ;
					if ( organization != null )
					{
						Organization = vcard.Organization.Name ;
					}

					Role = vcard.Role ;
					Title = vcard.Title ;
					Url = vcard.Url ;

					BitmapImage image = Storage.ImageFromPhoto( vcard.Photo ) ;

					Image = image ;
				}

				if ( Image == null )
				{
					if ( !IsInitialized )
					{
						Image = Storage.GetDefaultAvatar() ;
					}
					else if ( IsService )
					{
						Image = Storage.GetDefaultServiceAvatar() ;
					}
					else
					{
						Image = Storage.GetDefaultAvatar() ;
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new VcardHandler( SetVcard ), vcard ) ;
			}
		}

		public string StatusText
		{
			get
			{
				return _statusText ;
			}
		}

		public string Role
		{
			get
			{
				return _role ;
			}
			set
			{
				_role = value ;
				NotifyPropertyChanged( "Role" ) ;
			}
		}

		public string FullName
		{
			get
			{
				return _fullName ;
			}
			set
			{
				string oldDisplayName = DisplayName ;

				if ( _fullName != value )
				{
					_isDirty = true ;
				}

				_fullName = value ;

				NotifyPropertyChanged( "FullName" ) ;

				if ( oldDisplayName != DisplayName )
				{
					NotifyPropertyChanged( "DisplayName" ) ;
				}
			}
		}

		public string NickName
		{
			get
			{
				return _nickName ;
			}
			set
			{
				string oldDisplayName = DisplayName ;

				if ( _nickName != value )
				{
					_isDirty = true ;
				}

				_nickName = value ;

				NotifyPropertyChanged( "NickName" ) ;

				if ( oldDisplayName != DisplayName )
				{
					NotifyPropertyChanged( "DisplayName" ) ;
				}
			}
		}

		public string Description
		{
			get
			{
				return _description ;
			}
			set
			{
				_description = value ;
				NotifyPropertyChanged( "Description" ) ;
			}
		}

		public string Name
		{
			get
			{
				return _name ;
			}
			set
			{
				string oldDisplayName = DisplayName ;

				_name = value ;
				NotifyPropertyChanged( "Name" ) ;

				if ( oldDisplayName != DisplayName )
				{
					NotifyPropertyChanged( "DisplayName" ) ;
				}
			}
		}

		public string Organization
		{
			get
			{
				return _organization ;
			}
			set
			{
				_organization = value ;
				NotifyPropertyChanged( "Organization" ) ;
			}
		}

		public string EmailPreferred
		{
			get
			{
				return _emailPreferred ;
			}
			set
			{
				_emailPreferred = value ;
				NotifyPropertyChanged( "EmailPreferred" ) ;
			}
		}

		public DateTime Birthday
		{
			get
			{
				return _birthday ;
			}
			set
			{
				_birthday = value ;
				NotifyPropertyChanged( "Birthday" ) ;
			}
		}

		public string Url
		{
			get
			{
				return _url ;
			}
			set
			{
				_url = value ;
				NotifyPropertyChanged( "Url" ) ;
			}
		}

		public string Title
		{
			get
			{
				return _title ;
			}
			set
			{
				_title = value ;
				NotifyPropertyChanged( "Title" ) ;
			}
		}

		public string ImageFileName
		{
			get
			{
				return _imagefileName ;
			}

			set
			{
				_imagefileName = value ;
			}
		}

		public BitmapImage Image
		{
			get
			{
				return _image ;
			}

			set
			{
				_image = value ;
				NotifyPropertyChanged( "Image" ) ;
			}
		}

		public ObservableCollectionDisp< ChatMessage > Messages
		{
			get
			{
				return _messages ;
			}
		}

		public ObservableCollectionDisp< string > Errors
		{
			get
			{
				return _errors ;
			}
		}

		public bool HasVCardRecivied
		{
			get
			{
				return _hasVCardRecivied ;
			}

			set
			{
				_hasVCardRecivied = value ;
				NotifyPropertyChanged( "HasVCardRecivied" ) ;
			}
		}

		public bool HasUnreadMessages
		{
			get
			{
				return _hasUnreadMessages ;
			}
			set
			{
				_hasUnreadMessages = value ;
				NotifyPropertyChanged( "HasUnreadMessages" ) ;
			}
		}

		public ChatMessage LastMessageFrom
		{
			get
			{
				return _lastMessageFrom ;
			}

			set
			{
				if ( _lastMessageFrom != value )
				{
					_isDirty = true ;
				}

				_lastMessageFrom = value ;

				NotifyPropertyChanged( "LastMessageFrom" ) ;
			}
		}

		public ChatMessage LastMessageTo
		{
			get
			{
				return _lastMessageTo ;
			}

			set
			{
				if ( _lastMessageTo != value )
				{
					_isDirty = true ;
				}

				_lastMessageTo = value ;

				NotifyPropertyChanged( "LastMessageTo" ) ;
			}
		}

		public SubscriptionType SubscriptionType
		{
			get
			{
				return _subscriptionType ;
			}
			set
			{
				if ( _subscriptionType != value )
				{
					_isDirty = true ;
				}

				_subscriptionType = value ;

				NotifyPropertyChanged( "SubscriptionType" ) ;
				NotifyPropertyChanged( "SubscriptionTypeText" ) ;
			}
		}

		public string SubscriptionTypeText
		{
			get
			{
				switch ( SubscriptionType )
				{
					case SubscriptionType.both:
						{
							return "Authorized in both ways" ;
						}
					case SubscriptionType.from:
						{
							return "Contact is authorized, but you aren't" ;
						}
					case SubscriptionType.to:
						{
							return "You are authorized, but the Contact isn't" ;
						}
				}

				return "Unknown authorization state" ;
			}
		}

		public string CustomName
		{
			get
			{
				return _customName ;
			}

			set
			{
				string oldDisplayName = DisplayName ;

				if ( _customName != value )
				{
					_isDirty = true ;
				}

				_customName = value ;

				if ( oldDisplayName != DisplayName )
				{
					NotifyPropertyChanged( "DisplayName" ) ;
				}

				NotifyPropertyChanged( "CustomName" ) ;
			}
		}

		public bool IsService
		{
			get
			{
				return ( _rosterItem != null && _rosterItem.Jid.User == null ) ;
			}
		}

		public bool IsServiceRegistered
		{
			get
			{
				return ( _rosterItem != null ) ;
			}
		}

		public bool MessagesPreloaded
		{
			get
			{
				return _messagesPreloaded ;
			}
			set
			{
				_messagesPreloaded = value ;
			}
		}

		public bool IsInDatabase
		{
			get
			{
				return _isInDatabase ;
			}

			set
			{
				_isInDatabase = value ;
			}
		}

		public bool IsDirty
		{
			get
			{
				return _isDirty ;
			}
			set
			{
				_isDirty = value ;
			}
		}

		public bool RemoveTemporaryImage
		{
			get
			{
				return _removeTemporaryImage ;
			}
			set
			{
				_removeTemporaryImage = value ;
			}
		}

		public string Transport
		{
			get
			{
				return _transport ;
			}
			set
			{
				_transport = value ;

				NotifyPropertyChanged( "Transport" ) ;
			}
		}

		public bool SupportsChatNotification
		{
			get
			{
				if ( _disco == null )
				{
					return true ; 
				}
				else
				{
					return true ;// return _disco.HasFeature( agsXMPP.Uri.CHATSTATES ) ;
				}
			}
		}

		public string DraftMessage
		{
			get
			{
				return _draftMessage ;
			}
			set
			{
				_draftMessage = value ;
			}
		}

		public void Dispose()
		{
			_messages.CollectionChanged -= _messages_CollectionChanged ;
		}

		public FlowDocument MessagesDocument
		{
			get
			{
				return _messagesDocument ;
			}
		}

		readonly FontFamily _textFont = new FontFamily( "Segoe" ) ;

		protected void GenerateMessagesDocument( IList messages )
		{
			if ( _messagesDocument == null )
			{
				_messagesDocument = new FlowDocument() ;
				_messagesDocument.Foreground = Brushes.White ;
				_messagesDocument.FontFamily = _textFont ;
				_messagesDocument.TextAlignment = TextAlignment.Left ;
			}

			foreach ( ChatMessage message in messages )
			{
				int index = Messages.IndexOf( message ) ;

				ChatMessage previousMessage = null ;

				if ( index >= 1 )
				{
					previousMessage = Messages[ index - 1 ] ;
				}

				_messagesDocument.Blocks.Add( GenerateMessage( message, previousMessage ) ) ;
			}
		}

		readonly Brush _alternativeBackground = new SolidColorBrush( System.Windows.Media.Color.FromRgb( 50, 50, 50 ) ) ;
		readonly Brush _alternativeForeground = new SolidColorBrush( System.Windows.Media.Color.FromRgb( 191, 215, 234 ) ) ;
		readonly Binding _timeBinding = new Binding( "RelativeTime" ) ;

		readonly Regex _urlregex = new Regex( @"[""'=]?(http://|ftp://|https://|www\.|ftp\.[\w]+)([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])", RegexOptions.IgnoreCase | RegexOptions.Compiled ) ;

		public Block GenerateMessage( ChatMessage message, ChatMessage previousMessage )
		{
			Section groupSection = _messagesDocument.Blocks.LastBlock as Section ;

			Paragraph paragraph = new Paragraph();

			paragraph.Padding = new Thickness( 0.0, 0.0, 0.0, 0.0 );
			paragraph.Margin = new Thickness( 0.0, 5.0, 0.0, 10.0 );

			bool newSection = ( groupSection == null ) ;

			if ( previousMessage == null 
				|| previousMessage.SentByMe != message.SentByMe
				|| ( message.Time - previousMessage.Time > TimeSpan.FromMinutes( Settings.Default.UI_GroupMessagesByMinutes ) ) )
			{
				Image avatar = new Image() ;
				avatar.Source = message.Image ;
				avatar.Width = 30.0 ;

				paragraph.Inlines.Add( avatar );
				paragraph.Inlines.Add( "  " );

				newSection = true ;
			}

			if ( message.SentByMe )
			{
				paragraph.Foreground = _alternativeForeground ;
			}

			MatchCollection matches = _urlregex.Matches( message.Body ) ;

			if ( matches.Count > 0 )
			{
				string[] founds = new string[ matches.Count ];

				for ( int i = 0; i < founds.Length; i++ )
				{
					founds[ i ] = matches[ i ].ToString() ;
				}

				string[] bodies = message.Body.Split( founds, StringSplitOptions.RemoveEmptyEntries ) ;

				for ( int j = 0; j < bodies.Length || j < founds.Length; j++ )
				{
					bool wrongUri = false ;

					if ( bodies.Length > j )
					{
						paragraph.Inlines.Add( bodies[ j ] ) ;
					}

					if ( founds.Length > j )
					{
						Run hyperlinkRun = new Run( founds[ j ] ) ;
						Hyperlink hyperlink = new XeusHyperlink( hyperlinkRun ) ;
						hyperlink.Foreground = Brushes.DarkSalmon ;

						try
						{
							string url = hyperlinkRun.Text ;
							
							if ( !url.Contains( ":" ) )
							{
								url = string.Format( "http://{0}", url ) ;
							}

							hyperlink.NavigateUri = new Uri( url ) ;
						}

						catch
						{
							// improper uri format
							wrongUri = true ;
						}

						if ( wrongUri )
						{
							paragraph.Inlines.Add( hyperlinkRun ) ;
						}
						else
						{
							paragraph.Inlines.Add( hyperlink ) ;
						}
					}
				}
			}
			else
			{
				paragraph.Inlines.Add( message.Body ) ;
			}

			paragraph.DataContext = message ;

			TextBlock textBlock = new TextBlock();
			textBlock.Style = MessageWindow.GetTimeTextBlockStyle() ;
			textBlock.SetBinding( TextBlock.TextProperty, _timeBinding ) ;

			paragraph.Inlines.Add( textBlock ) ;

			if ( newSection )
			{
				groupSection = new Section() ;

				if ( message.SentByMe )
				{
					groupSection.Background = _alternativeBackground ;
				}

				groupSection.Blocks.Add( paragraph ) ;
				groupSection.Margin = new Thickness( 3.0, 10.0, 3.0, 0.0 ) ;
				groupSection.BorderThickness = new Thickness( 0.0, 2.0, 0.0, 0.0 ) ;
				groupSection.BorderBrush = _alternativeBackground ;
			}

			groupSection.Blocks.Add( paragraph );

			return groupSection ;
		}
	}
}