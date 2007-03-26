using System;
using System.Collections.Generic;
using System.Text;

namespace xeus.Core
{
	internal interface IEvent
	{
		DateTime Recieved { get ; }
		DateTime ToBeRemoved { get ; }
		string Text { get ; }
	}
}
