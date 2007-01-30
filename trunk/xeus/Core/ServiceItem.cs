using System;
using agsXMPP ;
using agsXMPP.protocol.iq.disco ;

namespace xeus.Core
{
	internal class ServiceItem
	{
		private string _name = String.Empty ;
		private Jid _jid = null ;
		private DiscoInfo _disco = null ;

		public ServiceItem( string name, Jid jid, DiscoInfo disco )
		{
			_name = name ;
			_jid = jid ;
			_disco = disco ;
		}

		public string Name
		{
			get
			{
				return _name ;
			}
			set
			{
				_name = value ;
			}
		}
		
		public Jid Jid
		{
			get
			{
				return _jid ;
			}
		}

		public DiscoInfo Disco
		{
			get
			{
				return _disco ;
			}
		}
	}
}
