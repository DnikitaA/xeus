using System.Windows ;
using System.Windows.Input ;
using System.Windows.Threading ;

namespace xeus.Core
{
	public static class RosterItemCommands
	{
		private static Dispatcher _dispatcher ;
		private static RoutedUICommand _authSendTo = new RoutedUICommand( "Resend Authorization To Contact", "authSendTo", typeof ( RosterItemCommands ) ) ;
		private static RoutedUICommand _authRequestFrom = new RoutedUICommand( "Request Authorization From Contact", "authRequestFrom", typeof ( RosterItemCommands ) ) ;
		private static RoutedUICommand _authRemoveFrom = new RoutedUICommand( "Remove Your Authorization From Contact", "authRemoveFrom", typeof ( RosterItemCommands ) ) ;

		private static RoutedUICommand _contactAdd = new RoutedUICommand( "Add New Contact", "contactAdd", typeof ( RosterItemCommands ) ) ;
		private static RoutedUICommand _contactDelete = new RoutedUICommand( "Delete Contact", "contactDelete", typeof ( RosterItemCommands ) ) ;

		public static RoutedUICommand AuthSendTo
		{
			get
			{
				return _authSendTo ;
			}
		}

		public static RoutedUICommand AuthRequestFrom
		{
			get
			{
				return _authRequestFrom ;
			}
		}

		public static RoutedUICommand AuthRemoveFrom
		{
			get
			{
				return _authRemoveFrom ;
			}
		}

		public static RoutedUICommand ContactAdd
		{
			get
			{
				return _contactAdd ;
			}
		}

		public static RoutedUICommand ContactDelete
		{
			get
			{
				return _contactDelete ;
			}
		}

		static RosterItemCommands()
		{
			_dispatcher = Dispatcher.CurrentDispatcher ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _authSendTo, ExecuteAuthSendTo, CanExecuteAuthSendTo ) ) ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _authRequestFrom, ExecuteAuthRequestFrom, CanExecuteAuthRequestFrom ) ) ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _authRemoveFrom, ExecuteAuthRemoveFrom, CanExecuteAuthRemoveFrom ) ) ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _contactAdd, ExecuteContactAdd, CanExecuteContactAdd ) ) ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _contactDelete, ExecuteContactDelete, CanExecuteContactDelete ) ) ;
		}

		public static void CanExecuteAuthRemoveFrom( object sender, CanExecuteRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			e.Handled = true ;
			e.CanExecute = ( rosterItem != null && Client.Instance.IsAvailable ) ;
		}

		public static void CanExecuteAuthRequestFrom( object sender, CanExecuteRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			e.Handled = true ;
			e.CanExecute = ( rosterItem != null && Client.Instance.IsAvailable ) ;
		}

		public static void CanExecuteAuthSendTo( object sender, CanExecuteRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			e.Handled = true ;
			e.CanExecute = ( rosterItem != null && Client.Instance.IsAvailable ) ;
		}

		public static void CanExecuteContactAdd( object sender, CanExecuteRoutedEventArgs e )
		{
			e.Handled = true ;
			e.CanExecute = Client.Instance.IsAvailable ;
		}

		public static void CanExecuteContactDelete( object sender, CanExecuteRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			e.Handled = true ;
			e.CanExecute = ( rosterItem != null && Client.Instance.IsAvailable ) ;
		}

		public static void ExecuteAuthSendTo( object sender, ExecutedRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			if ( rosterItem != null )
			{
				Client.Instance.PresenceManager.ApproveSubscriptionRequest( rosterItem.XmppRosterItem.Jid ) ;
			}

			e.Handled = true ;
		}

		public static void ExecuteAuthRequestFrom( object sender, ExecutedRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			if ( rosterItem != null )
			{
				Client.Instance.PresenceManager.Subcribe( rosterItem.XmppRosterItem.Jid ) ;
			}

			e.Handled = true ;
		}
	
		public static void ExecuteAuthRemoveFrom( object sender, ExecutedRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			if ( rosterItem != null )
			{
				Client.Instance.PresenceManager.RefuseSubscriptionRequest( rosterItem.XmppRosterItem.Jid ) ;
			}

			e.Handled = true ;
		}

		public static void ExecuteContactAdd( object sender, ExecutedRoutedEventArgs e )
		{
			App.Instance.Window.AddUser();

			e.Handled = true ;
		}
	
		public static void ExecuteContactDelete( object sender, ExecutedRoutedEventArgs e )
		{
			RosterItem rosterItem = e.Parameter as RosterItem ;

			if ( rosterItem != null )
			{
				Client.Instance.Roster.DeleteRosterItem( rosterItem ) ;
			}

			e.Handled = true ;
		}
	}
}