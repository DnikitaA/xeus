using System ;
using System.Collections.ObjectModel ;
using System.Windows.Threading ;

namespace xeus.Core
{
	public class ObservableCollectionDisp< T > : ObservableCollection< T >
	{
		public readonly object _syncObject = new object();

		private Dispatcher dispatcherUIThread ;

		private delegate void SetItemCallback( int index, T item ) ;

		private delegate void RemoveItemCallback( int index ) ;

		private delegate void ClearItemsCallback() ;

		private delegate void InsertItemCallback( int index, T item ) ;

		private delegate void MoveItemCallback( int oldIndex, int newIndex ) ;

		public ObservableCollectionDisp( Dispatcher dispatcher )
		{
			dispatcherUIThread = dispatcher ;
		}

		protected override void SetItem( int index, T item )
		{
			if ( dispatcherUIThread.CheckAccess() )
			{
				base.SetItem( index, item ) ;
			}
			else
			{
				dispatcherUIThread.BeginInvoke( DispatcherPriority.Send,
				                                new SetItemCallback( SetItem ), index, new object[] { item } ) ;
			}
		}

		protected override void InsertItem( int index, T item )
		{
			if ( dispatcherUIThread.CheckAccess() )
			{
				base.InsertItem( index, item ) ;
			}
			else
			{
				dispatcherUIThread.BeginInvoke( DispatcherPriority.Send,
				                                new InsertItemCallback( InsertItem ), index, new object[] { item } ) ;
			}
		}

		protected override void RemoveItem( int index )
		{
			if ( dispatcherUIThread.CheckAccess() )
			{
				base.RemoveItem( index ) ;
			}
			else
			{
				dispatcherUIThread.BeginInvoke( DispatcherPriority.Send,
				                                new RemoveItemCallback( RemoveItem ), index, new object[] { } ) ;
			}
		}

		protected override void MoveItem( int oldIndex, int newIndex )
		{
			if ( dispatcherUIThread.CheckAccess() )
			{
				base.MoveItem( oldIndex, newIndex ) ;
			}
			else
			{
				dispatcherUIThread.BeginInvoke( DispatcherPriority.Send,
				                                new MoveItemCallback( MoveItem ), oldIndex, new object[] { newIndex } ) ;
			}
		}

		protected override void ClearItems()
		{
			if ( dispatcherUIThread.CheckAccess() )
			{
				base.ClearItems() ;
			}
			else
			{
				dispatcherUIThread.BeginInvoke( DispatcherPriority.Send, new ClearItemsCallback( ClearItems ) ) ;
			}
		}
	}
}