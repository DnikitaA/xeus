using System ;
using System.ComponentModel ;
using System.IO ;
using System.Windows.Controls ;
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

		public event PropertyChangedEventHandler PropertyChanged ;

		public ChatMessage( Message message, RosterItem rosterItem )
		{
			_message = message ;
			_rosterItem = rosterItem ;
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
				if ( _rosterItem == null )
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

		private void NotifyPropertyChanged( String info )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( info ) ) ;
			}
		}
	}
}
