using System.Windows.Threading ;
using agsXMPP ;
using agsXMPP.protocol.client ;

namespace xeus.Controls
{
	public partial class TransferWindow : WindowBase
	{
		private static TransferWindow _window = null ;

		protected delegate void TransferHandler( XmppClientConnection XmppCon, IQ iq ) ;

		internal static void Transfer( XmppClientConnection XmppCon, IQ iq )
		{
			if ( App.DispatcherThread.CheckAccess() )
			{
				if ( _window == null )
				{
					_window = new TransferWindow() ;
				}

				FileTransfer fileTransfer = new FileTransfer();
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