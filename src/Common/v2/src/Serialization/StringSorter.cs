#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Mvp.Xml.Common.Serialization
{
	/// <summary>
	/// Helper class to simpify sorting
	/// strings (Not really necessary in Whidbey).
	/// </summary>
	public class StringSorter
	{
		List<string> list = new List<string>();
		
		/// <summary>
		/// Helper class to sort strings alphabetically
		/// </summary>
		public StringSorter()
		{

		}

		/// <summary>
		/// Add a string to sort
		/// </summary>
		/// <param name="s"></param>
		public void AddString( string s )
		{
			list.Add(s);
		}

		/// <summary>
		/// Sort the strings that were added by calling
		/// <see cref="AddString"/>
		/// </summary>
		/// <returns>A sorted string array.</returns>
		public string[] GetOrderedArray()
		{
			list.Sort();
			return list.ToArray();
		}
	}
}
