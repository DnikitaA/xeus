using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Xml ;
using Clifton.Tools.Xml ;
using xeus.Properties ;

namespace xeus.Core
{
	internal class NullFieldValuePair : XmlDatabase.FieldValuePair
	{
		public NullFieldValuePair( string field, string val )
			: base( ( field == null ) ? String.Empty : field, ( val == null ) ? String.Empty : val )
		{
		}
	}

	internal class Database : XmlDatabase
	{
		private static Database _instance = new Database() ;

		public static Database Instance
		{
			get
			{
				return _instance ;
			}
		}

		public Database()
		{
			xdoc = null ;
		}

		private void OpenDatabase()
		{
			if ( xdoc == null )
			{
				xdoc = new XmlDocument();

				string path = string.Format( "{0}\\{1}", Storage.GetDbFolder(), "Default.xeusdb" ) ;

				try
				{
					RootName = "xeus" ;
					Load( path ) ;
				}

				catch ( Exception )
				{
					// does not exist
					RootName = "xeus" ;
					Create() ;
				}
			}
		}

		private void CloseDatabase()
		{
			xdoc = null ;
		}

		public List< RosterItem > ReadRosterItems()
		{
			OpenDatabase() ;

			List< RosterItem > rosterItems = new List< RosterItem >() ;

			try
			{
				DataTable data = Query( "Roster/RosterItem" ) ;

				foreach ( DataRow row in data.Rows )
				{
					rosterItems.Add( new RosterItem( row ) ) ;
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error reading Roster items: {0}", e.Message ) ;
			}

			CloseDatabase() ;

			return rosterItems ;
		}

		public List< ChatMessage > ReadMessages( RosterItem rosterItem )
		{
			OpenDatabase() ;

			List< ChatMessage > messages = new List< ChatMessage >() ;

			try
			{
				int maxMessages = Settings.Default.Roster_MaximumMessagesToLoad ;

				DataTable data = Query( "Messages/Message", string.Format( "@Key='{0}'", rosterItem.Key ) ) ;

				int i = 0 ;

				foreach ( DataRow row in data.Rows )
				{
					messages.Add( new ChatMessage( row, rosterItem ) ) ;

					if ( ++i > maxMessages )
					{
						break ;
					}
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error reading messages: {0}", e.Message ) ;
			}

			CloseDatabase() ;

			return messages ;
		}

		private void StoreMessages( ObservableCollectionDisp< ChatMessage > messages )
		{
			OpenDatabase() ;

			foreach ( ChatMessage item in messages )
			{
				try
				{
					FieldValuePair[] data = item.GetData() ;

					if ( !item.IsFromDb )
					{
						Insert( "Messages/Message", data ) ;
					}
				}

				catch ( Exception e )
				{
					Client.Instance.Log( "Error writing messages: {0}", e.Message ) ;
				}
			}
		}

		public void StoreRosterItems( ObservableCollectionDisp< RosterItem > rosterItems )
		{
			OpenDatabase() ;

			foreach ( RosterItem item in rosterItems )
			{
				try
				{
					FieldValuePair[] data = item.GetData() ;

					SaveOrUpdate( "Roster/RosterItem", string.Format( "@Key='{0}'", item.Key ), data ) ;
					StoreMessages( item.Messages ) ;
				}

				catch ( Exception e )
				{
					Client.Instance.Log( "Error writing roster items: {0}", e.Message ) ;
				}
			}
		}

		private void SaveOrUpdate( string path, string where, FieldValuePair[] fields )
		{
			if ( Update( path, where, fields ) == 0 )
			{
				Insert( path, fields ) ;
			}
		}

		public new void Save()
		{
			OpenDatabase() ;

			base.Save() ;

			CloseDatabase() ;
		}
	}
}