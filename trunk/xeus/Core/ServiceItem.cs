using System;
using System.ComponentModel ;
using agsXMPP ;
using agsXMPP.protocol.iq.disco ;

namespace xeus.Core
{
	internal class ServiceItem : INotifyPropertyChanged
	{
		private string _name = String.Empty ;
		private Jid _jid = null ;
		private DiscoInfo _disco = null ;

		private bool _isRegistered = false ;

		public event PropertyChangedEventHandler PropertyChanged ;

		private ObservableCollectionDisp< DiscoFeature > _features = new ObservableCollectionDisp< DiscoFeature >( App.DispatcherThread ) ;

		public ServiceItem( string name, Jid jid )
		{
			_name = name ;
			_jid = jid ;

			RefreshStatus() ;
		}

		public void RefreshStatus()
		{
			RosterItem rosterItem = Client.Instance.Roster.FindItem( _jid.Bare ) ;
			
			_isRegistered = ( rosterItem != null && rosterItem.IsServiceRegistered ) ;
		}

		public string Name
		{
			get
			{
				return _name ;
			}
			set
			{
				_name = value ;
			}
		}
		
		public Jid Jid
		{
			get
			{
				return _jid ;
			}
		}

		public DiscoInfo Disco
		{
			get
			{
				return _disco ;
			}

			set
			{
				_disco = value ;

				NotifyPropertyChanged( "Disco" );
				NotifyPropertyChanged( "Features" );
				NotifyPropertyChanged( "CanRegister" );
			}
		}

		public ObservableCollectionDisp< DiscoFeature > Features
		{
			get
			{
				return _features ;
			}
		}

		public bool IsRegistered
		{
			get
			{
				return _isRegistered ;
			}

			set
			{
				_isRegistered = value ;
				NotifyPropertyChanged( "IsRegistered" );
			}
		}

		public bool CanRegister
		{
			get
			{
				if ( _disco == null )
				{
					return false ;
				}

				return _disco.HasFeature( agsXMPP.Uri.IQ_REGISTER ) ;
			}
		}

		private void NotifyPropertyChanged( String info )
		{
			if ( PropertyChanged != null )
			{
				PropertyChanged( this, new PropertyChangedEventArgs( info ) ) ;
			}
		}
	}
}
