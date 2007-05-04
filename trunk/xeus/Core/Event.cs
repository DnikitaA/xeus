using System;
using System.Collections.Generic;
using System.Text;
using System.Timers ;
using System.Windows.Threading ;
using agsXMPP.protocol.client ;
using xeus.Properties ;

namespace xeus.Core
{
	internal class Event
	{
		private ObservableCollectionDisp< IEvent > _items =
			new ObservableCollectionDisp< IEvent >( App.Current.Dispatcher ) ;

		Timer _refreshTimer = new Timer( 1000 ) ;

		public delegate void RemoveItemHandler( int index ) ;

		public Event()
		{
			_refreshTimer.Start();
			_refreshTimer.AutoReset = false ;
			_refreshTimer.Elapsed += new ElapsedEventHandler( _refreshTimer_Elapsed );
		}

		void RemoveItem( int index )
		{
			if ( App.Current.Dispatcher.CheckAccess() )
			{
				if ( _items.Count > index )
				{
					_items.RemoveAt( index ) ;
				}
			}
			else
			{
				App.Current.Dispatcher.BeginInvoke( DispatcherPriority.Send,
				                                  new RemoveItemHandler( RemoveItem ), index ) ;
			}
		}

		void _refreshTimer_Elapsed( object sender, ElapsedEventArgs e )
		{
			lock ( _items._syncObject )
			{
				DateTime now = DateTime.Now ;
				
				for ( int i = _items.Count - 1; i >= 0; i-- )
				{
					if ( Items[ i ].ToBeRemoved < now )
					{
						RemoveItem( i ) ;
					}
				}
			}

			_refreshTimer.Start();
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
				_items.Insert( 0, theEvent ) ;
			}

			if ( Settings.Default.UI_Sound )
			{
				System.Media.SystemSounds.Beep.Play() ;
			}
		}

		public void MessageWindowActivated()
		{
			lock ( _items._syncObject )
			{
				foreach ( IEvent item in _items )
				{
					EventMessage message = item as EventMessage ;

					if ( message != null )
					{
						message.ResetTime() ;
					}
				}
			}
		}
	}
}
