using System ;
using System.Collections ;
using System.Collections.Specialized ;
using System.ComponentModel ;
using System.Data ;
using System.Windows.Controls ;
using System.Windows.Media.Imaging ;
using System.Windows.Threading ;
using agsXMPP.protocol.Base ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.roster ;
using agsXMPP.protocol.iq.vcard ;
using Clifton.Tools.Xml ;

namespace xeus.Core
{
	[Serializable]
	internal class RosterItem : INotifyPropertyChanged, IDisposable
	{
		private ObservableCollectionDisp< ChatMessage > _messages =
			new ObservableCollectionDisp< ChatMessage >( App.DispatcherThread ) ;

		private ObservableCollectionDisp< string > _errors =
			new ObservableCollectionDisp< string >( App.DispatcherThread ) ;

		private delegate void SetVcardCallback( Vcard vcard ) ;

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
		private Organization _organization ;
		private Email _emailPreferred ;
		private BitmapImage _image ;
		private bool _hasVCardRecivied = false ;
		private bool _hasUnreadMessages = false ;
		private bool _messagesPreloaded = false ;

		private string _lastMessageFrom = "No message sent" ;
		private string _lastMessageTo = "No message recieved" ;
		private SubscriptionType _subscriptionType = SubscriptionType.none ;
		private string _customName = null ;

		public event PropertyChangedEventHandler PropertyChanged ;

		public Presence _presence ;
		private string _statusDescription = "Unavailable" ;

		private RosterItem()
		{
			_messages.CollectionChanged += new NotifyCollectionChangedEventHandler( _messages_CollectionChanged ) ;
		}

		public RosterItem( DataRow row ) : this()
		{
			_key = row[ "Key" ] as string ;
			_lastMessageFrom = row[ "LastMessageFrom" ] as string ;
			_lastMessageTo = row[ "LastMessageTo" ] as string ;
			_subscriptionType = ( SubscriptionType )Enum.Parse( typeof( SubscriptionType ),
																row[ "SubscriptionType" ] as string, false ) ;
			_fullName = row[ "FullName" ] as string ;
			_nickName = row[ "NickName" ] as string ;
			_customName = row[ "CustomName" ] as string ;
		}

		public XmlDatabase.FieldValuePair[] GetData()
		{
			XmlDatabase.FieldValuePair[] data = new XmlDatabase.FieldValuePair[ 7 ] ;

			data[ 0 ] = new NullFieldValuePair( "Key", Key ) ;
			data[ 1 ] = new NullFieldValuePair( "LastMessageFrom", LastMessageFrom ) ;
			data[ 2 ] = new NullFieldValuePair( "LastMessageTo", LastMessageTo ) ;
			data[ 3 ] = new NullFieldValuePair( "SubscriptionType", SubscriptionType.ToString() ) ;
			data[ 4 ] = new NullFieldValuePair( "FullName", FullName ) ;
			data[ 5 ] = new NullFieldValuePair( "NickName", NickName ) ;
			data[ 6 ] = new NullFieldValuePair( "CustomName", CustomName ) ;

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

						break ;
					}
				case NotifyCollectionChangedAction.Reset:
					{
						HasUnreadMessages = false ;
						break ;
					}
			}
		}

		bool SetLastMessages( IList newMessages )
		{
			DateTime maxFrom = DateTime.MinValue ;
			DateTime maxTo = DateTime.MinValue ;
			string fromMe = null ;
			string toMe = null ;

			bool newMessagesCame = false ;

			foreach ( ChatMessage message in newMessages )
			{
				if ( message.SentByMe )
				{
					if ( fromMe == null || maxFrom < message.Time )
					{
						fromMe = string.Format( "{0}\n{1}", message.Time, message.Body ) ;
						maxFrom = message.Time ;
					}
				}
				else
				{
					newMessagesCame = true ;

					if ( toMe == null || maxTo < message.Time )
					{
						toMe = string.Format( "{0}\n{1}", message.Time, message.Body ) ;
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
				_rosterItem = value ;

				Name = _rosterItem.Name ;
				SubscriptionType = _rosterItem.Subscription ;
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

		public string DisplayName
		{
			get
			{
				if ( !String.IsNullOrEmpty( CustomName ) )
				{
					return FullName ;
				}
				if ( !String.IsNullOrEmpty( FullName ) )
				{
					return FullName ;
				}
				else if ( !String.IsNullOrEmpty( NickName ) )
				{
					return NickName ;
				}

				return ( !String.IsNullOrEmpty( Name ) ) ? Name : Key ;
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

		public string Group
		{
			get
			{
				string group ;

				if ( IsService )
				{
					group = " Services" ;
				}
				else if ( _rosterItem == null || _presence == null || _presence.Type == PresenceType.unavailable )
				{
					group = " Offline" ;
				}
				else if ( _rosterItem.GetGroups().Count > 0 )
				{
					Group rosterGroup = ( Group ) _rosterItem.GetGroups().Item( 0 ) ;
					group = rosterGroup.Name ;
				}
				else
				{
					group = " Ungrouped" ;
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

				if ( group != Group )
				{
					NotifyPropertyChanged( "Group" ) ;
				}
			}
		}

		public void SetVcard( Vcard vcard )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( vcard != null )
				{
					Birthday = vcard.Birthday ;
					Description = vcard.Description ;
					EmailPreferred = vcard.GetPreferedEmailAddress() ;
					FullName = vcard.Fullname ;
					NickName = vcard.Nickname ;
					Organization = vcard.Organization ;
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
				                                  new SetVcardCallback( SetVcard ), vcard ) ;
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
				_fullName = value ;
				NotifyPropertyChanged( "FullName" ) ;
				NotifyPropertyChanged( "DisplayName" ) ;
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
				_nickName = value ;
				NotifyPropertyChanged( "NickName" ) ;
				NotifyPropertyChanged( "DisplayName" ) ;
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
				_name = value ;
				NotifyPropertyChanged( "Name" ) ;
				NotifyPropertyChanged( "DisplayName" ) ;
			}
		}

		public Organization Organization
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

		public Email EmailPreferred
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

		public string LastMessageFrom
		{
			get
			{
				return _lastMessageFrom ;
			}
			private set
			{
				_lastMessageFrom = value ;
				NotifyPropertyChanged( "LastMessageFrom" ) ;
			}
		}

		public string LastMessageTo
		{
			get
			{
				return _lastMessageTo ;
			}
			
			private set
			{
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
				_subscriptionType = value ;
				NotifyPropertyChanged( "SubscriptionType" ) ;
				NotifyPropertyChanged( "SubscriptionTypeText" ) ;
			}
		}

		private void NotifyPropertyChanged( String info )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( info ) ) ;
			}
		}

		public string SubscriptionTypeText
		{
			get
			{
				return string.Format( "Subscription: {0}", SubscriptionType ) ;
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
				_customName = value ;
				NotifyPropertyChanged( "CustomName" ) ;
				NotifyPropertyChanged( "DisplayName" ) ;
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
				return ( _rosterItem != null /*&& _rosterItem.Jid.Resource == "registered"*/ ) ;
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

		public void Dispose()
		{
			_messages.CollectionChanged -= _messages_CollectionChanged ;
		}
	}
}