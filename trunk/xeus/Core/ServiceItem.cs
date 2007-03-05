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

		public ServiceItem( string name, Jid jid, DiscoInfo disco )
		{
			_name = name ;
			_jid = jid ;
			_disco = disco ;

			foreach ( DiscoFeature discoFeature in disco.GetFeatures() )
			{
				Features.Add( discoFeature );
			}

			_isRegistered = ( Client.Instance.Roster.FindItem( jid.Bare ) != null ) ;
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
