using System ;
using System.Collections.Generic ;
using System.Data ;
using Clifton.Tools.Xml ;

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

		private FieldValuePair[] GetData( RosterItem item )
		{
			FieldValuePair[] data = new FieldValuePair[ 4 ] ;

			data[ 0 ] = new NullFieldValuePair( "Key", item.Key ) ;
			data[ 1 ] = new NullFieldValuePair( "Name", item.Name ) ;
			data[ 2 ] = new NullFieldValuePair( "FullName", item.FullName ) ;
			data[ 3 ] = new NullFieldValuePair( "NickName", item.NickName ) ;

			return data ;
		}

		public List< RosterItem > ReadRosterItems()
		{
			List< RosterItem > rosterItems = new List< RosterItem >( );

			DataTable data = Query( "Roster/RosterItem" ) ;

			foreach ( DataRow row in data.Rows )
			{
				rosterItems.Add( new RosterItem( row ) );
			}

			return rosterItems ;
		}

		public void StoreRosterItems( ObservableCollectionDisp< RosterItem > rosterItems )
		{
			foreach ( RosterItem item in rosterItems )
			{
				FieldValuePair[] data = GetData( item ) ;

				SaveOrUpdate( "Roster/RosterItem", string.Format( "@Key='{0}'", item.Key ), data ) ;
			}
		}

		void SaveOrUpdate( string path, string where, FieldValuePair[] fields )
		{
			if ( Update( path, where, fields ) == 0 )
			{
				Insert( path, fields ) ;
			}
		}

		public new void Save()
		{
			base.Save() ;
		}
	}
}