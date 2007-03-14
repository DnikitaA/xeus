using System ;
using System.Collections.Generic ;
using System.Data ;
using System.Data.Common ;
using System.Text ;
using xeus.Properties ;

namespace xeus.Core
{
	internal class Database
	{
		private DbProviderFactory _factoryProvider = DbProviderFactories.GetFactory( "System.Data.SQLite" ) ;

		private string Path
		{
			get
			{
				return string.Format( "{0}\\{1}", Storage.GetDbFolder(), "xeus.db" ) ;
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
			using ( DbConnection connection = _factoryProvider.CreateConnection() )
			{
				connection.ConnectionString = string.Format( "Data Source=\"{0}\"", Path ) ;
				connection.Open() ;

				foreach ( RosterItem item in rosterItems )
				{
					if ( item.IsService )
					{
						continue ;
					}

					try
					{
						SaveOrUpdate( item.GetData(), "Key", "RosterItem", connection ) ;
					}

					catch ( Exception e )
					{
						Client.Instance.Log( "Error writing roster items: {0}", e.Message ) ;
					}
				}
			}
		}

		private void SaveOrUpdate( Dictionary< string, object > values, string keyField, string table, DbConnection connection )
		{
			StringBuilder query = new StringBuilder() ;

			query.AppendFormat( "SELECT * FROM {0} WHERE [{1}]='{2}'", table, keyField, values[ keyField ].ToString() ) ;

			DbCommand command = connection.CreateCommand() ;
			command.CommandText = query.ToString() ;
			DbDataReader reader = command.ExecuteReader() ;

			bool exists = reader.HasRows ;

			reader.Close() ;

			StringBuilder queryUpdate = new StringBuilder() ;

			if ( exists )
			{
				queryUpdate.AppendFormat( "UPDATE {0} SET ", table ) ;

				bool isFirst = true ;

				foreach ( KeyValuePair< string, object > pair in values )
				{
					if ( !isFirst )
					{
						queryUpdate.Append( "," ) ;
					}

					isFirst = false ;

					if ( pair.Value is Int32 )
					{
						queryUpdate.AppendFormat( "{0}={1}", pair.Key, pair.Value ) ;
					}
					else
					{
						queryUpdate.AppendFormat( "{0}='{1}'", pair.Key, pair.Value ) ;
					}
				}
			}
			else
			{
				queryUpdate.AppendFormat( "INSERT INTO {0} (", table ) ;

				bool isFirst = true ;

				foreach ( KeyValuePair< string, object > pair in values )
				{
					if ( !isFirst )
					{
						queryUpdate.Append( "," ) ;
					}

					isFirst = false ;

					queryUpdate.Append( pair.Key ) ;
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

					if ( pair.Value is Int32 )
					{
						queryUpdate.Append( pair.Value ) ;
					}
					else
					{
						queryUpdate.AppendFormat( "'{0}'", pair.Value ) ;
					}
				}
			}

			queryUpdate.Append( ")" ) ;

			command.CommandText = queryUpdate.ToString() ;
			command.ExecuteNonQuery() ;
		}
	}
}