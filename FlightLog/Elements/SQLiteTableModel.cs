// 
// SQLiteTableModel.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Jeffrey Stedfast
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using SQLite;

namespace FlightLog {
	public enum SQLiteOrderByDirection {
		Ascending,
		Descending
	}
	
	public class SQLiteOrderBy {
		public SQLiteOrderByDirection Direction { get; private set; }
		public string FieldName { get; private set; }
		
		public SQLiteOrderBy (string field) : this (field, SQLiteOrderByDirection.Ascending) { }
		
		public SQLiteOrderBy (string field, SQLiteOrderByDirection dir)
		{
			FieldName = field;
			Direction = dir;
		}
		
		public override string ToString ()
		{
			if (Direction == SQLiteOrderByDirection.Descending)
				return string.Format ("\"{0}\" desc", FieldName);
			
			return string.Format ("\"{0}\"", FieldName);
		}
	}
	
	[AttributeUsage (AttributeTargets.Property)]
	public class SQLiteSearchAliasAttribute : Attribute {
		public SQLiteSearchAliasAttribute (string alias)
		{
			Alias = alias;
		}
		
		public string Alias {
			get; private set;
		}
	}
	
	public class SQLiteTableModel<T> : IDisposable where T : new ()
	{
		class SectionTitle {
			public string Title { get; set; }
		}
		
		
		Dictionary<string, List<string>> aliases = new Dictionary<string, List<string>> (StringComparer.InvariantCultureIgnoreCase);
		Dictionary<string, Type> types = new Dictionary<string, Type> (StringComparer.InvariantCultureIgnoreCase);
		SQLiteWhereExpression searchExpr = null;
		List<T> cache = new List<T> ();
		string searchText = null;
		TableMapping titleMap;
		string[] titles;
		int sections;
		int[] rows;
		int offset;
		
		void Initialize (Type type)
		{
			List<string> list;
			
			foreach (var prop in type.GetProperties (BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)) {
				if (!prop.CanWrite)
					continue;
				
				if (prop.GetCustomAttributes (typeof (IgnoreAttribute), true).Length > 0)
					continue;
				
				if (prop.GetCustomAttributes (typeof (PrimaryKeyAttribute), true).Length > 0)
					continue;
				
				var attrs = prop.GetCustomAttributes (typeof (SQLiteSearchAliasAttribute), true);
				
				if (attrs != null && attrs.Length > 0) {
					foreach (var attr in attrs) {
						SQLiteSearchAliasAttribute alias = (SQLiteSearchAliasAttribute) attr;
						
						if (!aliases.TryGetValue (alias.Alias, out list))
							aliases[alias.Alias] = list = new List<string> ();
						
						list.Add (prop.Name);
					}
				} else {
					if (!aliases.TryGetValue (prop.Name, out list))
						aliases[prop.Name] = list = new List<string> ();
					
					list.Add (prop.Name);
				}
				
				types.Add (prop.Name, prop.PropertyType);
			}
		}
		
		public SQLiteTableModel (SQLiteConnection sqlitedb, int pageSize, SQLiteOrderBy orderBy, string sectionExpr)
		{
			SectionExpression = sectionExpr;
			Connection = sqlitedb;
			PageSize = pageSize;
			OrderBy = orderBy;
			
			if (sectionExpr != null) {
				titleMap = new TableMapping (typeof (SectionTitle));
				titleMap.Columns[0].Name = sectionExpr;
			}
			
			Initialize (typeof (T));
			Refresh ();
		}
		
		public SQLiteConnection Connection {
			get; private set;
		}
		
		protected string SectionExpression {
			get; private set;
		}
		
		public SQLiteOrderBy OrderBy {
			get; private set;
		}
		
		public int PageSize {
			get; private set;
		}
		
		public SQLiteWhereExpression SearchExpression {
			get { return searchExpr; }
			set {
				if (value == searchExpr)
					return;
				
				searchExpr = value;
				Refresh ();
			}
		}
		
		public string SearchText {
			get { return searchText; }
			set {
				if (value == searchText)
					return;
				
				SearchExpression = ParseSearchExpression (value);
				searchText = value;
			}
		}
		
		public string TableName {
			get { return Connection.Table<T> ().Table.TableName; }
		}
		
		class SearchToken {
			public string FieldName;
			public string Match;
			
