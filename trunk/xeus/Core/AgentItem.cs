using System;
using System.Collections.Generic;
using System.Text;

namespace xeus.Core
{
	class AgentItem
	{
		private agsXMPP.protocol.iq.agent.Agent _agent ;

		public AgentItem( agsXMPP.protocol.iq.agent.Agent agent )
		{
			_agent = agent ;
		}

		public bool CanRegister
		{
			get
			{
				return _agent.CanRegister ;
			}
		}

		public string Description
		{
			get
			{
				return _agent.Description ;
			}
		}

		public string Name
		{
			get
			{
				return _agent.Name ;
			}
		}

		public bool IsTransport
		{
			get
			{
				return _agent.IsTransport ;
			}
		}
	}
}
