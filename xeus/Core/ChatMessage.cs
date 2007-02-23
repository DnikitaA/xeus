using System ;
using System.ComponentModel ;
using System.IO ;
using System.Windows.Documents ;
using System.Windows.Markup ;
using System.Windows.Media.Imaging ;
using System.Xml ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	class ChatMessage : INotifyPropertyChanged
	{
		private Message _message ;
		private readonly RosterItem _rosterItem ;
		private DateTime _time ;
		private string _relativeTime ;

		public event PropertyChangedEventHandler PropertyChanged ;

		public ChatMessage( Message message, RosterItem rosterItem, DateTime time )
		{
			_message = message ;
			_time = time ;
			_rosterItem = rosterItem ;
			_relativeTime = TimeUtilities.FormatRelativeTime( time ) ;
		}

		public string From
		{
			get
			{
				return _message.From.Bare ;
			}
		}

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

		public FlowDocument Document
		{
			get
			{
				string xaml = HTMLConverter.HtmlToXamlConverter.ConvertHtmlToXaml( Body, true ) ;
				return XamlReader.Load( new XmlTextReader( new StringReader( xaml ) ) ) as FlowDocument ;
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
				return ( _message.From.Bare == Client.Instance.MyJid.Bare ) ;
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

		private void NotifyPropertyChanged( String info )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( info ) ) ;
			}
		}
	}
}
