using System.Windows ;
using System.Windows.Input ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for WindowBase.xaml
	/// </summary>
	public partial class WindowBase : Window
	{

		private ResizeMode _originalResizeMode = ResizeMode.NoResize ;

		public ResizeMode OriginalResizeMode
		{
			get
			{
				return _originalResizeMode ;
			}
		}

		protected override void OnInitialized( System.EventArgs e )
		{
			_originalResizeMode = ResizeMode ;

			//ResizeMode = ResizeMode.NoResize ;

			base.OnInitialized( e );
		}
	}
}