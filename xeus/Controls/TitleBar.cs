using System.Windows ;
using System.Windows.Controls ;
using System.Windows.Input ;
using System.Windows.Media ;

namespace xeus.Controls
{
	/// <summary>
	/// ========================================
	/// .NET Framework 3.0 Custom Control
	/// ========================================
	///
	/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
	///
	/// Step 1a) Using this custom control in a XAML file that exists in the current project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:MyNamespace="clr-namespace:TestAlex"
	///
	///
	/// Step 1b) Using this custom control in a XAML file that exists in a different project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:MyNamespace="clr-namespace:TestAlex;assembly=TestAlex"
	///
	/// You will also need to add a project reference from the project where the XAML file lives
	/// to this project and Rebuild to avoid compilation errors:
	///
	///     Right click on the target project in the Solution Explorer and
	///     "Add Reference"->"Projects"->[Browse to and select this project]
	///
	///
	/// Step 2)
	/// Go ahead and use your control in the XAML file. Note that Intellisense in the
	/// XML editor does not currently work on custom controls and its child elements.
	///
	///     <MyNamespace:TitleBar/>
	///
	/// </summary>
	public class TitleBar : Control
	{
		private Button closeButton ;
		private Button maxButton ;
		private Button minButton ;

		public TitleBar()
		{
			MouseLeftButtonDown += new MouseButtonEventHandler( OnTitleBarLeftButtonDown ) ;
			MouseDoubleClick += new MouseButtonEventHandler( TitleBar_MouseDoubleClick ) ;
			Loaded += new RoutedEventHandler( TitleBar_Loaded ) ;
		}

		private void TitleBar_MouseDoubleClick( object sender, MouseButtonEventArgs e )
		{
			MaxButton_Click( sender, e ) ;
		}

		private void TitleBar_Loaded( object sender, RoutedEventArgs e )
		{
			closeButton = ( Button ) Template.FindName( "CloseButton", this ) ;
			minButton = ( Button ) Template.FindName( "MinButton", this ) ;
			maxButton = ( Button ) Template.FindName( "MaxButton", this ) ;

			closeButton.Click += new RoutedEventHandler( CloseButton_Click ) ;
			minButton.Click += new RoutedEventHandler( MinButton_Click ) ;
			maxButton.Click += new RoutedEventHandler( MaxButton_Click ) ;
		}


		static TitleBar()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata( typeof ( TitleBar ), new FrameworkPropertyMetadata( typeof ( TitleBar ) ) ) ;
		}

		#region event handlers

		private void OnTitleBarLeftButtonDown( object sender, MouseEventArgs e )
		{
			Window window = TemplatedParent as Window ;
			if ( window != null )
			{
				window.DragMove() ;
			}
		}

		private void CloseButton_Click( object sender, RoutedEventArgs e )
		{
			Window window = TemplatedParent as Window ;
			if ( window != null )
			{
				window.Close() ;
			}
		}

		private void MinButton_Click( object sender, RoutedEventArgs e )
		{
			Window window = TemplatedParent as Window ;
			if ( window != null )
			{
				window.WindowState = WindowState.Minimized ;
			}
		}

		private void MaxButton_Click( object sender, RoutedEventArgs e )
		{
			Window window = TemplatedParent as Window ;
			if ( window != null )
			{
				if ( window.WindowState == WindowState.Maximized )
				{
					/*
					maxButton.ImageDown = "/images/maxpressed_n.png" ;
					maxButton.ImageNormal = "/images/max_n.png" ;
					maxButton.ImageOver = "/images/maxhot_n.png" ;
					 */

					window.WindowState = WindowState.Normal ;
				}
				else
				{
					/*
					maxButton.ImageDown = "/images/normalpress.png" ;
					maxButton.ImageNormal = "/images/normal.png" ;
					maxButton.ImageOver = "/images/normalhot.png" ;
					 */

					window.WindowState = WindowState.Maximized ;
				}
			}
		}

		#endregion

		#region properties

		public string Title
		{
			get
			{
				return ( string ) GetValue( TitleProperty ) ;
			}
			set
			{
				SetValue( TitleProperty, value ) ;
			}
		}

		public ImageSource Icon
		{
			get
			{
				return ( ImageSource ) GetValue( IconProperty ) ;
			}
			set
			{
				SetValue( IconProperty, value ) ;
			}
		}

		#endregion

		#region dependency properties

		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register(
				"Title", typeof ( string ), typeof ( TitleBar ) ) ;

		public static readonly DependencyProperty IconProperty =
			DependencyProperty.Register(
				"Icon", typeof ( ImageSource ), typeof ( TitleBar ) ) ;

		#endregion
	}
}