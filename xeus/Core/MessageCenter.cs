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
			new ObservableCollectionDisp< Message >( App.Current.Dispatcher ) ;

		private ObservableCollectionDisp< Message > _errorMessages =
			new ObservableCollectionDisp< Message >( App.Current.Dispatcher ) ;

		private ObservableCollectionDisp< ChatMessage > _chatMessages =
			new ObservableCollectionDisp< ChatMessage >( App.Current.Dispatcher ) ;

		private ObservableCollectionDisp< HeadlineMessage > _hedlineMessages =
			new ObservableCollectionDisp< HeadlineMessage >( App.Current.Dispatcher ) ;

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

		public void RemoveMoveUnreadMessages()
		{
			lock ( Client.Instance.MessageCenter.ChatMessages._syncObject )
			{
				Client.Instance.MessageCenter.ChatMessages.Clear() ;
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
						else if ( !String.IsNullOrEmpty( msg.Body ) )
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
						if ( !String.IsNullOrEmpty( msg.Body ) )
						{
							lock ( _hedlineMessages._syncObject )
							{
								_hedlineMessages.Add( new HeadlineMessage( msg ) ) ;
							}
						}

						break ;
					}
				case MessageType.chat:
					{
						RosterItem rosterItem = Client.Instance.Roster.FindItem( msg.From.Bare ) ;

						if ( rosterItem == null )
						{
							// not in roster
							lock ( Client.Instance.Roster.Items._syncObject )
							{
								agsXMPP.protocol.iq.roster.RosterItem xmppRosterItem = new agsXMPP.protocol.iq.roster.RosterItem() ;
								xmppRosterItem.Jid = msg.From ;
								rosterItem = new RosterItem( xmppRosterItem ) ;

								Client.Instance.Roster.Items.Add( rosterItem );
								Client.Instance.Roster.AskForVCard( msg.From.Bare ) ;
							}
						}

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

							if ( !MessageWindow.IsOpen()
									|| !MessageWindow.IsChatActive() )
							{
								Client.Instance.Event.AddEvent( new EventMessage( rosterItem, message ) ) ;
							}

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