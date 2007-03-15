using System ;
using System.Collections.Generic ;
using System.Data.Common ;
using System.Data.SQLite ;
using System.IO ;
using System.Text ;
using xeus.Properties ;

namespace xeus.Core
{
	internal class Database
	{
		private static DbProviderFactory _factoryProvider = DbProviderFactories.GetFactory( "System.Data.SQLite" ) ;

		private static string Path
		{
			get
			{
				return string.Format( "{0}\\{1}", Storage.GetDbFolder(), "xeus.db" ) ;
			}
		}

		private static SQLiteConnection _connection = null ;

		public static void OpenDatabase()
		{
			bool dbExisist = File.Exists( Path ) ;

			_connection = ( SQLiteConnection ) _factoryProvider.CreateConnection() ;
			_connection.ConnectionString = string.Format( "Data Source=\"{0}\"", Path ) ;
			_connection.Open() ;

			if ( !dbExisist )
			{
				CreateDatabase() ;
			}
		}

		private static void CreateDatabase()
		{
			using ( SQLiteCommand cmd = _connection.CreateCommand() )
			{
				cmd.CommandText = "CREATE TABLE [Group] ([IsExpander] INTEGER NOT NULL DEFAULT '0',"
				                  + "[Name] VARCHAR NOT NULL PRIMARY KEY UNIQUE);" ;
				cmd.ExecuteNonQuery() ;

				cmd.CommandText = "CREATE TABLE [Message] (Key VARCHAR NOT NULL, "
				                  + "[SentFrom] VARCHAR NOT NULL, "
				                  + "[SentTo] VARCHAR NOT NULL, "
				                  + "[Time] INTEGER NOT NULL, "
				                  + "[Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE, "
				                  + "[Body] VARCHAR NOT NULL);" ;
				cmd.ExecuteNonQuery() ;

				cmd.CommandText = "CREATE TABLE [RosterItem] (Key VARCHAR NOT NULL PRIMARY KEY UNIQUE, "
				                  + "[SubscriptionType] CHAR NOT NULL, "
				                  + "[FullName] VARCHAR, [NickName] VARCHAR, "
				                  + "[CustomName] VARCHAR, [IdLastMessageFrom] INTEGER, "
				                  + "[IdLastMessageTo] INTEGER);" ;
				cmd.ExecuteNonQuery() ;
			}
		}

		public static void CloseDatabase()
		{
			_connection.Close() ;
		}

