using System;
using System.Collections.Generic;
using System.Text;
using agsXMPP.protocol.client ;

namespace xeus.Core
{
	class HeadlineMessage
	{
		private string _from ;
		private string _to ;
		private string _body ;
		private DateTime _time ;

		public HeadlineMessage( Message message )
		{
			_from = message.From.Bare ;
			_to = message.To.Bare ;
			_body = message.Body ;
			_time = DateTime.Now ;
		}

		public string From
		{
			get
			{
				return _from ;
			}
		}

		public string To
		{
			get
			{
				return _to ;
			}
		}

		public string Body
		{
			get
			{
				return _body ;
			}
		}

		public DateTime Time
		{
			get
			{
				return _time ;
			}
		}

		public override string ToString()
		{
			return Body ;
		}
	}
}
