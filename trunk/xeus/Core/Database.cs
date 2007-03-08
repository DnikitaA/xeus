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
		public Database()
		{
			OpenDatabase() ;
		}

		private void OpenDatabase()
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

		public List< RosterItem > ReadRosterItems()
		{
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

			return rosterItems ;
		}

		public List< ChatMessage > ReadMessages( RosterItem rosterItem )
		{
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
						messages.RemoveAt( 0 ) ;
					}
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error reading messages: {0}", e.Message ) ;
			}

			return messages ;
		}

		private void StoreMessages( ObservableCollectionDisp< ChatMessage > messages )
		{
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

		public void StoreGroups( Dictionary< string, bool > expanderStates )
		{
			foreach ( KeyValuePair< string, bool > state in expanderStates )
			{
				FieldValuePair [] data = new FieldValuePair [] { new FieldValuePair( "IsExpanded", state.Value.ToString() ),
																	new FieldValuePair( "Name", state.Key ) } ;

				SaveOrUpdate( "Roster/Groups", string.Format( "@Name='{0}'", state.Key ), data ) ;
			}
		}

		public Dictionary< string, bool > ReadGroups()
		{
			Dictionary< string, bool > expanderStates = new Dictionary< string, bool >();

			try
			{
				DataTable data = Query( "Roster/Groups" ) ;

				foreach ( DataRow row in data.Rows )
				{
					expanderStates.Add( ( string )row[ "Name" ], bool.Parse( ( string )row[ "IsExpanded" ] ) ) ;
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error reading groups: {0}", e.Message ) ;
			}
			
			return expanderStates ;
		}

		public void StoreRosterItems( ObservableCollectionDisp< RosterItem > rosterItems )
		{
			foreach ( RosterItem item in rosterItems )
			{
				if ( item.IsService )
				{
					continue ;
				}

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
	}
}