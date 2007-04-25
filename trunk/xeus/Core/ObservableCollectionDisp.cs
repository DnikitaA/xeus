using System ;
using System.Collections.ObjectModel ;
using System.Windows.Threading ;

namespace xeus.Core
{
	public class ObservableCollectionDisp< T > : ObservableCollection< T >
	{
		public readonly object _syncObject = new object();

		private Dispatcher _dispatcherUIThread ;

		private delegate void SetItemCallback( int index, T item ) ;

		private delegate void RemoveItemCallback( int index ) ;

		private delegate void ClearItemsCallback() ;

		private delegate void InsertItemCallback( int index, T item ) ;

		private delegate void MoveItemCallback( int oldIndex, int newIndex ) ;

		public ObservableCollectionDisp( Dispatcher dispatcher )
		{
			_dispatcherUIThread = dispatcher ;
		}

		protected override void SetItem( int index, T item )
		{
			if ( _dispatcherUIThread.CheckAccess() )
			{
				lock ( _syncObject )
				{
					base.SetItem( index, item ) ;
				}
			}
			else
			{
				_dispatcherUIThread.BeginInvoke( DispatcherPriority.Normal,
				                                new SetItemCallback( SetItem ), index, new object[] { item } ) ;
			}
		}

		protected override void InsertItem( int index, T item )
		{
			if ( _dispatcherUIThread.CheckAccess() )
			{
				lock ( _syncObject )
				{
					if ( index > Count )
					{
						base.InsertItem( Count, item );
					}
					else
					{
						base.InsertItem( index, item );
					}
				}
			}
			else
			{
				_dispatcherUIThread.BeginInvoke( DispatcherPriority.Normal,
				                                new InsertItemCallback( InsertItem ), index, new object[] { item } ) ;
			}
		}


		protected override void RemoveItem( int index )
		{
			if ( _dispatcherUIThread.CheckAccess() )
			{
				lock ( _syncObject )
				{
					base.RemoveItem( index ) ;
				}
			}
			else
			{
				_dispatcherUIThread.BeginInvoke( DispatcherPriority.Normal,
				                                new RemoveItemCallback( RemoveItem ), index, new object[] { } ) ;
			}
		}

		protected override void MoveItem( int oldIndex, int newIndex )
		{
			if ( _dispatcherUIThread.CheckAccess() )
			{
				lock ( _syncObject )
				{
					base.MoveItem( oldIndex, newIndex ) ;
				}
			}
			else
			{
				_dispatcherUIThread.BeginInvoke( DispatcherPriority.Normal,
				                                new MoveItemCallback( MoveItem ), oldIndex, new object[] { newIndex } ) ;
			}
		}

		protected override void ClearItems()
		{
			if ( _dispatcherUIThread.CheckAccess() )
			{
				lock ( _syncObject )
				{
					base.ClearItems() ;
				}
			}
			else
			{
				_dispatcherUIThread.BeginInvoke( DispatcherPriority.Normal, new ClearItemsCallback( ClearItems ) ) ;
			}
		}
	}
}