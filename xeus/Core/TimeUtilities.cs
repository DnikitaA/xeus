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
				if ( ( now - startTime ).TotalSeconds <= 10 )
				{
					return "Right now" ;
				}
				else if ( Math.Round( ( now - startTime ).TotalMinutes, 0 ) == 0 )
				{
					// same minute
					builder.AppendFormat( "{0} sec ago",  Math.Round( ( now - startTime ).TotalSeconds / 10 * 10, 0 ) ) ;
				}
				else if ( Math.Round( ( now - startTime ).TotalHours, 0 ) == 0 )
				{
					builder.AppendFormat( "{0} min ago", Math.Round( ( now - startTime ).TotalMinutes, 0 ) ) ;
				}
				else
				{
					builder.AppendFormat( "{0} ago", ( now - startTime ) ) ;
				}
			}
			else if ( ( now.Date - startTime.Date ).TotalDays == 1 )
			{
				// yesterday
				builder.AppendFormat( "yesterday {0}", startTime.TimeOfDay ) ;
			}
			else
			{
				builder.AppendFormat( "{0:D}\n{1} days ago", startTime, Math.Round( ( now.Date - startTime.Date ).TotalDays, 0 ) ) ;
			}

			return builder.ToString() ;
		}
	}
}
