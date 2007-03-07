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

		static RosterItemCommands()
		{
			_dispatcher = Dispatcher.CurrentDispatcher ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _authSendTo, ExecuteAuthSendTo, CanExecuteAuthSendTo ) ) ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _authRequestFrom, ExecuteAuthRequestFrom, CanExecuteAuthRequestFrom ) ) ;

			Application.Current.MainWindow.CommandBindings.Add(
				new CommandBinding( _authRemoveFrom, ExecuteAuthRemoveFrom, CanExecuteAuthRemoveFrom ) ) ;
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
	}
}