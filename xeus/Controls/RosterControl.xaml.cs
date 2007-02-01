using System ;
using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Data ;
using System.Windows.Input ;
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

		public RosterControl()
		{
			InitializeComponent() ;

			_rosterItemBig = ( DataTemplate ) App.Current.FindResource( "RosterItemBig" ) ;
			_rosterItemSmall = ( DataTemplate ) App.Current.FindResource( "RosterItemSmall" ) ;
			_roster.ItemTemplate = _rosterItemSmall ;

			_sliderItemSize.ValueChanged += new RoutedPropertyChangedEventHandler<double>( _sliderItemSize_ValueChanged );
		}

		void _sliderItemSize_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
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

			if ( rosterItem != null )
			{
				OpenMessageWindow( rosterItem ) ;
			}
		}

		private void OpenMessageWindow( RosterItem rosterItem )
		{
			MessageWindow.Instance.DisplayChat( rosterItem.Key );
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