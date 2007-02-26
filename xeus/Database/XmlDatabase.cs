/*
(c) 2005, Marc Clifton
All Rights Reserved
 
Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list
of conditions and the following disclaimer. 

Redistributions in binary form must reproduce the above copyright notice, this
list of conditions and the following disclaimer in the documentation and/or other
materials provided with the distribution. 

Neither the name of the Marc Clifton nor the names of its contributors may be
used to endorse or promote products derived from this software without specific
prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE
OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Data;
using System.IO;
using System.Xml;

namespace Clifton.Tools.Xml
{
	/// <summary>
	/// <para>XmlDatabase is a wrapper to an xml document that provides insert, update
	/// query, and delete operations on single or multiple elements in a path,
	/// qualified by an optional "where" clause.</para>
	/// <para>If you need something more sophisticated (like an actual XmlDataProvider),
	/// please see Paul Wilson's project at:</para>
	/// <para>http://workspaces.gotdotnet.com/XmlDbClient</para>
	/// <para>http://weblogs.asp.net/pwilson/archive/2004/09/18/231185.aspx</para>
	/// </summary>
	public class XmlDatabase
	{
		/// <summary>
		/// Embedded class for constructing field-value pairs.
		/// </summary>
		public class FieldValuePair
		{
			/// <summary>
			/// The field name.
			/// </summary>
			protected string field;
			/// <summary>
			/// The value name.
			/// </summary>
			protected string val;

			/// <summary>
			/// Gets/sets the field name.
			/// </summary>
			public string Field
			{
				get {return field;}
				set {field=value;}
			}

			/// <summary>
			/// Gets/sets the field value.
			/// </summary>
			public string Value
			{
				get {return val;}
				set {val=value;}
			}

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="field">The field name.</param>
			/// <param name="val">The initial value.</param>
			/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
			public FieldValuePair(string field, string val)
			{
				if (field==null)
				{
					throw(new ArgumentNullException("field cannot be null."));
				}
				if (val==null)
				{
					throw(new ArgumentNullException("val cannot be null."));
				}


				this.field=field;
				this.val=val;
			}
		}

		/// <summary>
		/// The filename used in Load() and Save() operations.
		/// </summary>
		protected string filename;

		/// <summary>
		/// The xml root element name.
		/// </summary>
		protected string rootName;

		/// <summary>
		/// The xml document.
		/// </summary>
		protected XmlDocument xdoc;

		/// <summary>
		/// Get/set the filename to save to/from.
		/// </summary>
		public string FileName
		{
			get {return filename;}
			set {filename=value;}
		}

		/// <summary>
		/// Get/set the root element name.
		/// </summary>
		public string RootName
		{
			get {return rootName;}
			set {rootName=value;}
		}

		/// <summary>
		/// Default constructor.  Assigns a default name to the xml root element.
		/// </summary>
		public XmlDatabase()
		{
			rootName="XmlDatabase";
			Create();
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="rootName">The name of the xml root element.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		public XmlDatabase(string rootName)
		{
			if (rootName==null)
			{
				throw(new ArgumentNullException("root name cannot be null."));
			}

			this.rootName=rootName;
			Create();
		}

		/// <summary>
		/// Creates a new xml document instance using the assigned name for the
		/// root element.  The new xml document becomes the working document.
		/// </summary>
		public void Create()
		{
			xdoc=new XmlDocument();
			XmlDeclaration xmlDeclaration=xdoc.CreateXmlDeclaration("1.0","utf-8",null); 
			xdoc.InsertBefore(xmlDeclaration, xdoc.DocumentElement); 
		}

		#region IO

		/// <summary>
		/// Load an xml document with the previoulsy assigned filename.
		/// </summary>
		/// <exception cref="System.NullReferenceException">Thrown when filename is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public void Load()
		{
			if (filename==null)
			{
				throw(new ArgumentNullException("filename cannot be null."));
			}

			xdoc.Load(filename);
		}

		/// <summary>
		/// Load an xml document with the specified filename.
		/// </summary>
		/// <param name="filename">Loads the document from this file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public void Load(string filename)
		{
			if (filename==null)
			{
				throw(new ArgumentNullException("filename cannot be null."));
			}

			this.filename=filename;
			Load();
		}

		/// <summary>
		/// Save an xml document with the current filename.
		/// </summary>
		/// <exception cref="System.NullReferenceException">Thrown when filename is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public void Save()
		{
			if (filename==null)
			{
				throw(new NullReferenceException("filename cannot be null."));
			}

			xdoc.Save(filename);
		}

		/// <summary>
		/// Saves the document using the specified TextWriter.
		/// </summary>
		/// <param name="tw">The document is saved to this TextWriter.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument and TextWriter classes.</remarks>
		public void SaveAs(TextWriter tw)
		{
			if (tw==null)
			{
				throw(new ArgumentNullException("TextWriter cannot be null."));
			}

			xdoc.Save(tw);
			tw.Flush();
		}

		/// <summary>
		/// Saves the document to the specified StringWriter.
		/// </summary>
		/// <param name="sw">The document is saved to this StringWriter.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument and StringWriter classes.</remarks>
		public void SaveAs(StringWriter sw)
		{
			if (sw==null)
			{
				throw(new ArgumentNullException("StringWriter cannot be null."));
			}

			xdoc.Save(sw);
			sw.Flush();
		}

		/// <summary>
		/// Save the document to the specified filename.  This replaces the current
		/// filename value with the one specified.
		/// </summary>
		/// <param name="newFileName">The new filename to which the document is written.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public void SaveAs(string newFileName)
		{
			if (newFileName==null)
			{
				throw(new ArgumentNullException("filename cannot be null."));
			}

			xdoc.Save(newFileName);
		}

		#endregion

		#region Insert
		/// <summary>
		/// Inserts an empty record at the bottom of the hierarchy, creating the
		/// tree as required.
		/// </summary>
		/// <param name="path">The xml path to the bottom node.</param>
		/// <returns>The XmlNode inserted into the hierarchy.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public XmlNode Insert(string path)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}

			string path2=rootName+"/"+path;
			string[] segments=path2.Split('/');
			XmlNode lastNode=InsertNode(xdoc, segments, 0);
			return lastNode;
		}

		/// <summary>
		/// Inserts an record with a single field at the bottom of the hierarchy.
		/// </summary>
		/// <param name="path">The xml path to the bottom node.</param>
		/// <param name="field">The field to add to the record.</param>
		/// <param name="val">The value assigned to the field.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public void Insert(string path, string field, string val)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}
			if (val==null)
			{
				throw(new ArgumentNullException("val cannot be null."));
			}

			XmlNode node=Insert(path);
			XmlAttribute attrib=xdoc.CreateAttribute(field);
			attrib.Value=val;
			node.Attributes.Append(attrib);
		}

		/// <summary>
		/// Insert a record with multiple fields at the bottom of the hierarchy.
		/// </summary>
		/// <param name="path">The xml path to the bottom node.</param>
		/// <param name="fields">The array of fields as field/value pairs.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public void Insert(string path, FieldValuePair[] fields)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (fields==null)
			{
				throw(new ArgumentNullException("fields cannot be null."));
			}

			XmlNode node=Insert(path);
			foreach(FieldValuePair fvp in fields)
			{
				XmlAttribute attrib=xdoc.CreateAttribute(fvp.Field);
				attrib.Value=fvp.Value;
				node.Attributes.Append(attrib);
			}
		}
		#endregion

		#region Update
		/// <summary>
		/// Update a single field in all records in the specified path.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="field">The field name to update.</param>
		/// <param name="val">The new value.</param>
		/// <returns>The number of records affected.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public int Update(string path, string field, string val)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}
			if (val==null)
			{
				throw(new ArgumentNullException("val cannot be null."));
			}

			XmlNodeList nodeList=xdoc.SelectNodes(rootName+"/"+path);
			foreach(XmlNode node in nodeList)
			{
				node.Attributes[field].Value=val;
			}
			return nodeList.Count;
		}

		/// <summary>
		/// Update a single field in all records in the specified path matching
		/// the xml where clause.  The where clause is appended to the last element
		/// of the path.  Qualifiers applied to elements other than the last element
		/// must be specified in the path statement itself using xml syntax.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="where">The path qualifier.</param>
		/// <param name="field">The field name to update.</param>
		/// <param name="val">The new value.</param>
		/// <returns>The number of records affected.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public int Update(string path, string where, string field, string val)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}
			if (val==null)
			{
				throw(new ArgumentNullException("val cannot be null."));
			}
			if (where==null)
			{
				throw(new ArgumentNullException("where cannot be null."));
			}

			return Update(path+"["+where+"]", field, val);
		}

		/// <summary>
		/// Update multiple fields in all records in the specified path matching
		/// the xml where clause.  The where clause is appended to the last element
		/// of the path.  Qualifiers applied to elements other than the last element
		/// must be specified in the path statement itself using xml syntax.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="where">The path qualifier.</param>
		/// <param name="fields">The array of fields as field/value pairs.</param>
		/// <returns>The number of records affected.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public int Update(string path, string where, FieldValuePair[] fields)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (fields==null)
			{
				throw(new ArgumentNullException("fields cannot be null."));
			}
			if (where==null)
			{
				throw(new ArgumentNullException("where cannot be null."));
			}

			XmlNodeList nodeList=xdoc.SelectNodes(rootName+"/"+path+"["+where+"]");
			foreach(XmlNode node in nodeList)
			{
				foreach(FieldValuePair fvp in fields)
				{
					XmlAttribute attrib=xdoc.CreateAttribute(fvp.Field);
					attrib.Value=fvp.Value;
					node.Attributes.Append(attrib);
				}
			}
			return nodeList.Count;
		}
		#endregion

		#region Delete
		/// <summary>
		/// Deletes all records of the specified path.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <returns>The number of records deleted.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public int Delete(string path)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}

			XmlNodeList nodeList=xdoc.SelectNodes(rootName+"/"+path);
			foreach(XmlNode node in nodeList)
			{
				node.ParentNode.RemoveChild(node);
			}
			return nodeList.Count;
		}

		/// <summary>
		/// Deletes all records on the specified path qualified by the where clause.
		/// The where clause is appended to the last element of the path.
		/// Qualifiers applied to elements other than the last element
		/// must be specified in the path statement itself using xml syntax.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="where">The where conditions.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public int Delete(string path, string where)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (where==null)
			{
				throw(new ArgumentNullException("where cannot be null."));
			}

			return Delete(path+"["+where+"]");
		}

		/// <summary>
		/// Deletes a field from all records on the specified path.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="field">The field to delete.</param>
		/// <returns>The number of records affected.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public int DeleteField(string path, string field)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}

			XmlNodeList nodeList=xdoc.SelectNodes(rootName+"/"+path);
			foreach(XmlNode node in nodeList)
			{
				XmlAttribute attrib=node.Attributes[field];
				node.Attributes.Remove(attrib);
			}
			return nodeList.Count;
		}
		#endregion

		#region Query
		/// <summary>
		/// Return a single string representing the value of the specified field
		/// for the first record encountered.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="field">The desired field.</param>
		/// <returns>A string with the field's value or null.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public string QueryScalar(string path, string field)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}

			string ret=null;
			XmlNode node=xdoc.SelectSingleNode(rootName+"/"+path);
			if (node != null)
			{
				XmlAttribute xa=node.Attributes[field];
				if (xa != null)
				{
					ret=xa.Value;
				}
			}
			return ret;
		}

		/// <summary>
		/// Return a single string representing the value of the specified field
		/// for the first record encountered as qualified by the xml where clause.
		/// The where clause is appended to the last element of the path.
		/// Qualifiers applied to elements other than the last element
		/// must be specified in the path statement itself using xml syntax.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="field">The desired field.</param>
		/// <param name="where">The where clause.</param>
		/// <returns>A string with the field's value or null.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public string QueryScalar(string path, string field, string where)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}
			if (where==null)
			{
				throw(new ArgumentNullException("where cannot be null."));
			}

			return QueryScalar(path+"["+where+"]", field);
		}

		/// <summary>
		/// Returns a DataTable for all rows on the path.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <returns>The DataTable with the returned rows.
		/// The row count will be 0 if no rows returned.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public DataTable Query(string path)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}

			DataTable dt=new DataTable();
			XmlNodeList nodeList=xdoc.SelectNodes(rootName+"/"+path);
			if (nodeList.Count != 0)
			{
				CreateColumns(dt, nodeList[0]);
			}
			foreach(XmlNode node in nodeList)
			{
				DataRow dr=dt.NewRow();
				foreach(XmlAttribute attr in node.Attributes)
				{
					dr[attr.Name]=attr.Value;
				}
				dt.Rows.Add(dr);
			}
			return dt;
		}

		/// <summary>
		/// Returns a DataTable for all rows on the path qualified by the where
		/// clause.
		/// The where clause is appended to the last element of the path.
		/// Qualifiers applied to elements other than the last element
		/// must be specified in the path statement itself using xml syntax.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="where">The where clause.</param>
		/// <returns>The DataTable with the returned rows.
		/// The row count will be 0 if no rows returned.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public DataTable Query(string path, string where)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (where==null)
			{
				throw(new ArgumentNullException("where cannot be null."));
			}

			return Query(path+"["+where+"]");
		}

		/// <summary>
		/// Returns an array of values for the specified field for all rows on
		/// the path.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="field">The desired field.</param>
		/// <returns>The array of string values for each row qualified by the path.
		/// A null is returned if the query results in 0 rows.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public string[] QueryField(string path, string field)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}

			XmlNodeList nodeList=xdoc.SelectNodes(rootName+"/"+path);
			string[] s=null;
			if (nodeList.Count != 0)
			{
				s=new string[nodeList.Count];
				int i=0;
				foreach(XmlNode node in nodeList)
				{
					s[i++]=node.Attributes[field].Value;
				}
			}
			return s;
		}

		/// <summary>
		/// Returns an array of values for the specified field for all rows on
		/// the path.
		/// The where clause is appended to the last element of the path.
		/// Qualifiers applied to elements other than the last element
		/// must be specified in the path statement itself using xml syntax.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="where">The where clause.</param>
		/// <param name="field">The desired field.</param>
		/// <returns>The array of string values for each row qualified by the path.
		/// A null is returned if the query results in 0 rows.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public string[] QueryField(string path, string where, string field)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (field==null)
			{
				throw(new ArgumentNullException("field cannot be null."));
			}
			if (where==null)
			{
				throw(new ArgumentNullException("where cannot be null."));
			}

			return QueryField(path+"["+where+"]", field);
		}

		/// <summary>
		/// Returns a DataTable of specified fields for all rows on the specified
		/// path.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="fieldList">The string array of desired fields.</param>
		/// <returns>The DataTable.  If no rows are returned, the DataTable row
		/// count will be 0.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public DataTable Query(string path, string[] fieldList)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (fieldList==null)
			{
				throw(new ArgumentNullException("fieldList cannot be null."));
			}

			DataTable dt=new DataTable();
			XmlNodeList nodeList=xdoc.SelectNodes(rootName+"/"+path);
			CreateColumns(dt, fieldList);
			foreach(XmlNode node in nodeList)
			{
				DataRow dr=dt.NewRow();
				foreach(XmlAttribute attr in node.Attributes)
				{
					if (dt.Columns.Contains(attr.Name))
					{
						dr[attr.Name]=attr.Value;
					}
				}
				dt.Rows.Add(dr);
			}
			return dt;
		}

		/// <summary>
		/// Returns a DataTable of specified fields for all rows on the specified
		/// path qualified by the where clause.
		/// The where clause is appended to the last element of the path.
		/// Qualifiers applied to elements other than the last element
		/// must be specified in the path statement itself using xml syntax.
		/// </summary>
		/// <param name="path">The xml path.</param>
		/// <param name="where">The where clause.</param>
		/// <param name="fieldList">The string array of desired fields.</param>
		/// <returns>The DataTable.  If no rows are returned, the DataTable row
		/// count will be 0.</returns>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">Thrown when an argument is null.</exception>
		/// <remarks>Additional exceptions may be thrown by the XmlDocument class.</remarks>
		public DataTable Query(string path, string where, string[] fieldList)
		{
			if (path==null)
			{
				throw(new ArgumentNullException("path cannot be null."));
			}
			if (fieldList==null)
			{
				throw(new ArgumentNullException("fieldList cannot be null."));
			}
			if (where==null)
			{
				throw(new ArgumentNullException("where cannot be null."));
			}

			return Query(path+"["+where+"]", fieldList);
		}
		#endregion

		#region Helpers
		/// <summary>
		/// Inserts a node at the specified segment if it doesn't exist, otherwise
		/// traverses the node.
		/// </summary>
		/// <param name="node">The current node.</param>
		/// <param name="segments">The path segment list.</param>
		/// <param name="idx">The current segment.</param>
		/// <returns></returns>
		protected XmlNode InsertNode(XmlNode node, string[] segments, int idx)
		{
			XmlNode newNode=null;

			if (idx==segments.Length)
			{
				// All done.
				return node;
			}

			// Traverse the existing hierarchy but ensure that we create a 
			// new record at the last leaf.
			if (idx+1 < segments.Length)
			{
				foreach(XmlNode child in node.ChildNodes)
				{
					if (child.Name==segments[idx])
					{
						newNode=InsertNode(child, segments, idx+1);
						return newNode;
					}
				}
			}
			newNode=xdoc.CreateElement(segments[idx]);
			node.AppendChild(newNode);
			XmlNode nextNode=InsertNode(newNode, segments, idx+1);
			return nextNode;
		}

		/// <summary>
		/// Creates columns given an XmlNode.
		/// </summary>
		/// <param name="dt">The target DataTable.</param>
		/// <param name="node">The source XmlNode.</param>
		protected void CreateColumns(DataTable dt, XmlNode node)
		{
			foreach(XmlAttribute attr in node.Attributes)
			{
				dt.Columns.Add(new DataColumn(attr.Name));
			}
		}

		/// <summary>
		/// Creates columns given a string array.
		/// </summary>
		/// <param name="dt">The target DataTable.</param>
		/// <param name="fieldList">The string array of fields</param>
		protected void CreateColumns(DataTable dt, string[] fieldList)
		{
			foreach(string s in fieldList)
			{
				dt.Columns.Add(new DataColumn(s));
			}
		}
		#endregion Helpers
	}
}