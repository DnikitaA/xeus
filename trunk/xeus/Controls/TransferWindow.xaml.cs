using System.Windows.Threading ;
using agsXMPP ;
using agsXMPP.protocol.client ;

namespace xeus.Controls
{
	public partial class TransferWindow : WindowBase
	{
		private static TransferWindow _window = null ;

		protected delegate void TransferHandler( XmppClientConnection XmppCon, IQ iq ) ;
		private delegate void TransferFinishHandler( object sender, bool cancelled ) ;

		internal static void Transfer( XmppClientConnection XmppCon, Jid to, string fileName )
		{
			if ( _window == null )
			{
				_window = new TransferWindow() ;
			}

			FileTransfer fileTransfer = new FileTransfer();
			fileTransfer.TransferFinish += new FileTransfer.TransferFinishHandler( fileTransfer_TransferFinish );

			_window._list.Items.Add( fileTransfer ) ;
			fileTransfer.Transfer( XmppCon, to, fileName ) ;

			_window.Show() ;
			_window.Activate() ;
			_window.Closed += new System.EventHandler( _window_Closed );
		}

		static void _window_Closed( object sender, System.EventArgs e )
		{
			_window = null ;
		}

		internal static void Transfer( XmppClientConnection XmppCon, IQ iq )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( _window == null )
				{
					_window = new TransferWindow() ;
				}

				FileTransfer fileTransfer = new FileTransfer();
				fileTransfer.TransferFinish += new FileTransfer.TransferFinishHandler( fileTransfer_TransferFinish );

				_window._list.Items.Add( fileTransfer ) ;
				fileTransfer.Transfer( XmppCon, iq ) ;

				_window.Show() ;
				_window.Activate() ;
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new TransferHandler( Transfer ), XmppCon, iq ) ;
			}
		}

		static void fileTransfer_TransferFinish( object sender, bool cancelled )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( cancelled )
				{
					FileTransfer fileTransfer = sender as FileTransfer ;

					_window._list.Items.Remove( fileTransfer ) ;

					if ( _window._list.Items.Count == 0 )
					{
						CloseWindow() ;
					}
				}
			}
			else
			{
				App.DispatcherThread.BeginInvoke( DispatcherPriority.Normal,
				                                  new TransferFinishHandler( fileTransfer_TransferFinish ), sender, cancelled ) ;
			}
		}

		public static void CloseWindow()
		{
			if ( _window != null )
			{
				_window.Close() ;
				_window = null ;
			}
		}

		public TransferWindow()
		{
			InitializeComponent() ;
		}
	}
}