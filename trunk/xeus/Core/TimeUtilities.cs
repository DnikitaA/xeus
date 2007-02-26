using System;
using System.Collections.Generic;
using System.Text;

namespace xeus.Core
{
	public static class TimeUtilities
	{
		public static string FormatRelativeTime( DateTime startTime )
		{
			DateTime now = DateTime.Now ;

			StringBuilder builder = new StringBuilder();

			if ( now.Date == startTime.Date )
			{
				// today
				if ( ( now - startTime ).Seconds <= 10 )
				{
					return "Right now" ;
				}
				else if ( ( now - startTime ).Minutes == 0 )
				{
					// same minute
					builder.AppendFormat( "{0} sec ago",  ( ( now - startTime ).Seconds / 10 * 10 ) ) ;
				}
				else if ( ( now - startTime ).Hours == 0 )
				{
					builder.AppendFormat( "{0} min ago", ( now - startTime ).Minutes ) ;
				}
				else
				{
					builder.AppendFormat( "{0} ago", ( now - startTime ) ) ;
				}
			}
			else if ( ( now.Date - startTime.Date ).Days == 1 )
			{
				// yesterday
			}
			else
			{
				builder.AppendFormat( "{0:D}\n{1} days ago", startTime, ( now.Date - startTime.Date ).Days ) ;
			}

			return builder.ToString() ;
		}
	}
}
