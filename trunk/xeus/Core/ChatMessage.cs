using System ;
using System.ComponentModel ;
using System.IO ;
using System.Windows.Documents ;
using System.Windows.Markup ;
using System.Windows.Media.Imaging ;
using System.Xml ;
using System.Xml.Serialization ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	[Serializable]
	class ChatMessage : INotifyPropertyChanged
	{
		private Message _message ;
		private readonly RosterItem _rosterItem ;
		private DateTime _time ;
		private string _relativeTime ;

		public event PropertyChangedEventHandler PropertyChanged ;

		public ChatMessage()
		{
		}

		public ChatMessage( Message message, RosterItem rosterItem, DateTime time )
		{
			_message = message ;
			_time = time ;
			_rosterItem = rosterItem ;
			_relativeTime = TimeUtilities.FormatRelativeTime( time ) ;
		}

		public string MessageInnerXml
		{
			get
			{
				return _message.InnerXml ;
			}

			set
			{
				if ( _message == null )
				{
					_message = new Message();
				}

				_message.InnerXml = value ;
			}
		}

		[XmlIgnore]
		public string From
		{
			get
			{
				return _message.From.Bare ;
			}
		}

		[XmlIgnore]
		public string Body
		{
			get
			{
				if ( _message.Html == null )
				{
					return _message.Body ;
				}
				else
				{
					return _message.Html.Body.InnerHtml ;
				}
			}
		}

		[XmlIgnore]
		public FlowDocument Document
		{
			get
			{
				string xaml = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml( Body, true ) ;
				return XamlReader.Load( new XmlTextReader( new StringReader( xaml ) ) ) as FlowDocument ;
			}
		}

		[XmlIgnore]
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

		[XmlIgnore]
		public bool SentByMe
		{
			get
			{
				return ( _message.From.Bare == Client.Instance.MyJid.Bare ) ;
			}
		}

		public DateTime Time
		{
			get
			{
				return _time ;
			}
			
			set
			{
				_time = value ;
			}
		}

		[XmlIgnore]
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

		private void NotifyPropertyChanged( String info )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( info ) ) ;
			}
		}
	}
}
