using System.Windows ;
using System.Windows.Input ;
using System.Windows.Threading ;

namespace xeus.Core
{
	public static class RosterItemCommands
	{
		private static Dispatcher _dispatcher ;
		private static RoutedUICommand _addCommand = new RoutedUICommand( "Add", "add", typeof ( RosterItemCommands ) ) ;

		private static RoutedUICommand _removeCommand =
			new RoutedUICommand( "Remove", "remove", typeof ( RosterItemCommands ) ) ;


		public static RoutedUICommand Add
		{
			get
			{
				return _addCommand ;
			}
		}


		public static RoutedUICommand Remove
		{
			get
			{
				return _removeCommand;
			}
		}


		static RosterItemCommands()
		{
			_dispatcher = Dispatcher.CurrentDispatcher ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _addCommand, ExecuteAddCommand, CanExecuteAddCommand ) ) ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _removeCommand, ExecuteRemoveCommand, CanExecuteRemoveCommand ) ) ;
		}

		public static void CanExecuteAddCommand( object sender, CanExecuteRoutedEventArgs e )
		{
			// Handler to provide mechanism for determining when the add command can be executed.
			e.Handled = true ;
			e.CanExecute = true ;
		}

		public static void CanExecuteRemoveCommand( object sender, CanExecuteRoutedEventArgs e )
		{
			// Handler to provide mechanism for determining when the remove command can be executed.
			e.Handled = true ;
		}

		public static void ExecuteAddCommand( object sender, ExecutedRoutedEventArgs e )
		{
			// Handler to actually execute the code to perform the add command
			//add your code here to handle
		}


		public static void ExecuteRemoveCommand( object sender, ExecutedRoutedEventArgs e )
		{
			// Handler to actually execute the code to perform the remove command
			//add your code here to handle
		}
	}
}