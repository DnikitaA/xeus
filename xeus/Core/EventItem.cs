using System;
using System.Collections.Generic;
using System.Text;
using System.Windows ;
using xeus.Properties ;

namespace xeus.Core
{
	internal abstract class EventItem : IEvent
	{
		private DateTime _recieved = DateTime.Now ;
		protected DateTime _toBeRemoved = DateTime.Now.AddSeconds( Settings.Default.UI_DefaultEventItemSeconds ) ;
		protected string _text = "No text defined" ;

		public virtual DateTime Recieved
		{
			get
			{
				return _recieved ;
			}
		}

		public virtual DateTime ToBeRemoved
		{
			get
			{
				return _toBeRemoved ;
			}
		}

		public virtual string Text
		{
			get
			{
				return _text ;
			}
		}

		public override string ToString()
		{
			return Text ;
		}
	}
}
