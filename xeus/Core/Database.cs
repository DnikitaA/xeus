using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Common ;
using xeus.Properties ;

namespace xeus.Core
{
	/*
	internal class NullFieldValuePair : XmlDatabase.FieldValuePair
	{
		public NullFieldValuePair( string field, string val )
			: base( ( field == null ) ? String.Empty : field, ( val == null ) ? String.Empty : val )
		{
		}
	}*/

	internal class Database
	{
		private DbProviderFactory _factoryProvider = DbProviderFactories.GetFactory( "System.Data.SQLite" ) ;

		public Database()
		{
		}

		private string Path
		{
			get
			{
				return string.Format( "{0}\\{1}", Storage.GetDbFolder(), "Default.xeusdb" ) ;
			}
		}

		public List< RosterItem > ReadRosterItems()
		{
			List< RosterItem > rosterItems = new List< RosterItem >() ;

			using ( DbConnection connection = _factoryProvider.CreateConnection() )
			{
				connection.ConnectionString = string.Format( "Data Source=\"{0}\"", Path ) ;
				connection.Open() ;

				try
				{
					DbCommand command = connection.CreateCommand() ;
					command.CommandText = "SELECT * FROM RosterItem" ;
					command.CommandType = CommandType.Text ;
					DbDataReader reader = command.ExecuteReader() ;

					while ( reader.Read() )
					{
						RosterItem rosterItem = new RosterItem( reader ) ;
						rosterItems.Add( rosterItem ) ;
					}

					reader.Close() ;
				}

				catch ( Exception e )
				{
					Client.Instance.Log( "Error reading Roster items: {0}", e.Message ) ;
				}
			}

			return rosterItems ;
		}

		public List< ChatMessage > ReadMessages( RosterItem rosterItem )
		{
			List< ChatMessage > messages = new List< ChatMessage >() ;

			using ( DbConnection connection = _factoryProvider.CreateConnection() )
			{
				int maxMessages = Settings.Default.Roster_MaximumMessagesToLoad ;

				connection.ConnectionString = string.Format( "Data Source=\"{0}\"", Path ) ;
				connection.Open() ;

				try
				{
					DbCommand command = connection.CreateCommand() ;
					command.CommandText = string.Format( "SELECT TOP {0} * FROM Message WHERE Key='{1}' ORDER BY Id DESC",
					                                     maxMessages, rosterItem.Key ) ;

					command.CommandType = CommandType.Text ;
					DbDataReader reader = command.ExecuteReader() ;

					while ( reader.Read() )
					{
						messages.Insert( 0, new ChatMessage( reader, rosterItem ) ) ;
					}

					reader.Close() ;
				}

				catch ( Exception e )
				{
					Client.Instance.Log( "Error reading messages: {0}", e.Message ) ;
				}
			}

			return messages ;
		}

		/*
		private void StoreMessages( ObservableCollectionDisp< ChatMessage > messages )
		{
			lock ( messages._syncObject )
			{
				foreach ( ChatMessage item in messages )
				{
					if ( !item.IsFromDb )
					{
						try
						{
							FieldValuePair[] data = item.GetData() ;

							Insert( "Messages/Message", data ) ;
						}

						catch ( Exception e )
						{
							Client.Instance.Log( "Error writing messages: {0}", e.Message ) ;
						}
					}
				}
			}
		}*/

		/*
		public void InsertMessage( ChatMessage message )
		{
			if ( !message.IsFromDb )
			{
				FieldValuePair[] data = message.GetData() ;

				try
				{
					if ( !message.IsFromDb )
					{
						Insert( "Messages/Message", data ) ;
					}

					message.IsFromDb = true ;
				}

				catch ( Exception e )
				{
					Client.Instance.Log( "Error writing messages: {0}", e.Message ) ;
				}
			}
		}*/

		public void StoreGroups( Dictionary< string, bool > expanderStates )
		{
			foreach ( KeyValuePair< string, bool > state in expanderStates )
			{
				/*
				FieldValuePair[] data = new FieldValuePair[]
					{
						new FieldValuePair( "IsExpanded", state.Value.ToString() ),
						new FieldValuePair( "Name", state.Key )
					} ;

				SaveOrUpdate( "Roster/Groups", string.Format( "@Name='{0}'", state.Key ), data ) ;*/
			}
		}

		public Dictionary< string, bool > ReadGroups()
		{
			Dictionary< string, bool > expanderStates = new Dictionary< string, bool >() ;

			using ( DbConnection connection = _factoryProvider.CreateConnection() )
			{
				int maxMessages = Settings.Default.Roster_MaximumMessagesToLoad ;

				connection.ConnectionString = string.Format( "Data Source=\"{0}\"", Path ) ;
				connection.Open() ;

				try
				{
					DbCommand command = connection.CreateCommand() ;
					command.CommandText = "SELECT * FROM Group" ;

					command.CommandType = CommandType.Text ;
					DbDataReader reader = command.ExecuteReader() ;

					while ( reader.Read() )
					{
						expanderStates.Add( ( string ) reader[ "Name" ], bool.Parse( ( string ) reader[ "IsExpanded" ] ) ) ;
					}

					reader.Close() ;
				}

				catch ( Exception e )
				{
					Client.Instance.Log( "Error reading groups: {0}", e.Message ) ;
				}
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
					StoreLastMessages( item ) ;
				}

				catch ( Exception e )
				{
					Client.Instance.Log( "Error writing roster items: {0}", e.Message ) ;
				}
			}
		}

		private void StoreLastMessages( RosterItem rosterItem )
		{
			FieldValuePair[] data ;

			if ( rosterItem.LastMessageFrom != null )
			{
				data = rosterItem.LastMessageFrom.GetData() ;
				SaveOrUpdate( "Roster/LastMessagesFrom", string.Format( "@Key='{0}'", rosterItem.Key ), data ) ;
			}

			if ( rosterItem.LastMessageTo != null )
			{
				data = rosterItem.LastMessageTo.GetData() ;
				SaveOrUpdate( "Roster/LastMessagesTo", string.Format( "@Key='{0}'", rosterItem.Key ), data ) ;
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