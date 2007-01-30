using System;
using System.Collections.Generic;
using System.Globalization ;
using System.Text;
using System.Windows ;
using System.Windows.Data ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	[ValueConversion( typeof( Presence ), typeof( Visibility ) )]
	class RosterItemVisibleConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value != null )
			{
				Presence status = ( Presence )value ;

				/*if ( status == ShowType.NONE )
				{
					return Visibility.Collapsed ;
				}
				else*/
				{
					return Visibility.Visible ;
				}
			}

			return Visibility.Visible ;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException() ;
		}
	}
}
