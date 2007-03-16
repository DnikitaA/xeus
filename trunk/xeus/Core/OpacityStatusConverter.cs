using System ;
using System.Globalization ;
using System.Windows.Data ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	[ ValueConversion( typeof ( Presence ), typeof ( double ) ) ]
	internal class OpacityStatusConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value != null )
			{
				Presence status = ( Presence ) value ;

				if ( status.Type == PresenceType.available )
				{
					return 1.0 ;
				}

				return 0.4 ;
			}

			return 0.2 ;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException() ;
		}
	}
}