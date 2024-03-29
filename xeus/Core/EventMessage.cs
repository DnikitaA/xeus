using System;
using System.Collections.Generic;
using System.Text;
using xeus.Properties ;

namespace xeus.Core
{
	internal class EventMessage : EventItem
	{
		private RosterItem _rosterItem ;
		private readonly ChatMessage _chatMessage ;

		public EventMessage( RosterItem rosterItem, ChatMessage chatMessage )
		{
			_rosterItem = rosterItem ;
			_chatMessage = chatMessage ;

			_toBeRemoved = Recieved.AddSeconds( Settings.Default.UI_MessageEventItemSeconds ) ;
			
		}

		public RosterItem RosterItem
		{
			get
			{
				return _rosterItem ;
			}

			set
			{
				_rosterItem = value ;
			}
		}

		public ChatMessage ChatMessage
		{
			get
			{
				return _chatMessage ;
			}
		}

		public void ResetTime()
		{
			_toBeRemoved = DateTime.MinValue ;
		}
	}
}
