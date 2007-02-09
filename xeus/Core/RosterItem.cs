using System ;
using System.ComponentModel ;
using System.Windows.Controls ;
using System.Windows.Media.Imaging ;
using System.Windows.Threading ;
using agsXMPP.protocol.Base ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.vcard ;

namespace xeus.Core
{
	internal class RosterItem : INotifyPropertyChanged
	{
		private ObservableCollectionDisp< Message > _messages =
			new ObservableCollectionDisp< Message >( App.DispatcherThread ) ;

		private ObservableCollectionDisp< string > _errors =
			new ObservableCollectionDisp< string >( App.DispatcherThread ) ;

		private delegate void SetVcardCallback( Vcard vcard ) ;

		private agsXMPP.protocol.iq.roster.RosterItem _rosterItem ;
		private string _statusText = "Not Available" ;

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

		public event PropertyChangedEventHandler PropertyChanged ;

		public Presence _presence ;
		private string _statusDescription = "Not Available" ;

		public RosterItem( agsXMPP.protocol.iq.roster.RosterItem rosterItem )
		{
			_rosterItem = rosterItem ;
		}

		public agsXMPP.protocol.iq.roster.RosterItem XmppRosterItem
		{
			get
			{
				return _rosterItem ;
			}
		}

		public string Key
		{
			get
			{
				return _rosterItem.Jid.Bare ;
			}
		}

		public string DisplayName
		{
			get
			{
				if ( !String.IsNullOrEmpty( FullName ) )
				{
					return FullName ;
				}
				else if ( !String.IsNullOrEmpty( NickName ) )
				{
					return NickName ;
				}

				return ( _rosterItem.Name != null ) ? _rosterItem.Name : _rosterItem.Jid.ToString() ;
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
				if ( _presence == null || _presence.Type == PresenceType.unavailable )
				{
					return "<offline>" ;
				}
				else if ( _rosterItem.Jid.User == null )
				{
					return "<services>" ;
				}
				else if ( _rosterItem.GetGroups().Count > 0 )
				{
					Group group = ( Group ) _rosterItem.GetGroups().Item( 0 ) ;
					return group.Name ;
				}
				else
				{
					return "<none>" ;
				}
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

				if ( ( _presence == null && value != null )
					|| ( _presence != null && value == null ) )
				{
					_errors.Clear();
				}

				_presence = value ;
				_statusDescription = "Not Available" ;

				if ( _presence == null )
				{
					_statusText = "Not Available" ;
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
					if ( XmppRosterItem.Jid.User == null )
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
				App.DispatcherThread.BeginInvoke( DispatcherPriority.ApplicationIdle,
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

		public ObservableCollectionDisp< Message > Messages
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

		private void NotifyPropertyChanged( String info )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( info ) ) ;
			}
		}
	}
}