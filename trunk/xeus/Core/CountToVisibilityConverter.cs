using System ;
using System.Globalization ;
using System.Windows ;
using System.Windows.Data ;

namespace xeus.Core
{
	[ ValueConversion( typeof ( int ), typeof ( Visibility ) ) ]
	internal class CountToVisibilityConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			return ( ( int )value == 0 ) ? Visibility.Hidden : Visibility.Visible ;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException() ;
		}
	}
}