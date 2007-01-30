using agsXMPP.protocol.client ;

namespace xeus.Core
{
	internal class MessageCenter
	{
		private ObservableCollectionDisp< Message > _normalMessages =
			new ObservableCollectionDisp< Message >( App.DispatcherThred ) ;

		private ObservableCollectionDisp< Message > _errorMessages =
			new ObservableCollectionDisp< Message >( App.DispatcherThred ) ;

		private ObservableCollectionDisp< Message > _chatMessages =
			new ObservableCollectionDisp< Message >( App.DispatcherThred ) ;

		private ObservableCollectionDisp< Message > _hedlineMessages =
			new ObservableCollectionDisp< Message >( App.DispatcherThred ) ;

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

		public ObservableCollectionDisp< Message > ChatMessages
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
						_chatMessages.Add( msg );
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