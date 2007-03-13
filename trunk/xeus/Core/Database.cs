using System ;
using System.Collections.Generic ;
using System.Data ;
using System.IO ;
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

		string Path
		{
			get
			{
				return string.Format( "{0}\\{1}", Storage.GetDbFolder(), "Default.xeusdb" ) ;
			}
		}

		private void OpenDatabase()
		{
			xdoc = new XmlDocument();

			try
			{
				RootName = "xeus" ;
				Load( Path ) ;
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
					RosterItem rosterItem = new RosterItem( row ) ;
					rosterItems.Add( rosterItem ) ;

					DataTable dataMsg = Query( "Roster/LastMessagesFrom", string.Format( "@Key='{0}'", rosterItem.Key ) ) ;

					if ( dataMsg.Rows.Count > 0 )
					{
						rosterItem.LastMessageFrom = new ChatMessage( dataMsg.Rows[ 0 ], rosterItem );
					}

					dataMsg = Query( "Roster/LastMessagesTo", string.Format( "@Key='{0}'", rosterItem.Key ) ) ;

					if ( dataMsg.Rows.Count > 0 )
					{
						rosterItem.LastMessageTo = new ChatMessage( dataMsg.Rows[ 0 ], rosterItem );
					}
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
		}

		public void InsertMessage( ChatMessage message )
		{
			lock ( message )
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

		new public void Save()
		{
			try
			{
				// do backup
				string backupPath = string.Format( "{0}.backup", Path ) ;
				File.Delete( backupPath ) ;
				File.Copy( Path, backupPath ) ;
			}

			catch
			{
				
			}

			try
			{
				base.Save();
			}

			catch
			{
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