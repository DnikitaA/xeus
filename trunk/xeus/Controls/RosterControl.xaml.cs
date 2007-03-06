using System ;
using System.Collections ;
using System.Collections.Specialized ;
using System.ComponentModel ;
using System.Threading ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Data ;
using System.Windows.Input ;
using System.Windows.Threading ;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for RoosterControl.xaml
	/// </summary>
	public partial class RosterControl : UserControl
	{
		private static DataTemplate _rosterItemBig ;
		private static DataTemplate _rosterItemSmall ;

		private ICollectionView _rosterView ;
		private InlineSearch _inlineSearch ;
		InlineMethod _inlineMethod = new InlineMethod();

		private delegate void SelectItemCallback( RosterItem item ) ;

		public RosterControl()
		{
			InitializeComponent() ;

			_rosterItemBig = ( DataTemplate ) App.Current.FindResource( "RosterItemBig" ) ;
			_rosterItemSmall = ( DataTemplate ) App.Current.FindResource( "RosterItemSmall" ) ;

			Client.Instance.Roster.Items.CollectionChanged += new NotifyCollectionChangedEventHandler( Items_CollectionChanged ) ;

			_roster.ItemTemplate = _rosterItemSmall ;

			_sliderItemSize.ValueChanged += new RoutedPropertyChangedEventHandler< double >( _sliderItemSize_ValueChanged ) ;
			_inlineMethod.Finished += new InlineMethod.InlineResultHandler( _inlineMethod_Finished );

			_roster.SelectionChanged += new SelectionChangedEventHandler( _roster_SelectionChanged );

			Client.Instance.Roster.ReadRosterFromDb() ;
		}

		void _roster_SelectionChanged( object sender, SelectionChangedEventArgs e )
		{
			/*ListBoxItem item = _roster.ItemContainerGenerator.ContainerFromIndex( _roster.SelectedIndex ) as ListBoxItem;
			foreach ( object o in _roster.ItemsSource )
			{
				
			}*/
		}

		void _inlineSearch_Closed( bool isEnter )
		{
			_roster.Focus() ;

			if ( isEnter )
			{
				
			}
		}

		void _inlineMethod_Finished( object result )
		{
			RosterItem rosterItem = ( RosterItem ) result ;
			SelectItem( rosterItem ) ;
		}

		private void SelectItem( RosterItem item )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( item == null )
				{
					_inlineSearch.NotFound = true ;
				}
				else
				{
					_roster.SelectedItem = item ;
					
					_roster.ScrollIntoView( item );
					_inlineSearch.NotFound = false ;
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new SelectItemCallback( SelectItem ), item ) ;
			}
		}

		public InlineSearch InlineSearch
		{
			get
			{
				return _inlineSearch ;
			}
			set
			{
				if ( _inlineSearch != null )
				{
					_inlineSearch.TextChanged -= _inlineSearch_TextChanged ;
					_inlineSearch.Closed -= _inlineSearch_Closed ;
				}
				_inlineSearch = value ;
				_inlineSearch.TextChanged += new TextChangedEventHandler(_inlineSearch_TextChanged);
				_inlineSearch.Closed += new InlineSearch.ClosedHandler( _inlineSearch_Closed );
			}
		}

		object SearchInList( ref bool stop, object param )
		{
			RosterItem found = null ;

			foreach ( RosterItem rosterItem in Client.Instance.Roster.Items )
			{
				if ( stop )
				{
					return null ;
				}

				if ( ( ( string )param ) == String.Empty )
				{
					return null ;
				}
				
				if ( rosterItem.DisplayName.StartsWith( ( string )param, StringComparison.OrdinalIgnoreCase ) )
				{
					found = rosterItem ;
					break ;
				}
			}

			return found ;
		}

		void _inlineSearch_TextChanged( object sender, TextChangedEventArgs e )
		{
			_inlineMethod.Go( new InlineParam( SearchInList, _inlineSearch.Text ) );
		}

		void OnKeyDown( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Delete )
			{
				RosterItem rosterItem = _roster.SelectedItem as RosterItem ;
				if ( rosterItem != null )
				{
					Client.Instance.Roster.DeleteRosterItem( rosterItem ) ;
				}
			}
			else if ( e.Key == Key.Return )
			{
				RosterItem rosterItem = _roster.SelectedItem as RosterItem ;
				OpenMessageWindow( rosterItem ) ;
			}
			else if ( InlineSearch != null )
			{
				InlineSearch.SendKey( e.Key ) ;
			}
		}

		private void SubscribeItems( IList items )
		{
			foreach ( RosterItem item in items )
			{
				item.PropertyChanged += new PropertyChangedEventHandler( item_PropertyChanged ) ;
			}
		}

		private void UnsubscribeItems( IList items )
		{
			foreach ( RosterItem item in items )
			{
				item.PropertyChanged -= new PropertyChangedEventHandler( item_PropertyChanged ) ;
			}
		}

		private void item_PropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			switch ( e.PropertyName )
			{
				case "Group":
					{
						if ( _rosterView == null )
						{
							_rosterView = CollectionViewSource.GetDefaultView( _roster.ItemsSource ) ;
						}

						//_rosterView.Refresh() ;
						Client.Instance.Roster.Items.Remove( ( RosterItem ) sender ) ;
						Client.Instance.Roster.Items.Add( ( RosterItem ) sender );
						break ;
					}
			}
		}

		private void Items_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					{
						SubscribeItems( e.NewItems ) ;
						break ;
					}
				case NotifyCollectionChangedAction.Remove:
					{
						UnsubscribeItems( e.OldItems ) ;
						break ;
					}
				case NotifyCollectionChangedAction.Replace:
					{
						UnsubscribeItems( e.OldItems ) ;
						break ;
					}
				case NotifyCollectionChangedAction.Reset:
					{
						UnsubscribeItems( e.OldItems ) ;
						break ;
					}
			}
		}

		private void _sliderItemSize_ValueChanged( object sender, RoutedPropertyChangedEventArgs< double > e )
		{
			if ( e.NewValue > 100.0 )
			{
				if ( _roster.ItemTemplate != _rosterItemBig )
				{
					_roster.ItemTemplate = _rosterItemBig ;
				}
			}
			else
			{
				if ( _roster.ItemTemplate != _rosterItemSmall )
				{
					_roster.ItemTemplate = _rosterItemSmall ;
				}
			}
		}

		private void OnDoubleClickRosterItem( object sender, MouseButtonEventArgs e )
		{
			RosterItem rosterItem = _roster.SelectedItem as RosterItem ;
			OpenMessageWindow( rosterItem ) ;
		}

		private void OpenMessageWindow( RosterItem rosterItem )
		{
			if ( rosterItem != null )
			{
				MessageWindow.DisplayChatWindow( rosterItem.Key, true ) ;
			}
		}

		private void RosterControl_Drop( object sender, DragEventArgs e )
		{
			e.Handled = true ;
		}

		private void RosterControl_DragOver( object sender, DragEventArgs e )
		{
			RosterItem rosterItem = e.Data.GetData( "xeus.RosterItem" ) as RosterItem ;

			if ( rosterItem != null )
			{
				e.Effects = DragDropEffects.Move ;
			}
			else
			{
				e.Effects = DragDropEffects.None ;
			}

			e.Handled = true ;
		}

		private void RosterControl_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
		{
			/*FrameworkElement item = _roster.InputHitTest( e.GetPosition( _roster ) ) as FrameworkElement ;

			if ( item != null && item.DataContext is RosterItem )
			{
				DataObject data = new DataObject( "xeus.RosterItem", item.DataContext ) ;


				try
				{
					DragDropEffects effects = DragDrop.DoDragDrop( _roster, data, DragDropEffects.Move ) ;
				}
				catch ( Exception e1 )
				{
					Console.WriteLine( e1 ) ;
				}
			}*/
		}
	}
}