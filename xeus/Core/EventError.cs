using System;
using System.Collections.Generic;
using System.Text;

namespace xeus.Core
{
	internal class EventError : EventItem
	{
		private string _title = String.Empty ;

		public EventError( string text, string title )
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
