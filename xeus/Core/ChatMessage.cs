using System ;
using System.ComponentModel ;
using System.Data ;
using System.Windows.Media.Imaging ;
using agsXMPP.protocol.client ;
using Clifton.Tools.Xml ;

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
		private bool _isFromDb = false ;
		private string _threadId = null ;

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
			_threadId = message.Thread ;
			_body = message.Body ;
			_time = time ;
			_rosterItem = rosterItem ;
			_from = message.From.Bare ;
			_to = message.To.Bare ;
			_relativeTime = TimeUtilities.FormatRelativeTime( time ) ;
		}

		public XmlDatabase.FieldValuePair[] GetData()
		{
			XmlDatabase.FieldValuePair[] data = new XmlDatabase.FieldValuePair[ 5 ] ;

			string key = ( SentByMe ) ? To : From ;

			data[ 0 ] = new NullFieldValuePair( "Key", key ) ;
			data[ 1 ] = new NullFieldValuePair( "From", From ) ;
			data[ 2 ] = new NullFieldValuePair( "To", To ) ;
			data[ 3 ] = new NullFieldValuePair( "Time", Time.ToBinary().ToString() ) ;
			data[ 4 ] = new NullFieldValuePair( "Body", Body ) ;

			return data ;
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

		public string ThreadId
		{
			get
			{
				return _threadId ;
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

			set
			{
				_isFromDb = value ;
			}
		}
	}
}