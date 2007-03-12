using System;
using System.Collections ;
using System.Collections.Generic;
using System.Text;

namespace xeus.Core
{
	internal class RosterSort : IComparer
	{
		public int Compare( object x, object y )
		{
			RosterItem itemX = ( RosterItem ) x ;
			RosterItem itemY = ( RosterItem ) y ;

			if ( itemX.Group == itemY.Group )
			{
				return string.Compare( itemX.DisplayName, itemY.DisplayName, true ) ;
			}
			else
			{
				bool isSysGroupX = RosterItem.IsSystemGroup( itemX.Group ) ;
				bool isSysGroupY = RosterItem.IsSystemGroup( itemY.Group ) ;
				
				if ( isSysGroupX == isSysGroupY )
				{
					return string.Compare( itemX.Group, itemY.Group ) ;
				}
				else
				{
					return ( isSysGroupX ) ? 1 : -1 ;
				}
			}
		}
	}
}
