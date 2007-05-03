using System ;
using System.ComponentModel ;
using System.Windows.Threading ;

namespace xeus.Core
{
	internal class NotifyInfoDispatcher : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged ;
		public delegate void NotifyPropertyChangedHandler( String info ) ;

		protected void NotifyPropertyChanged( String info )
		{
			if ( PropertyChanged != null && App.Current != null )
			{
				if ( App.Current.Dispatcher.CheckAccess() )
				{
					PropertyChanged( this, new PropertyChangedEventArgs( info ) ) ;
				}
				else
				{
					App.Current.Dispatcher.BeginInvoke( DispatcherPriority.Normal,
					                                  new NotifyPropertyChangedHandler( NotifyPropertyChanged ), info ) ;
				}
			}
		}
	}
}