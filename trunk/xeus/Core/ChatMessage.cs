using System ;
using System.Collections.Generic ;
using System.Data.Common ;
using System.Windows.Documents ;
using System.Windows.Media.Imaging ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	internal class ChatMessage : NotifyInfoDispatcher
	{
		private string _from ;
		private string _to ;
		private readonly RosterItem _rosterItem ;
		private DateTime _time ;
		private string _relativeTime ;
		private string _body ;
		private int _id = 0 ;

		public ChatMessage( DbDataReader reader, RosterItem rosterItem )
		{
			Id = ( Int32 ) ( Int64 ) reader[ "Id" ] ;

			_rosterItem = rosterItem ;

			_body = ( string ) reader[ "Body" ] ;
			_from = ( string ) reader[ "SentFrom" ] ;
			_to = ( string ) reader[ "SentTo" ] ;
			_time = DateTime.FromBinary( ( Int64 ) reader[ "Time" ] ) ;
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

		public Dictionary< string, object > GetData()
		{
			Dictionary< string, object > data = new Dictionary< string, object >() ;

			data.Add( "Key", Key ) ;
			data.Add( "Body", Body ) ;
			data.Add( "SentFrom", From ) ;
			data.Add( "SentTo", To ) ;
			data.Add( "Time", _time.ToBinary() ) ;

			return data ;
		}

		public string Key
		{
			get
			{
				return ( SentByMe ) ? To : From ;
			}
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
				if ( _rosterItem == null )
				{
					return Storage.GetDefaultAvatar() ;
				}
				else if ( SentByMe )
				{
					return ( Client.Instance.MyRosterItem != null ) ? Client.Instance.MyRosterItem.Image : null ;
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

		public int Id
		{
			get
			{
				return _id ;
			}
			set
			{
				_id = value ;
			}
		}

	}
}