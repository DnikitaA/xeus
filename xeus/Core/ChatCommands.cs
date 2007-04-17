using System;
using System.Collections.Generic;
using System.Text;
using System.Windows ;
using System.Windows.Input ;
using xeus.Controls ;

namespace xeus.Core
{
	class ChatCommands
	{
		private static RoutedUICommand _chatSearch = new RoutedUICommand( "Search Message", "chatrSearch", typeof ( ChatCommands ) ) ;
		private static RoutedUICommand _chatSearchNext = new RoutedUICommand( "Search Next Message", "chatSearchNext", typeof ( ChatCommands ) ) ;

		public static RoutedUICommand ChatSearch
		{
			get
			{
				return _chatSearch ;
			}
		}

		public static RoutedUICommand ChatSearchNext
		{
			get
			{
				return _chatSearchNext ;
			}
		}

		public static void BindChatCommands( Window window )
		{
			window.CommandBindings.Add(
				new CommandBinding( _chatSearch, ExecuteChatSearch, CanExecuteChatSearch ) ) ;

			window.CommandBindings.Add(
				new CommandBinding( _chatSearchNext, ExecuteChatSearchNext, CanExecuteChatSearchNext ) ) ;

			window.InputBindings.Add( new KeyBinding( _chatSearch, Key.F, ModifierKeys.Control ) ) ;
			window.InputBindings.Add( new KeyBinding( _chatSearchNext, Key.F3, ModifierKeys.None ) ) ;
		}

		public static void CanExecuteChatSearch( object sender, CanExecuteRoutedEventArgs e )
		{
			e.Handled = true ;
			e.CanExecute = true ;
		}

		public static void CanExecuteChatSearchNext( object sender, CanExecuteRoutedEventArgs e )
		{
			e.Handled = true ;
			e.CanExecute = true ;
		}
		
		public static void ExecuteChatSearch( object sender, ExecutedRoutedEventArgs e )
		{
			MessageWindow.Instance.DisplaySearch() ;

			e.Handled = true ;
		}

		public static void ExecuteChatSearchNext( object sender, ExecutedRoutedEventArgs e )
		{
			MessageWindow.Instance.DisplaySearchNext() ;

			e.Handled = true ;
		}	}
}
