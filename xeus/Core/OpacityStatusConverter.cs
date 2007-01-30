using System;
using System.Collections.Generic;
using System.Globalization ;
using System.Text;
using System.Windows.Data ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	[ValueConversion( typeof( Presence ), typeof( double ) )]
	class OpacityStatusConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value != null )
			{
				Presence status = ( Presence )value ;

				if ( status.Type == PresenceType.available )
				{
					return 1.0 ;
				}
				else
				{
					return 0.9 ;
				}
			}

			return 0.9 ;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException() ;
		}
	}
}
