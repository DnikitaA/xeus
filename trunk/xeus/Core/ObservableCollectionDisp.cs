using System ;
using System.Collections.ObjectModel ;
using System.ComponentModel ;
using System.Text ;
using System.Windows.Threading ;

namespace xeus.Core
{
	public class ObservableCollectionDisp< T > : ObservableCollection< T >, INotifyPropertyChanged
	{
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

		public string FlatText
		{
			get
			{
				StringBuilder builder = new StringBuilder( 256 );

				foreach ( T item in Items )
				{
					if ( builder.Length > 0 )
					{
						builder.AppendLine() ;
					}

					builder.Append( item.ToString() ) ;
				}

				return builder.ToString() ;
			}
		}

		protected override void SetItem( int index, T item )
		{
			if ( dispatcherUIThread.CheckAccess() )
			{
				base.SetItem( index, item ) ;
				NotifyPropertyChanged( "FlatText" ) ;
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
				NotifyPropertyChanged( "FlatText" ) ;
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
				NotifyPropertyChanged( "FlatText" ) ;
			}
			else
			{
				dispatcherUIThread.BeginInvoke( DispatcherPriority.Send,
				                                new RemoveItemCallback( RemoveItem ), index, new object[] {} ) ;
			}
		}

		protected override void MoveItem( int oldIndex, int newIndex )
		{
			if ( dispatcherUIThread.CheckAccess() )
			{
				base.MoveItem( oldIndex, newIndex ) ;
				NotifyPropertyChanged( "FlatText" ) ;
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
				NotifyPropertyChanged( "FlatText" ) ;
			}
			else
			{
				dispatcherUIThread.BeginInvoke( DispatcherPriority.Send, new ClearItemsCallback( ClearItems ) ) ;
			}
		}

		private void NotifyPropertyChanged( String info )
		{
			base.OnPropertyChanged( new PropertyChangedEventArgs( info ) );
		}
	}
}