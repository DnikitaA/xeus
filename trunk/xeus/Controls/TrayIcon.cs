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

		private Icon _mainIcon = Properties.Resources.xeus ;
		private Icon _messageIcon = Properties.Resources.message ;
		private Icon _messageIconTrans = Properties.Resources.message_trans ;

		private TrayState _state = TrayState.Normal ;

		System.Timers.Timer _reloadTime = new System.Timers.Timer( 500 );

		public enum TrayState
		{
			Normal,
			NewMessage
		}

		public TrayIcon()
		{
			_notifyIcon.Icon = _mainIcon ;
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
						if ( _notifyIcon.Icon != _mainIcon )
						{
							_notifyIcon.Icon = _mainIcon ;
						}
						break ;
					}

				case TrayState.NewMessage:
					{
						if ( _notifyIcon.Icon == _messageIcon )
						{
							_notifyIcon.Icon = _messageIconTrans ;
						}
						else
						{
							_notifyIcon.Icon = _messageIcon ;
						}
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
/*
				switch ( _state )
				{
					case TrayState.Normal:
						{
							_reloadTime.Stop();
							_notifyIcon.Icon = _mainIcon ;
							break ;
						}
					case TrayState.NewMessage:
						{
							_reloadTime.Start();
							break ;
						}
				}*/
			}
		}

		public void AlertError( string text )
		{
			_notifyIcon.ShowBalloonTip( 500, "Error", text, ToolTipIcon.Error ) ;			
		}

		public void Dispose()
		{
			_reloadTime.Stop();

			if ( _notifyIcon != null )
			{
				_notifyIcon.Dispose() ;
				_mainIcon.Dispose();
				_messageIcon.Dispose();
				_messageIconTrans.Dispose();
			}
		}
	}
}
