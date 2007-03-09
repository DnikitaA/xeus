using System ;
using System.Collections.Generic;
using System.Collections.ObjectModel ;
using agsXMPP.protocol.iq.disco ;
using agsXMPP.protocol.iq.register ;
using Uri=agsXMPP.Uri;

namespace xeus.Core
{
	public class Services
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

		public ServiceItem FindItem( string bare )
		{
			lock ( this )
			{
				foreach ( ServiceItem item in _items )
				{
					if ( string.Compare( item.Jid.Bare, bare, true ) == 0 )
					{
						return item ;
					}
				}
			}

			return null ;
		}

		public void UnregisterService( ServiceItem service )
		{
			Client.Instance.UnregisterService( service.Jid );
		}

		public void RegisterService( ServiceItem service )
		{
			Client.Instance.RegisterService( service.Jid );
		}
	}
}
