using System.Collections.Generic;
using System.Collections.ObjectModel ;
using agsXMPP ;
using agsXMPP.protocol.iq.disco ;

namespace xeus.Core
{
	internal class Services
	{
		ObservableCollectionDisp< ServiceItem > _items = new ObservableCollectionDisp< ServiceItem >( App.DispatcherThread );

		public ObservableCollectionDisp< ServiceItem > SearchEngines
		{
			get
			{
				return GetServicesBySupports( Uri.IQ_SEARCH ) ;
			}
		}

		public ObservableCollectionDisp< ServiceItem > Proxies
		{
			get
			{
				return GetServicesBySupports( Uri.BYTESTREAMS ) ;
			}
		}

		public ObservableCollectionDisp< ServiceItem > Items
		{
			get
			{
				return _items ;
			}
		}

		private ObservableCollectionDisp< ServiceItem > GetServicesBySupports( string filter )
		{
			ObservableCollectionDisp< ServiceItem > filteredServices = new ObservableCollectionDisp< ServiceItem >( App.DispatcherThread ) ;

			foreach ( ServiceItem item in _items )
			{
				if ( item.Disco.HasFeature( filter ) )
				{
					filteredServices.Add( item );
				}
			}

			return filteredServices ;
		}
	}
}