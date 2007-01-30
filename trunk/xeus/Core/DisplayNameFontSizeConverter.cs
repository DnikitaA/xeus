using System ;
using System.Globalization ;
using System.Windows.Data ;

namespace xeus.Core
{
	[ValueConversion( typeof( double ), typeof( double ) )]
	internal class DisplayNameFontSizeConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			// value is from 50 to 200
			if ( value != null )
			{
				double width = ( double )value ;

				if ( width < 100.0 )
				{
					return 12.0 ;
				}

				return width / 10.0 ;
			}

			return 14.0 ;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException() ;
		}
	}
}