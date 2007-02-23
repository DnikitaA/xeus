using System ;
using System.Windows.Markup ;
using agsXMPP.protocol.client ;
using xeus.Controls ;

namespace xeus.Core
{
	internal class MessageCenter
	{
		private ObservableCollectionDisp< Message > _normalMessages =
			new ObservableCollectionDisp< Message >( App.DispatcherThread ) ;

		private ObservableCollectionDisp< Message > _errorMessages =
			new ObservableCollectionDisp< Message >( App.DispatcherThread ) ;

		private ObservableCollectionDisp< ChatMessage > _chatMessages =
			new ObservableCollectionDisp< ChatMessage >( App.DispatcherThread ) ;

		private ObservableCollectionDisp< Message > _hedlineMessages =
			new ObservableCollectionDisp< Message >( App.DispatcherThread ) ;

		public void Alert( string text )
		{
			App.Instance.Window.Alert( text ) ;
		}

		public void RegisterEvent( Client client )
		{
			client.Message += new Client.MessageHandler( client_Message ) ;
		}

		public ObservableCollectionDisp< Message > NormalMessages
		{
			get
			{
				return _normalMessages ;
			}
		}

		public ObservableCollectionDisp< Message > ErrorMessages
		{
			get
			{
				return _errorMessages ;
			}
		}

		public ObservableCollectionDisp< ChatMessage > ChatMessages
		{
			get
			{
				return _chatMessages ;
			}
		}

		public ObservableCollectionDisp< Message > HedlineMessages
		{
			get
			{
				return _hedlineMessages ;
			}
		}

		public void MoveUnreadMessagesToRosterItem( RosterItem rosterItem )
		{
			for ( int i = Client.Instance.MessageCenter.ChatMessages.Count - 1; i >= 0 ; i = Client.Instance.MessageCenter.ChatMessages.Count - 1 )
			{
				ChatMessage message = Client.Instance.MessageCenter.ChatMessages[ i ] ;

				rosterItem.Messages.Add( message ) ;

				Client.Instance.MessageCenter.ChatMessages.Remove( message ) ;
			}
		}

		private void client_Message( Message msg )
		{
			switch ( msg.Type )
			{
				case MessageType.normal:
					{
						_normalMessages.Add( msg );
						break ;
					}
				case MessageType.headline:
					{
						_hedlineMessages.Add( msg );
						break ;
					}
				case MessageType.chat:
					{
						RosterItem rosterItem = Client.Instance.Roster.FindItem( msg.From.Bare ) ;

						if ( MessageWindow.IsOpen() )
						{
							rosterItem.Messages.Add( new ChatMessage( msg, rosterItem, DateTime.Now ) );
							MessageWindow.DisplayChatWindow( rosterItem.Key, false );
						}
						else
						{
							_chatMessages.Add( new ChatMessage( msg, rosterItem, DateTime.Now ) ) ;
						}

						break ;
					}
				case MessageType.error:
					{
						_errorMessages.Add( msg );
						break ;
					}
				case MessageType.groupchat:
					{
						break ;
					}
			}
		}
	}
}