			public SearchToken (string field, string match)
			{
				FieldName = field;
				Match = match;
			}
		}
		
		static string GetNextToken (string text, ref int i, bool allowField, out bool quoted)
		{
			while (i < text.Length && char.IsWhiteSpace (text[i]))
				i++;
			
			quoted = false;
			
			if (i == text.Length)
				return null;
			
			int start = i;
			int length;
			
			if (text[i] != '"') {
				while (i < text.Length && !char.IsWhiteSpace (text[i])) {
					if (text[i] == ':' && allowField)
						break;
					
					i++;
				}
				
				length = i - start;
			} else {
				quoted = true;
				
				start = ++i;
				while (i < text.Length && text[i] != '"')
						i++;
				
				length = i - start;
				
				if (i < text.Length) {
					// consume the end quote
					i++;
				}
			}
			
			if (length > 0)
				return text.Substring (start, length);
			
			return null;
		}
		
		SQLiteWhereExpression ParseSearchExpression (string text)
		{
			SQLiteWhereExpression where = new SQLiteWhereExpression ();
			SQLiteAndExpression and = new SQLiteAndExpression ();
			List<string> fields;
			bool quoted;
			
			for (int i = 0; i < text.Length; i++) {
				string token = GetNextToken (text, ref i, true, out quoted);
				
				if (i < text.Length && text[i] == ':') {
					i++;
					
					if (string.IsNullOrEmpty (token))
						continue;
					
					string match = GetNextToken (text, ref i, false, out quoted);
					
					if (match != null) {
						if (aliases.TryGetValue (token, out fields)) {
							SQLiteOrExpression or = new SQLiteOrExpression ();
							
							foreach (var field in fields)
								or.Children.Add (new SQLiteLikeExpression (field, match));
							
							and.Children.Add (or);
						}
					} else {
						SQLiteOrExpression or = new SQLiteOrExpression ();
						
						foreach (var col in types) {
							if (col.Value == typeof (string))
								or.Children.Add (new SQLiteLikeExpression (col.Key, token));
						}
						
						and.Children.Add (or);
					}
				} else if (token != null) {
					SQLiteOrExpression or = new SQLiteOrExpression ();
					
					if (!quoted && aliases.TryGetValue (token, out fields)) {
						foreach (var field in fields) {
							if (types[field] == typeof (bool))
								or.Children.Add (new SQLiteIsExpression (field, true));
						}
					}
					
					foreach (var col in types) {
						if (col.Value == typeof (string))
							or.Children.Add (new SQLiteLikeExpression (col.Key, token));
					}
					
					and.Children.Add (or);
				}
			}
			
			if (!and.HasChildren)
				return null;
			
			where.Children.Add (and);
			
			return where;
		}
		
		protected virtual SQLiteCommand CreateSectionCountCommand ()
		{
			if (SectionExpression == null)
				return null;
			
			string query = "select count (distinct " + SectionExpression + ") from \"" + TableName + "\"";
			object[] args;
			
			if (SearchExpression != null)
				query += " " + SearchExpression.ToString (out args);
			else
				args = new object [0];
			
			return Connection.CreateCommand (query, args);
		}
		
		public int SectionCount {
			get {
				if (sections == -1) {
					var cmd = CreateSectionCountCommand ();
					if (cmd != null)
						sections = cmd.ExecuteScalar<int> ();
					else
						sections = 1;
				}
				
				return sections;
			}
		}
		
		protected virtual SQLiteCommand CreateSectionTitlesCommand ()
		{
			if (SectionExpression == null)
				return null;
			
			string query = "select distinct " + SectionExpression + " from \"" + TableName + "\" as Title";
			object[] args;
			
			if (SearchExpression != null)
				query += " " + SearchExpression.ToString (out args);
			else
				args = new object [0];
			
			if (OrderBy != null)
				query += " order by " + OrderBy.ToString ();
			
			return Connection.CreateCommand (query, args);
		}
		
		public string[] SectionTitles {
			get {
				if (titles == null) {
					var cmd = CreateSectionTitlesCommand ();
					if (cmd != null)
						titles = cmd.ExecuteQuery<SectionTitle> (titleMap).Select (x => x.Title).ToArray ();
				}
				
				return titles;
			}
		}
		
