using System;
using System.ComponentModel ;
using agsXMPP ;
using agsXMPP.protocol.iq.disco ;

namespace xeus.Core
{
	internal class ServiceItem : NotifyInfoDispatcher
	{
		private string _name = String.Empty ;
		private Jid _jid = null ;
		private DiscoInfo _disco = null ;

		private bool _isRegistered = false ;

		private ObservableCollectionDisp< DiscoFeature > _features = new ObservableCollectionDisp< DiscoFeature >( App.DispatcherThread ) ;
		private string _type = String.Empty ;
		private string _category = String.Empty ;

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
				NotifyPropertyChanged( "Name" );
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

				DiscoIdentity [] discoIdentities = value.GetIdentities() ;

				if ( discoIdentities != null && discoIdentities.Length > 0 )
				{
					if ( !string.IsNullOrEmpty( discoIdentities[ 0 ].Name ) )
					{
						Name = discoIdentities[ 0 ].Name ;
					}

					Type = discoIdentities[ 0 ].Type ;
					Category = discoIdentities[ 0 ].Category ;
				}

				NotifyPropertyChanged( "Disco" );
				NotifyPropertyChanged( "Features" );
				NotifyPropertyChanged( "CanRegister" );
			}
		}

		public string Type
		{
			get
			{
				return _type ;
			}

			set
			{
				_type = value ;
				NotifyPropertyChanged( "Type" );
			}
		}

		public string Category
		{
			get
			{
				return _category ;
			}

			set
			{
				_category = value ;
				NotifyPropertyChanged( "Category" );
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
	}
}
