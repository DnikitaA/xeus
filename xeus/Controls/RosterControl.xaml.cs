using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Collections.Specialized ;
using System.ComponentModel ;
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
		private InlineMethod _inlineMethod = new InlineMethod() ;

		private delegate void SelectItemCallback( RosterItem item ) ;

		public RosterControl()
		{
			Initialized += new EventHandler( RosterControl_Initialized );

			InitializeComponent() ;
		}

		void OnContextMenuOpen( object sender, RoutedEventArgs e )
		{
			ContextMenu contextMenu = sender as ContextMenu ;

			if ( contextMenu != null )
			{
				MenuItem menuItem = ( MenuItem )contextMenu.Items[ 0 ] ;

				menuItem.Items.Clear();
				
				foreach ( string group in _expanderStates.Keys )
				{
					if ( !RosterItem.IsSystemGroup( group ) )
					{
						MenuItem item = new MenuItem() ;
						item.Header = group ;

						menuItem.Items.Add( item ) ;

						item.Unloaded += new RoutedEventHandler( item_Unloaded ) ;
						item.Click += new RoutedEventHandler( item_Click ) ;
					}
				}

				if ( Client.Instance.IsAvailable )
				{
					if ( menuItem.Items.Count > 0 )
					{
						menuItem.Items.Add( new Separator() ) ;
					}

					MenuItem itemNewGroup = new MenuItem() ;
					itemNewGroup.Header = "Create New group" ;
					itemNewGroup.Tag = "newGroup" ;

					menuItem.Items.Add( itemNewGroup ) ;

					itemNewGroup.Unloaded += new RoutedEventHandler( item_Unloaded ) ;
					itemNewGroup.Click += new RoutedEventHandler( item_Click ) ;
				}
			}
		}

		void item_Click( object sender, RoutedEventArgs e )
		{
			MenuItem item = ( MenuItem ) sender ;
			RosterItem rosterItem = item.DataContext as RosterItem ;

			if ( rosterItem != null )
			{
				if ( item.Tag != null && item.Tag.ToString() == "newGroup" )
				{
					AskForSingleValue askForSingleValue = new AskForSingleValue( "Add new Group", "Group Name" );

					askForSingleValue.ShowDialog();

					if ( askForSingleValue.DialogResult.HasValue && askForSingleValue.DialogResult.Value )
					{
						Client.Instance.SetRosterGropup( rosterItem, askForSingleValue.Value );
					}
				}
				else
				{
					Client.Instance.SetRosterGropup( rosterItem, item.Header.ToString() );
				}
			}
		}

		void item_Unloaded( object sender, RoutedEventArgs e )
		{
			MenuItem item = ( MenuItem ) sender ;
			
			item.Unloaded -= new RoutedEventHandler( item_Unloaded );
			item.Click -= new RoutedEventHandler( item_Click );
		}


		void RosterControl_Initialized( object sender, EventArgs e )
		{
			_rosterItemBig = ( DataTemplate ) App.Current.FindResource( "RosterItemBig" ) ;
			_rosterItemSmall = ( DataTemplate ) App.Current.FindResource( "RosterItemSmall" ) ;

			SetItemTemplate();

			_sliderItemSize.ValueChanged += new RoutedPropertyChangedEventHandler< double >( _sliderItemSize_ValueChanged ) ;
			_inlineMethod.Finished += new InlineMethod.InlineResultHandler( _inlineMethod_Finished ) ;

			_expanderStates = new Database().ReadGroups() ;

			Client.Instance.Roster.Items.CollectionChanged += new NotifyCollectionChangedEventHandler( Items_CollectionChanged ) ;
			Client.Instance.Roster.ReadRosterFromDb() ;
		}

		private void _inlineSearch_Closed( bool isEnter )
		{
			_roster.Focus() ;

			lock ( _displayNamesLock )
			{
				_displayNames = null ;
			}
		}

		private void _inlineMethod_Finished( object result )
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

					_roster.ScrollIntoView( item ) ;
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
				_inlineSearch.TextChanged += new TextChangedEventHandler( _inlineSearch_TextChanged ) ;
				_inlineSearch.Closed += new InlineSearch.ClosedHandler( _inlineSearch_Closed ) ;
			}
		}

		public Dictionary< string, bool > ExpanderStates
		{
			get
			{
				return _expanderStates ;
			}
			set
			{
				_expanderStates = value ;
			}
		}

		object _displayNamesLock = new object();
		private List< KeyValuePair< string, RosterItem > > _displayNames = null ;

		private object SearchInList( ref bool stop, object param )
		{
			lock ( _displayNamesLock )
			{
				if ( _displayNames == null )
				{
					_displayNames = new List< KeyValuePair< string, RosterItem > >() ;

					lock ( Client.Instance.Roster.Items._syncObject )
					{
						foreach ( RosterItem rosterItem in Client.Instance.Roster.Items )
						{
							_displayNames.Add( new KeyValuePair< string, RosterItem >( rosterItem.DisplayName.ToUpper().Trim(), rosterItem ) ) ;
						}
					}
				}
			}

			RosterItem found = null ;
			
			string toFound = ( ( string ) param ).ToUpper() ;

			foreach ( KeyValuePair< string, RosterItem > displayName in _displayNames )
			{
				if ( stop )
				{
					return null ;
				}

				if ( ( ( string ) param ) == String.Empty )
				{
					return null ;
				}

				if ( displayName.Key.Contains( toFound ) )
				{
					found = displayName.Value ;
					break ;
				}
			}

			return found ;
		}

		private void _inlineSearch_TextChanged( object sender, TextChangedEventArgs e )
		{
			_inlineMethod.Go( new InlineParam( SearchInList, _inlineSearch.Text ) ) ;
		}

		private void OnKeyDown( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Return )
			{
				RosterItem rosterItem = _roster.SelectedItem as RosterItem ;
				OpenMessageWindow( rosterItem ) ;
			}
			else if ( InlineSearch != null )
			{
				InlineSearch.SendKey( e.Key ) ;
			}
		}

		private Dictionary< string, bool > _expanderStates = new Dictionary< string, bool >();

		private void OnLoadExpander( object sender, RoutedEventArgs e )
		{
			Expander expander = sender as Expander ;
			CollectionViewGroup viewGroup = expander.DataContext as CollectionViewGroup ;
			string expanderName = viewGroup.Name.ToString() ;

			expander.IsExpanded = IsExpanded( expanderName ) ;
		}

		void OnExpanded( object sender, RoutedEventArgs e )
		{
			Expander expander = sender as Expander ;
			CollectionViewGroup viewGroup = expander.DataContext as CollectionViewGroup ;
			string expanderName = viewGroup.Name.ToString() ;

			ExpanderStates[ expanderName ] = true ;
		}

		void OnCollapsed( object sender, RoutedEventArgs e )
		{
			Expander expander = sender as Expander ;
			CollectionViewGroup viewGroup = expander.DataContext as CollectionViewGroup ;
			string expanderName = viewGroup.Name.ToString() ;

			ExpanderStates[ expanderName ] = false ;
		}

		bool IsExpanded( string expanderName )
		{
			bool expanded ;
			
			if ( ExpanderStates.TryGetValue( expanderName, out expanded ) )
			{
				return expanded ;
			}
			else
			{
				ExpanderStates[ expanderName ] = true ;
			}
			
			return true ;
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
				case "DisplayName":
					{
						if ( _rosterView == null )
						{
							_rosterView = CollectionViewSource.GetDefaultView( _roster.ItemsSource ) ;
						}

						//_rosterView.Refresh() ;
						Client.Instance.Roster.AddRemoveItem( ( RosterItem ) sender ) ;
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

		void SetItemTemplate()
		{
			if ( _sliderItemSize.Value > 150.0 )
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

		private void _sliderItemSize_ValueChanged( object sender, RoutedPropertyChangedEventArgs< double > e )
		{
			SetItemTemplate();
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