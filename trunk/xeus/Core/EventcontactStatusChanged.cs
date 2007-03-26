using System;
using System.Collections.Generic;
using System.Text;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	class EventContactStatusChanged : EventItem
	{
		public EventContactStatusChanged()
		{
			_text = "Status changed" ;
		}

		public EventContactStatusChanged( RosterItem item )
		{
			_text = string.Format( "Contact {0} changed to {1}",
			                       item.DisplayName, item.StatusText ) ;
		}
	}
}
