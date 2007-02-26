using System ;
using System.ComponentModel ;
using System.Data ;
using System.Windows.Media.Imaging ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	internal class ChatMessage : INotifyPropertyChanged
	{
		private string _from ;
		private string _to ;
		private readonly RosterItem _rosterItem ;
		private DateTime _time ;
		private string _relativeTime ;
		private string _body ;
		private bool _isFromDb = false ;

		public event PropertyChangedEventHandler PropertyChanged ;

		public ChatMessage( DataRow row, RosterItem rosterItem )
		{
			_isFromDb = true ;
			_rosterItem = rosterItem ;

			_body = row[ "Body" ] as string ;
			_from = row[ "From" ] as string ;
			_to = row[ "To" ] as string ;
			_time = DateTime.FromBinary( long.Parse( row[ "Time" ] as string ) ) ;
			_relativeTime = TimeUtilities.FormatRelativeTime( _time ) ;
		}

		public ChatMessage( Message message, RosterItem rosterItem, DateTime time )
		{
			_body = message.Body ;
			_time = time ;
			_rosterItem = rosterItem ;
			_from = message.From.Bare ;
			_to = message.To.Bare ;
			_relativeTime = TimeUtilities.FormatRelativeTime( time ) ;
		}

		public string From
		{
			get
			{
				return _from ;
			}
		}

		public string Body
		{
			get
			{
				return _body ;
			}
		}

		public BitmapImage Image
		{
			get
			{
				if ( _rosterItem == null || SentByMe )
				{
					return Storage.GetDefaultAvatar() ;
				}
				else
				{
					return _rosterItem.Image ;
				}
			}
		}

		public bool SentByMe
		{
			get
			{
				return ( _from == Client.Instance.MyJid.Bare ) ;
			}
		}

		public DateTime Time
		{
			get
			{
				return _time ;
			}
		}

		public string RelativeTime
		{
			get
			{
				return _relativeTime ;
			}

			set
			{
				_relativeTime = value ;
				NotifyPropertyChanged( "RelativeTime" ) ;
			}
		}

		public string To
		{
			get
			{
				return _to ;
			}
		}

		public bool IsFromDb
		{
			get
			{
				return _isFromDb ;
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