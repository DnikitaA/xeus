using System;
using System.Collections.Generic;
using System.Globalization ;
using System.Text;
using System.Windows ;
using System.Windows.Data ;

namespace xeus.Core
{
	[ValueConversion( typeof( double ), typeof( DataTemplate ) )]
	class RosterItemSizeConverter : IValueConverter
	{
		private static DataTemplate _rosterItemBig ;
		private static DataTemplate _rosterItemSmall ;

		static RosterItemSizeConverter()
		{
			_rosterItemBig = ( DataTemplate )App.Current.FindResource( "RosterItemBig" ) ;
			_rosterItemSmall = ( DataTemplate )App.Current.FindResource( "RosterItemSmall" ) ;
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( ( double )value > 100.0 )
			{
				return _rosterItemBig ;
			}
			else
			{
				return _rosterItemSmall ;
			}
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException() ;
		}
	}
}
