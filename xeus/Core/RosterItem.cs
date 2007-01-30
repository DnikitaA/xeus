using System ;
using System.ComponentModel ;
using System.IO ;
using System.Net ;
using System.Windows.Controls ;
using System.Windows.Media.Imaging ;
using agsXMPP.protocol.Base ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.iq.vcard ;

namespace xeus.Core
{
	internal class RosterItem : INotifyPropertyChanged
	{
		private agsXMPP.protocol.iq.roster.RosterItem _rosterItem ;
		private string _statusText = "Offline" ;

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

		private ControlTemplate _templateDnd ;
		private ControlTemplate _templateAway ;
		private ControlTemplate _templateFreeForChat ;
		private ControlTemplate _templateXAway ;

		public event PropertyChangedEventHandler PropertyChanged ;

		public Presence _presence ;
		private string _statusDescription = String.Empty ;

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
				return ( _rosterItem.Name != null ) ? _rosterItem.Name : _rosterItem.Jid.ToString() ;
			}
		}

		public ControlTemplate StatusTemplate
		{
			get
			{
				if ( _presence == null )
				{
					return null ;
				}

				switch ( _presence.Show )
				{
					case ShowType.dnd:
						{
							if ( _templateDnd == null )
							{
								_templateDnd = ( ControlTemplate ) App.Instance.FindResource( "StatusDnd" ) ;
							}
							return _templateDnd ;
						}

					case ShowType.away:
						{
							if ( _templateAway == null )
							{
								_templateAway = ( ControlTemplate ) App.Instance.FindResource( "StatusAway" ) ;
							}
							return _templateAway ;
						}

					case ShowType.chat:
						{
							if ( _templateFreeForChat == null )
							{
								_templateFreeForChat = ( ControlTemplate ) App.Instance.FindResource( "StatusFreeForChat" ) ;
							}
							return _templateFreeForChat ;
						}

					case ShowType.xa:
						{
							if ( _templateXAway == null )
							{
								_templateXAway = ( ControlTemplate ) App.Instance.FindResource( "StatusXAway" ) ;
							}
							return _templateXAway ;
						}

					default:
						{
							return null ;
						}
				}
			}
		}

		public string Group
		{
			get
			{
				if ( _rosterItem.GetGroups().Count > 0 )
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
				_presence = value ;

				if ( _presence == null )
				{
					_statusText = "<not available>" ;
					_statusDescription = String.Empty ;
				}
				else
				{
					if ( _presence.Status != null )
					{
						_statusDescription = _presence.Status ;
					}
					else
					{
						_statusDescription = String.Empty ;
					}

					switch ( _presence.Show )
					{
						case ShowType.NONE:
							{
								_statusText = "Online" ;
								break ;
							}
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
								_statusText = "Unknown" ;
								break ;
							}
					}
				}

				NotifyPropertyChanged( "Presence" ) ;
				NotifyPropertyChanged( "StatusText" ) ;
				NotifyPropertyChanged( "StatusTemplate" ) ;
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
		}

		public void SetPhoto( Photo photo )
		{
			if ( photo == null )
			{
				_image = null ;
			}
			else
			{
				try
				{
					if ( photo.HasTag( "BINVAL" ) )
					{
						byte[] pic = Convert.FromBase64String( photo.GetTag( "BINVAL" ) ) ;
						MemoryStream memoryStream = new MemoryStream( pic, 0, pic.Length ) ;
						BitmapImage bitmap = new BitmapImage() ;
						bitmap.BeginInit() ;
						bitmap.StreamSource = memoryStream ;
						bitmap.EndInit() ;
						_image = bitmap ;
					}
					else if ( photo.HasTag( "EXTVAL" ) )
					{
						WebRequest webRequest = WebRequest.Create( photo.GetTag( "EXTVAL" ) ) ;
						WebResponse response = webRequest.GetResponse() ;

						BitmapImage bitmap = new BitmapImage() ;
						bitmap.BeginInit() ;
						bitmap.StreamSource = response.GetResponseStream() ;
						bitmap.EndInit() ;

						response.Close();

						_image = bitmap ;
					}
					else if ( photo.TextBase64.Length > 0 )
					{
						byte[] pic = Convert.FromBase64String( photo.Value ) ;
						MemoryStream memoryStream = new MemoryStream( pic, 0, pic.Length ) ;
						BitmapImage bitmap = new BitmapImage() ;
						bitmap.BeginInit() ;
						bitmap.StreamSource = memoryStream ;
						bitmap.EndInit() ;
						_image = bitmap ;
					}
					else
					{
						_image = null ;
					}
				}
				catch
				{
					_image = null ;
				}
			}

			NotifyPropertyChanged( "Image" ) ;
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