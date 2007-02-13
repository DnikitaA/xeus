using System.Windows ;
using System.Windows.Input ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for WindowBase.xaml
	/// </summary>
	public partial class WindowBase : Window
	{
		protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
		{
			base.OnMouseLeftButtonDown( e ) ;

			DragMove() ;
		}
	}
}