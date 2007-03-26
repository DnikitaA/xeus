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

		public Popup()
		{
			InitializeComponent();

			DataContext = Client.Instance ;

			Client.Instance.Event.Items.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler( Items_CollectionChanged );
		}

		void Items_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			if ( Client.Instance.Event.Items.Count > 0 )
			{
				Show() ;
			}
			else
			{
				Hide();
			}

			Left = System.Windows.SystemParameters.PrimaryScreenWidth - _listBox.Width ;
			Top = System.Windows.SystemParameters.PrimaryScreenHeight - _listBox.Height ;
		}
	}
}