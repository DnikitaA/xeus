using System;
using System.Collections.Generic;
using System.Drawing ;
using System.Text;
using System.Windows.Forms ;

namespace xeus.Controls
{
	class TrayIcon : IDisposable
	{
		private NotifyIcon _notifyIcon = new NotifyIcon() ;

		Queue< Icon > _pending = new Queue< Icon >( 4 );
		Queue< Icon > _normal = new Queue< Icon >( 1 );
		Queue< Icon > _message = new Queue< Icon >( 2 );

		private TrayState _state = TrayState.Normal ;

		System.Timers.Timer _reloadTime = new System.Timers.Timer( 500 );

		public enum TrayState
		{
			Normal,
			NewMessage,
			Pending
		}

		public TrayIcon()
		{
			_normal.Enqueue( Properties.Resources.xeus );

			_pending.Enqueue( Properties.Resources.xeus1 );
			_pending.Enqueue( Properties.Resources.xeus2 );
			_pending.Enqueue( Properties.Resources.xeus3 );
			_pending.Enqueue( Properties.Resources.xeus4 );

			_message.Enqueue( Properties.Resources.message );
			_message.Enqueue( Properties.Resources.message_trans );

			_notifyIcon.Visible = true ;
			_notifyIcon.Text = "xeus" ;
			
			_reloadTime.AutoReset = true ;
			_reloadTime.Elapsed += new System.Timers.ElapsedEventHandler( _reloadTime_Elapsed );
			_reloadTime.Start();
		}

		void _reloadTime_Elapsed( object sender, System.Timers.ElapsedEventArgs e )
		{
			switch ( _state )
			{
				case TrayState.Normal:
					{
						_notifyIcon.Icon = _normal.Dequeue() ;
						_normal.Enqueue( _notifyIcon.Icon ) ;
						break ;
					}

				case TrayState.NewMessage:
					{
						_notifyIcon.Icon = _message.Dequeue() ;
						_message.Enqueue( _notifyIcon.Icon ) ;
						break;
					}
				case TrayState.Pending:
					{
						_notifyIcon.Icon = _pending.Dequeue() ;
						_pending.Enqueue( _notifyIcon.Icon ) ;
						break;
					}
			}			
		}

		public NotifyIcon NotifyIcon
		{
			get
			{
				return _notifyIcon ;
			}
		}

		public TrayState State
		{
			get
			{
				return _state ;
			}
			set
			{
				if ( _state == value )
				{
					return ;
				}

				_state = value ;
			}
		}

		public void Dispose()
		{
			_reloadTime.Stop();

			if ( _notifyIcon != null )
			{
				_notifyIcon.Dispose() ;
			}
		}
	}
}
