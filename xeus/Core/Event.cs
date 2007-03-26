using System;
using System.Collections.Generic;
using System.Text;
using System.Timers ;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	internal class Event
	{
		private ObservableCollectionDisp< IEvent > _items =
			new ObservableCollectionDisp< IEvent >( App.DispatcherThread ) ;

		Timer _refreshTimer = new Timer( 1000 ) ;

		public Event()
		{
			_refreshTimer.Start();
			_refreshTimer.Elapsed += new ElapsedEventHandler( _refreshTimer_Elapsed );
		}

		void _refreshTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			lock ( _items._syncObject )
			{
				IEvent [] events = new IEvent[ _items.Count ] ;
				_items.CopyTo( events, 0 ) ;

				DateTime now = DateTime.Now ;

				foreach ( IEvent item in events )
				{
					if ( item.ToBeRemoved < now )
					{
						_items.Remove( item ) ;
					}
				}
			}
		}

		public ObservableCollectionDisp< IEvent > Items
		{
			get
			{
				return _items ;
			}
		}

		public void AddEvent( IEvent theEvent )
		{
			lock ( _items._syncObject )
			{
				_items.Add( theEvent ) ;
			}
		}
	}
}
