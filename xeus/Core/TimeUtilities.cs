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
				else if ( ( now - startTime ).TotalMinutes < 1 )
				{
					// same minute
					builder.AppendFormat( "{0} sec ago",  Math.Round( ( now - startTime ).TotalSeconds / 10 * 10, 0 ) ) ;
				}
				else if ( ( now - startTime ).TotalHours < 1 )
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
				builder.AppendFormat( "yesterday {0:00}:{1:00}", startTime.TimeOfDay.Hours, startTime.TimeOfDay.Minutes ) ;
			}
			else if ( ( now.Date - startTime.Date ).TotalDays < 5 )
			{
				builder.AppendFormat( "{0} days ago, {1:00}:{2:00}", Math.Round( ( now.Date - startTime.Date ).TotalDays, 0 ), startTime.TimeOfDay.Hours, startTime.TimeOfDay.Minutes ) ;
			}
			else
			{
				builder.Append( startTime.ToString() ) ;
			}

			return builder.ToString() ;
		}
	}
}