		protected virtual SQLiteCommand CreateRowCountCommand (int section)
		{
			string query = "select count (*) from \"" + TableName + "\"";
			object[] args;
			
			if (SectionExpression != null) {
				SQLiteWhereExpression where = new SQLiteWhereExpression ();
				SQLiteAndExpression and = new SQLiteAndExpression ();
				
				and.Children.Add (new SQLiteExactExpression (SectionExpression, SectionTitles[section]));
				if (SearchExpression != null && SearchExpression.HasChildren)
					and.Children.AddRange (SearchExpression.Children);
				
				where.Children.Add (and);
				
				query += " " + where.ToString (out args);
			} else if (SearchExpression != null) {
				query += " " + SearchExpression.ToString (out args);
			} else {
				args = new object [0];
			}
			
			return Connection.CreateCommand (query, args);
		}
		
		public int GetRowCount (int section)
		{
			if (rows == null) {
				rows = new int [SectionCount];
				for (int i = 0; i < rows.Length; i++)
					rows[i] = -1;
			}
			
			if (section >= rows.Length)
				return -1;
			
			if (rows[section] == -1) {
				var cmd = CreateRowCountCommand (section);
				rows[section] = cmd.ExecuteScalar<int> ();
			}
			
			return rows[section];
		}
		
		int SectionAndRowToIndex (int section, int row)
		{
			int index = 0;
			
			for (int i = 0; i < section; i++)
				index += GetRowCount (i);
			
			return index + row;
		}
		
		protected virtual SQLiteCommand CreateQueryCommand (int limit, int offset)
		{
			string query = "select * from \"" + TableName + "\"";
			List<object> args = new List<object> ();
			
			if (SearchExpression != null) {
				object[] searchArgs;
				
				query += " " + SearchExpression.ToString (out searchArgs);
				args.AddRange (searchArgs);
			}
			
			if (OrderBy != null)
				query += " order by " + OrderBy.ToString ();
			
			query += " limit " + limit + " offset " + offset;
			
			return Connection.CreateCommand (query, args.ToArray ());
		}
		
		public T GetItem (int section, int row)
		{
			int index = SectionAndRowToIndex (section, row);
			int limit = PageSize;
			
			Connection.Trace = true;
			
			if (index == offset - 1) {
				// User is scrolling up. Fetch the previous page of items...
				int first = Math.Max (offset - PageSize, 0);
				
				// Calculate the number of items we need to fetch...
				limit = offset - first;
				
				// Calculate the number of items we need to uncache...
				int rem = limit - ((2 * PageSize) - cache.Count);
				
				if (rem > 0)
					cache.RemoveRange (cache.Count - rem, rem);
				
				var cmd = CreateQueryCommand (limit, first);
				var results = cmd.ExecuteQuery<T> ();
				
				// Insert our new items at the head of our cache list...
				cache.InsertRange (0, results);
				offset = first;
			} else if (index == offset + cache.Count) {
				// User is scrolling down. Fetch the next page of items...
				if (cache.Count > PageSize)
					cache.RemoveRange (0, cache.Count - PageSize);
				
				// Load 2 pages if we are at the beginning
				if (index == 0)
					limit = 2 * PageSize;
				
				var cmd = CreateQueryCommand (limit, index);
				var results = cmd.ExecuteQuery<T> ();
				
				offset = Math.Max (index - PageSize, 0);
				cache.AddRange (results);
			} else if (index < offset || index > offset + cache.Count) {
				// User is requesting an item in the middle of no-where...
				// align to the page enclosing the given index.
				// Note: this only works if PageSize is a power of 2.
				//int first = ((index + (PageSize - 1)) & ~(PageSize - 1)) - PageSize;
				int first = (index / PageSize) * PageSize;
				
				limit = 2 * PageSize;
				cache.Clear ();
				
				var cmd = CreateQueryCommand (limit, first);
				var results = cmd.ExecuteQuery<T> ();
				cache.AddRange (results);
				offset = first;
			}
			
			Connection.Trace = false;
			
			index -= offset;
			if (index < cache.Count)
				return cache[index];
			
			return default (T);
		}
		
		public virtual void Refresh ()
		{
			cache.Clear ();
			titles = null;
			sections = -1;
			rows = null;
			offset = 0;
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			cache.Clear ();
		}
		#endregion
	}
}
