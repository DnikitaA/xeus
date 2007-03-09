using System;
using System.Collections.Generic;
using System.Globalization ;
using System.Text;
using System.Windows.Data ;

namespace xeus.Core
{
	[ValueConversion( typeof( double ), typeof( double ) )]
	internal class SizeFromSliderConverterSmall : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			// value is from 50 to 200
			if ( value != null )
			{
				double sliderValue = ( double )value ;

				if ( sliderValue <= 150.0 )
				{
					// small item
					return sliderValue + 80.0 ;
				}
				else
				{
					// big item
					return sliderValue - 40.0 ;
				}
			}

			return 130.0 ;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException() ;
		}
	}
}
