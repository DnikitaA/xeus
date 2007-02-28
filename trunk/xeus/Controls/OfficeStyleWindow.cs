using System ;
using System.Runtime.InteropServices ;
using System.Windows ;
using System.Windows.Input ;
using System.Windows.Interop ;

namespace xeus.Controls
{
	public partial class OfficeStyleWindow
	{
		#region sizing event handlers

		private void OnSizeSouth( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.South ) ;
			}
		}

		private void OnSizeNorth( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.North ) ;
			}
		}

		private void OnSizeEast( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.East ) ;
			}
		}

		private void OnSizeWest( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.West ) ;
			}
		}

		private void OnSizeNorthWest( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.NorthWest ) ;
			}
		}

		private void OnSizeNorthEast( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.NorthEast ) ;
			}
		}

		private void OnSizeSouthEast( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.SouthEast ) ;
			}
		}

		private void OnSizeSouthWest( object sender, MouseButtonEventArgs e )
		{
			Window wnd = ( ( FrameworkElement ) sender ).TemplatedParent as Window ;
			if ( wnd != null )
			{
				WindowInteropHelper helper = new WindowInteropHelper( wnd ) ;
				DragSize( helper.Handle, SizingAction.SouthWest ) ;
			}
		}

		#endregion

		#region P/Invoke and helper method

		private const int WM_SYSCOMMAND = 0x112 ;
		private const int SC_SIZE = 0xF000 ;


		[ DllImport( "user32.dll", CharSet = CharSet.Auto ) ]
		private static extern IntPtr SendMessage( IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam ) ;

		private void DragSize( IntPtr handle, SizingAction sizingAction )
		{
			if ( Mouse.LeftButton == MouseButtonState.Pressed )
			{
				SendMessage( handle, WM_SYSCOMMAND, ( IntPtr ) ( SC_SIZE + sizingAction ), IntPtr.Zero ) ;
				SendMessage( handle, 514, IntPtr.Zero, IntPtr.Zero ) ;
			}
		}

		#endregion

		#region helper enum

		public enum SizingAction
		{
			North = 3,
			South = 6,
			East = 2,
			West = 1,
			NorthEast = 5,
			NorthWest = 4,
			SouthEast = 8,
			SouthWest = 7
		}

		#endregion
	}
}