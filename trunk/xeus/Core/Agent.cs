using agsXMPP ;

namespace xeus.Core
{
	internal class Agents
	{
		ObservableCollectionDisp< AgentItem > _items = new ObservableCollectionDisp< AgentItem >( App.DispatcherThread );

		public void RegisterEvents( XmppClientConnection xmppConnecion )
		{
			xmppConnecion.OnAgentStart += new ObjectHandler( xmppConnecion_OnAgentStart );
			xmppConnecion.OnAgentEnd += new ObjectHandler( xmppConnecion_OnAgentEnd );
			xmppConnecion.OnAgentItem += new XmppClientConnection.AgentHandler( xmppConnecion_OnAgentItem );
		}

		void xmppConnecion_OnAgentItem( object sender, agsXMPP.protocol.iq.agent.Agent agent )
		{
			_items.Add( new AgentItem( agent ) );
		}

		void xmppConnecion_OnAgentEnd( object sender )
		{
		}

		void xmppConnecion_OnAgentStart( object sender )
		{
		}

		public ObservableCollectionDisp< AgentItem > Items
		{
			get
			{
				return _items ;
			}
		}
	}
}
