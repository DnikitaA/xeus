using System;
using System.Collections.Generic;
using System.Text;
using System.Threading ;

namespace xeus.Core
{

	public delegate object InlineHandler( ref bool stop, object param ) ;

	public class InlineParam
	{
		private InlineHandler _inlineHandler ;
		private object _param ;

		public InlineParam( InlineHandler inlineHandler, object param )
		{
			_inlineHandler = inlineHandler ;
			_param = param ;
		}

		public InlineHandler InlineHandler
		{
			get
			{
				return _inlineHandler ;
			}
		}

		public object Param
		{
			get
			{
				return _param ;
			}
		}
	}

	class InlineMethod : IDisposable
	{
		private Thread _thread = null ;
		private bool _finish ;

		public delegate void InlineResultHandler( object result ) ;

		public event InlineResultHandler Finished ;

		public InlineMethod()
		{
		}

		void Method( object param )
		{
			object result = ( ( InlineParam ) param ).InlineHandler( ref _finish, ( ( InlineParam ) param ).Param ) ;

			if ( Finished != null )
			{
				Finished( result ) ;
			}
		}

		public void Go( InlineParam param )
		{
			if ( _thread != null )
			{
				_finish = true ;
				_thread.Join( 5000 ) ;
				_finish = false ;
			}

			_thread = new Thread( new ParameterizedThreadStart( Method ) );
			_thread.Priority = ThreadPriority.Lowest ;
			_thread.Start( param );
		}

		public void Dispose()
		{
			if ( _thread != null )
			{
				_finish = true ;
				_thread.Join( 5000 ) ;
			}
		}
	}
}
