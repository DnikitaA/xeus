using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for InlineSearch.xaml
	/// </summary>
	public partial class InlineSearch : System.Windows.Controls.UserControl
	{
		private bool _notFound = false ;
		private Brush _originalBackground ;

		public delegate void ClosedHandler( bool isEnter ) ;
		public event TextChangedEventHandler TextChanged ;
		public event ClosedHandler Closed ;

		public InlineSearch()
		{
			InitializeComponent();

			_text.PreviewKeyDown += new KeyEventHandler( _text_KeyDown );
			_originalBackground = _border.Background ;
		}

		void _text_KeyDown( object sender, KeyEventArgs e )
		{
			if ( e.Key == Key.Escape )
			{
				Close( false ) ;
			}
			else if ( e.Key == Key.Return )
			{
				Close( true ) ;
			}
		}

		void OnTextChanged( Object sender, TextChangedEventArgs e )
		{
			if ( TextChanged != null )
			{
				TextChanged( sender, e ) ;
			}
		}

		void Close( bool isEnter )
		{
			Visibility = Visibility.Collapsed ;
			_text.Text = String.Empty ;

			if ( Closed != null )
			{
				Closed( isEnter ) ;
			}
		}

		void OnCancel( Object sender, RoutedEventArgs e )
		{
			Close( false ) ;
		}

		public void SendKey( Key key )
		{
			if ( Keyboard.Modifiers == 0 )
			{
				if ( key >= Key.D0 && key <= Key.Z )
				{
					Visibility = Visibility.Visible ;
					_text.Focus() ;
				}
				else if ( key == Key.Escape )
				{
					Close( false ) ;
				}
				else if ( key == Key.Return )
				{
					Close( true ) ;
				}
			}
		}

		public string Text
		{
			get
			{
				return _text.Text ;
			}
		}

		public bool NotFound
		{
			get
			{
				return _notFound ;
			}
			set
			{
				_notFound = value ;
				_border.Background = ( value ) ? Brushes.DarkRed : _originalBackground ;
			}
		}
	}
}