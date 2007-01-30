using agsXMPP ;

namespace xeus.Core
{
	internal class Agent
	{
		public void RegisterEvents( XmppClientConnection xmppConnecion )
		{
			xmppConnecion.OnAgentStart += new ObjectHandler( xmppConnecion_OnAgentStart );
			xmppConnecion.OnAgentEnd += new ObjectHandler( xmppConnecion_OnAgentEnd );
			xmppConnecion.OnAgentItem += new XmppClientConnection.AgentHandler( xmppConnecion_OnAgentItem );
		}

		void xmppConnecion_OnAgentItem( object sender, agsXMPP.protocol.iq.agent.Agent agent )
		{
		}

		void xmppConnecion_OnAgentEnd( object sender )
		{
		}

		void xmppConnecion_OnAgentStart( object sender )
		{
		}
	}
}
