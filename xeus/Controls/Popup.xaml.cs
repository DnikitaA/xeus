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
using System.Windows.Shapes;
using xeus.Core ;

namespace xeus.Controls
{
	/// <summary>
	/// Interaction logic for Popup.xaml
	/// </summary>

	public partial class Popup : System.Windows.Window
	{
		private bool _isClosed = false ;

		public Popup()
		{
			InitializeComponent();

			DataContext = Client.Instance ;

			Client.Instance.Event.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( Items_CollectionChanged );
			
			SizeChanged += new SizeChangedEventHandler( Popup_SizeChanged );
		}

		void Popup_SizeChanged( object sender, SizeChangedEventArgs e )
		{
			Left = SystemParameters.WorkArea.Right - ActualWidth - 10 ;
			Top = SystemParameters.WorkArea.Bottom - ActualHeight - 10 ;
		}

		protected override void OnClosed( EventArgs e )
		{
			_isClosed = true ;

			base.OnClosed( e );
		}

		void Items_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			if ( _isClosed )
			{
				return ;
			}

			if ( Client.Instance.Event.Items.Count > 0 )
			{
				Show() ;
			}
			else
			{
				Hide();
			}
		}
	}
}