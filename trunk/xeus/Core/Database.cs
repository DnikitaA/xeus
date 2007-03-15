using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Common ;
using System.Data.SQLite ;
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

		private static DbConnection _connection = null ;

		public static void OpenDatabase()
		{
			_connection = _factoryProvider.CreateConnection() ;
			_connection.ConnectionString = string.Format( "Data Source=\"{0}\"", Path ) ;
			_connection.Open() ;
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
				DbCommand command = _connection.CreateCommand() ;
				command.CommandText = "SELECT * FROM [RosterItem]" ;

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

			return rosterItems ;
		}

		public List< ChatMessage > ReadMessages( RosterItem rosterItem )
		{
			List< ChatMessage > messages = new List< ChatMessage >() ;

			int maxMessages = Settings.Default.Roster_MaximumMessagesToLoad ;

			try
			{
				DbCommand command = _connection.CreateCommand() ;

				command.CommandText =
					string.Format( "SELECT TOP {0} FROM [Message] WHERE [Key]=@key ORDER BY [Id] DESC", maxMessages ) ;

				command.Parameters.Add( new SQLiteParameter( "key", rosterItem.Key ) ) ;

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


			return messages ;
		}

		public ChatMessage GetChatMessage( Int64 id, RosterItem rosterItem )
		{
			ChatMessage chatMessage = null ;

			try
			{
				DbCommand command = _connection.CreateCommand() ;

				command.CommandText = "SELECT * FROM [Message] WHERE [Id]=@id" ;

				command.Parameters.Add( new SQLiteParameter( "Id", id ) ) ;

				DbDataReader reader = command.ExecuteReader() ;

				while ( reader.Read() )
				{
					chatMessage = new ChatMessage( reader, rosterItem ) ;
				}

				reader.Close() ;
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

				id = SaveOrUpdate( values, null, "Message", "Id", _connection ) ;
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
				foreach ( KeyValuePair< string, bool > state in expanderStates )
				{
					Dictionary< string, object > values = new Dictionary< string, object >() ;

					values.Add( "Name", state.Key ) ;
					values.Add( "IsExpander", ( state.Value ) ? 1 : 0 ) ;

					SaveOrUpdate( values, "Name", "Group", null, _connection ) ;
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
				DbCommand command = _connection.CreateCommand() ;

				command.CommandText = "SELECT * FROM [Group]" ;

				DbDataReader reader = command.ExecuteReader() ;

				while ( reader.Read() )
				{
					expanderStates.Add( ( string ) reader[ "Name" ], ( ( Int64 ) reader[ "IsExpander" ] ) == 1 ) ;
				}

				reader.Close() ;
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
				foreach ( RosterItem item in rosterItems )
				{
					if ( item.IsService )
					{
						continue ;
					}

					try
					{
						SaveOrUpdate( item.GetData(), "Key", "RosterItem", null, _connection ) ;
					}

					catch ( Exception e )
					{
						Client.Instance.Log( "Error writing roster items: {0}", e.Message ) ;
					}
				}
			}
		}

		private Int32 SaveOrUpdate( Dictionary< string, object > values, string keyField, string table, string identityField,
		                            DbConnection connection )
		{
			bool exists = false ;

			int id = 0 ;

			if ( keyField != null )
			{
				StringBuilder query = new StringBuilder() ;

				query.AppendFormat( "SELECT * FROM [{0}] WHERE [{1}]=@keyparam", table, keyField ) ;

				DbCommand command = connection.CreateCommand() ;
				command.CommandText = query.ToString() ;

				command.Parameters.Add( new SQLiteParameter( "keyparam", values[ keyField ] ) ) ;

				DbDataReader reader = command.ExecuteReader() ;

				exists = reader.HasRows ;

				reader.Close() ;
			}


			DbCommand commandUpdate = connection.CreateCommand() ;

			StringBuilder queryUpdate = new StringBuilder() ;

			if ( exists )
			{
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

					queryUpdate.AppendFormat( "[{0}]=@{1}", pair.Key, pair.Key ) ;

					commandUpdate.Parameters.Add( new SQLiteParameter( pair.Key, pair.Value ) ) ;
				}
			}
			else
			{
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

				if ( identityField != null )
				{
					SQLiteParameter identity = new SQLiteParameter() ;
					identity.ParameterName = identityField ;
					identity.Direction = ParameterDirection.Output ;
					commandUpdate.Parameters.Add( identity ) ;
				}
			}


			commandUpdate.CommandText = queryUpdate.ToString() ;
			commandUpdate.ExecuteNonQuery() ;

			if ( identityField != null )
			{
				id = ( Int32 ) ( Int64 ) commandUpdate.Parameters[ identityField ].Value ;
			}

			return id ;
		}
	}
}