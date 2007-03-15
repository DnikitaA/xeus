using System ;
using System.Windows.Markup ;
using agsXMPP.protocol.client ;
using agsXMPP.protocol.extensions.chatstates ;
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

		private ObservableCollectionDisp< HeadlineMessage > _hedlineMessages =
			new ObservableCollectionDisp< HeadlineMessage >( App.DispatcherThread ) ;

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

		public ObservableCollectionDisp< HeadlineMessage > HedlineMessages
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

				lock ( rosterItem.Messages._syncObject )
				{
					rosterItem.Messages.Add( message ) ;
				}

				Client.Instance.MessageCenter.ChatMessages.Remove( message ) ;
			}
		}

		private void client_Message( Message msg )
		{
			switch ( msg.Type )
			{
				case MessageType.normal:
					{
						if ( msg.Chatstate != Chatstate.None )
						{
							RosterItem rosterItem = Client.Instance.Roster.FindItem( msg.From.Bare ) ;

							if ( rosterItem != null )
							{
								MessageWindow.ContactIsTyping( rosterItem.DisplayName, msg.Chatstate ) ;
							}
						}
						else
						{
							lock ( _hedlineMessages._syncObject )
							{
								_hedlineMessages.Add( new HeadlineMessage( msg ) ) ;
							}
						}

						break ;
					}
				case MessageType.headline:
					{
						lock ( _hedlineMessages._syncObject )
						{
							_hedlineMessages.Add( new HeadlineMessage( msg ) ) ;
						}

						break ;
					}
				case MessageType.chat:
					{
						RosterItem rosterItem = Client.Instance.Roster.FindItem( msg.From.Bare ) ;

						ChatMessage message = new ChatMessage( msg, rosterItem, DateTime.Now ) ;

						if ( string.IsNullOrEmpty( msg.Body ) )
						{
							if ( msg.Chatstate != Chatstate.None )
							{
								MessageWindow.ContactIsTyping( rosterItem.DisplayName, msg.Chatstate ) ;
							}
						}
						else
						{
							Database database = new Database();

							message.Id = database.InsertMessage( message ) ;

							if ( MessageWindow.IsOpen() )
							{
								lock ( rosterItem.Messages._syncObject )
								{
									rosterItem.Messages.Add( message ) ;
								}

								MessageWindow.DisplayChatWindow( rosterItem.Key, false ) ;
							}
							else
							{
								lock ( _chatMessages._syncObject )
								{
									_chatMessages.Add( message ) ;
								}
							}
						}

						break ;
					}
				case MessageType.error:
					{
						lock ( _errorMessages._syncObject )
						{
							_errorMessages.Add( msg ) ;
						}

						App.Instance.Window.AlertError( "Error", msg.Body );

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