		public List< RosterItem > ReadRosterItems()
		{
			List< RosterItem > rosterItems = new List< RosterItem >() ;

			try
			{
				using ( SQLiteCommand command = _connection.CreateCommand() )
				{
					command.CommandText = "SELECT * FROM [RosterItem]" ;

					SQLiteDataReader reader = command.ExecuteReader() ;

					while ( reader.Read() )
					{
						RosterItem rosterItem = new RosterItem( reader ) ;
						rosterItems.Add( rosterItem ) ;
					}

					reader.Close() ;
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

			int maxMessages = Settings.Default.Roster_MaximumMessagesToLoad ;

			try
			{
				using ( SQLiteCommand command = _connection.CreateCommand() )
				{
					command.CommandText =
						string.Format( "SELECT * FROM [Message] WHERE [Key]=@key ORDER BY [Id] DESC LIMIT {0}", maxMessages ) ;

					command.Parameters.Add( new SQLiteParameter( "key", rosterItem.Key ) ) ;

					SQLiteDataReader reader = command.ExecuteReader() ;

					while ( reader.Read() )
					{
						messages.Insert( 0, new ChatMessage( reader, rosterItem ) ) ;
					}

					reader.Close() ;
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error reading messages: {0}", e.Message ) ;
			}

			return messages ;
		}

		public ChatMessage GetChatMessage( Int64 id, RosterItem rosterItem )
		{
			ChatMessage chatMessage = null ;

			try
			{
				using ( SQLiteCommand command = _connection.CreateCommand() )
				{
					command.CommandText = "SELECT * FROM [Message] WHERE [Id]=@id" ;

					command.Parameters.Add( new SQLiteParameter( "Id", id ) ) ;

					SQLiteDataReader reader = command.ExecuteReader() ;

					while ( reader.Read() )
					{
						chatMessage = new ChatMessage( reader, rosterItem ) ;
					}

					reader.Close() ;
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error reading messages: {0}", e.Message ) ;
			}

			return chatMessage ;
		}

		public int InsertMessage( ChatMessage message )
		{
			int id = 0 ;

			try
			{
				Dictionary< string, object > values = message.GetData() ;

				id = Insert( values, "Message", true, _connection ) ;
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error writing groups: {0}", e.Message ) ;
			}

			return id ;
		}

		public void StoreGroups( Dictionary< string, bool > expanderStates )
		{
			try
			{
				using ( SQLiteTransaction transaction = _connection.BeginTransaction() )
				{
					foreach ( KeyValuePair< string, bool > state in expanderStates )
					{
						Dictionary< string, object > values = new Dictionary< string, object >() ;

						values.Add( "Name", state.Key ) ;
						values.Add( "IsExpander", ( state.Value ) ? 1 : 0 ) ;

						SaveOrUpdate( values, "Name", "Group", false, _connection ) ;
					}

					transaction.Commit();
				}
			}

			catch ( Exception e )
			{
				Client.Instance.Log( "Error writing groups: {0}", e.Message ) ;
			}
		}

		public Dictionary< string, bool > ReadGroups()
		{
			Dictionary< string, bool > expanderStates = new Dictionary< string, bool >() ;

			try
			{
				using ( SQLiteCommand command = _connection.CreateCommand() )
				{
					command.CommandText = "SELECT * FROM [Group]" ;

					SQLiteDataReader reader = command.ExecuteReader() ;

					while ( reader.Read() )
					{
						expanderStates.Add( ( string ) reader[ "Name" ], ( ( Int64 ) reader[ "IsExpander" ] ) == 1 ) ;
					}

					reader.Close() ;
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
			lock ( rosterItems._syncObject )
			{
				using ( SQLiteTransaction transaction = _connection.BeginTransaction() )
				{
					foreach ( RosterItem item in rosterItems )
					{
						if ( item.IsService )
						{
							continue ;
						}

						try
						{
							if ( !item.IsInDatabase )
							{
								Insert( item.GetData(), "RosterItem", false, _connection ) ;
								item.IsInDatabase = true ;
								item.IsDirty = false ;
							}
							else if ( item.IsDirty )
							{
								Update( item.GetData(), "Key", "RosterItem", _connection ) ;
								item.IsDirty = false ;
							}
						}

						catch ( Exception e )
						{
							Client.Instance.Log( "Error writing roster items: {0}", e.Message ) ;
						}
					}

					transaction.Commit();
				}
			}
		}

		public void DeleteRosterItem( RosterItem rosterItem )
		{
			Delete( "RosterItem", "Key", rosterItem.Key, _connection ) ;
		}

		void Delete( string table, string keyField, object keyValue, SQLiteConnection connection )
		{
			using ( SQLiteCommand commandDelete = connection.CreateCommand() )
			{
				StringBuilder queryDelete = new StringBuilder() ;

				queryDelete.AppendFormat( "DELETE FROM [{0}] WHERE [{1}]=@{1}", table, keyField ) ;
				
				commandDelete.Parameters.Add( new SQLiteParameter( keyField, keyValue ) ) ;

				commandDelete.CommandText = queryDelete.ToString() ;
				commandDelete.ExecuteNonQuery() ;
			}
		}

		private Int32 Insert( Dictionary< string, object > values, string table, bool readAutoId,
		                      SQLiteConnection connection )
		{
			using ( SQLiteCommand commandUpdate = connection.CreateCommand() )
			{
				StringBuilder queryUpdate = new StringBuilder() ;

				queryUpdate.AppendFormat( "INSERT INTO [{0}] (", table ) ;

				bool isFirst = true ;

				foreach ( KeyValuePair< string, object > pair in values )
				{
					if ( !isFirst )
					{
						queryUpdate.Append( "," ) ;
					}

					isFirst = false ;

					queryUpdate.AppendFormat( "[{0}]", pair.Key ) ;
				}

				queryUpdate.Append( ") VALUES (" ) ;

				isFirst = true ;

				foreach ( KeyValuePair< string, object > pair in values )
				{
					if ( !isFirst )
					{
						queryUpdate.Append( "," ) ;
					}

					isFirst = false ;

					queryUpdate.AppendFormat( "@{0}", pair.Key ) ;

					commandUpdate.Parameters.Add( new SQLiteParameter( pair.Key, pair.Value ) ) ;
				}

				queryUpdate.Append( ")" ) ;

				commandUpdate.CommandText = queryUpdate.ToString() ;
				commandUpdate.ExecuteNonQuery() ;
			}

			if ( readAutoId )
			{
				return GetlastRowId( connection ) ;
			}
			else
			{
				return 0 ;
			}
		}

		private void Update( Dictionary< string, object > values, string keyField, string table, SQLiteConnection connection )
		{
			using ( SQLiteCommand commandUpdate = connection.CreateCommand() )
			{
				StringBuilder queryUpdate = new StringBuilder() ;

				queryUpdate.AppendFormat( "UPDATE [{0}] SET ", table ) ;

				bool isFirst = true ;

				foreach ( KeyValuePair< string, object > pair in values )
				{
					if ( string.Compare( keyField, pair.Key, true ) == 0 )
					{
						continue ;
					}

					if ( !isFirst )
					{
						queryUpdate.Append( "," ) ;
					}

					isFirst = false ;

					queryUpdate.AppendFormat( "[{0}]=@{0}", pair.Key ) ;

					commandUpdate.Parameters.Add( new SQLiteParameter( pair.Key, pair.Value ) ) ;
				}

				queryUpdate.AppendFormat( " WHERE [{0}]=@{0}", keyField ) ;
				commandUpdate.Parameters.Add( new SQLiteParameter( keyField, values[ keyField ] ) ) ;

				commandUpdate.CommandText = queryUpdate.ToString() ;
				commandUpdate.ExecuteNonQuery() ;
			}
		}

		private Int32 SaveOrUpdate( Dictionary< string, object > values, string keyField, string table, bool readAutoId,
		                            SQLiteConnection connection )
		{
			bool exists = false ;

			int id = 0 ;

			if ( keyField != null )
			{
				StringBuilder query = new StringBuilder() ;

				query.AppendFormat( "SELECT * FROM [{0}] WHERE [{1}]=@keyparam", table, keyField ) ;

				using ( SQLiteCommand command = connection.CreateCommand() )
				{
					command.CommandText = query.ToString() ;

					command.Parameters.Add( new SQLiteParameter( "keyparam", values[ keyField ] ) ) ;

					SQLiteDataReader reader = command.ExecuteReader() ;

					exists = reader.HasRows ;

					reader.Close() ;
				}
			}

			if ( exists )
			{
				Update( values, keyField, table, connection ) ;
			}
			else
			{
				id = Insert( values, table, readAutoId, connection ) ;
			}

			return id ;
		}

		private int GetlastRowId( SQLiteConnection connection )
		{
			int id = 0 ;

			using ( SQLiteCommand command = connection.CreateCommand() )
			{
				command.CommandText = "SELECT last_insert_rowid()" ;

				SQLiteDataReader reader = command.ExecuteReader() ;

				if ( reader.Read() )
				{
					id = ( Int32 ) ( Int64 ) reader[ 0 ] ;
				}

				reader.Close() ;
			}

			return id ;
		}
	}
}