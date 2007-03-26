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
				e.Handled = true ;
				Close( false ) ;
			}
			else if ( e.Key == Key.Return )
			{
				e.Handled = true ;
				Close( true ) ;
			}
			else if ( e.Key == Key.PageDown )
			{
				e.Handled = true ;
				OnNext( this, null ) ;
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

		void OnNext( Object sender, RoutedEventArgs e )
		{
			if ( TextChanged != null )
			{
				TextChanged( sender, null ) ;
			}
		}

		public void SendKey( Key key )
		{
			if ( Keyboard.Modifiers == 0 )
			{
				if ( ( key >= Key.D0 && key <= Key.Z )
					|| ( key >= Key.NumPad0 && key <= Key.NumPad9 ) )
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
				else if ( key == Key.PageDown )
				{
					OnNext( this, null ) ;
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