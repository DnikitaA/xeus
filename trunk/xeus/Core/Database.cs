using System;
using System.Collections.Generic;
using System.IO ;
using System.Runtime.Serialization.Formatters.Binary ;
using System.Text;
using System.Xml ;
using Clifton.Tools.Xml;

namespace xeus.Core
{
	class Database
	{
		private static Database _instance = new Database();
		XmlDatabase _xmlDatabase = new XmlDatabase();

		public static Database Instance
		{
			get
			{
				return _instance;
			}
		}

		public Database()
		{
			string path = string.Format( "{0}\\{1}", Storage.GetDbFolder(), "Default" ) ;

			try
			{
				_xmlDatabase.Load( path ) ;
			}

			catch ( XmlException )
			{
				// does not exist
				_xmlDatabase.RootName = "xeus" ;
				_xmlDatabase.Create();
			}
		}

		public void SaveRosterItems( ObservableCollectionDisp< RosterItem > rosterItems )
		{
			string path = string.Format( "{0}\\{1}", Storage.GetDbFolder(), "Roster" ) ;

			using ( FileStream file = File.Create( path ) )
			{
				BinaryFormatter formatter = new BinaryFormatter() ;
				formatter.Serialize( file, rosterItems ) ;

				file.Flush();
				file.Close();
			}
		}
	}
}
