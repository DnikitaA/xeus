using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	class EventContactStatusChanged : EventItem
	{
		private readonly Presence _oldPresence ;
		private readonly Presence _newPresence ;
		private readonly RosterItem _rosterItem ;

		public EventContactStatusChanged( RosterItem rosterItem, Presence oldPresence )
		{
			_oldPresence = oldPresence ;
			_rosterItem = rosterItem ;
			_newPresence = rosterItem.Presence ;

			_text = string.Format( "{0} is {1}", rosterItem.DisplayName, rosterItem.StatusText ) ;
		}

		public ControlTemplate OldPresenceTemplate
		{
			get
			{
				return PresenceTemplate.GetStatusTemplate( _oldPresence ) ;
			}
		}

		public ControlTemplate NewPresenceTemplate
		{
			get
			{
				return PresenceTemplate.GetStatusTemplate( _newPresence ) ;
			}
		}

		public RosterItem RosterItem
		{
			get
			{
				return _rosterItem ;
			}
		}
	}
}
