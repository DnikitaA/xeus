using System;
using System.Collections.Generic;
using System.Text;

namespace xeus.Core
{
	internal class EventInfo : EventItem
	{
		private string _title = String.Empty ;

		public EventInfo( string text, string title )
		{
			_title = title ;
			_text = text ;
		}

		public string Title
		{
			get
			{
				return _title ;
			}
		}
	}
}
