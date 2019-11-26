#region using

using System;
using System.Collections.Generic;
using System.Xml.XPath; 
using System.Xml;
using System.Web.UI;

using Mvp.Xml.Common.XPath;

#endregion
	
namespace Mvp.Xml.Exslt 
{
	/// <summary>
	///   This class implements additional functions in the http://gotdotnet.com/exslt/sets namespace.
	/// </summary>		
	public class GDNSets 
	{
				
		/// <summary>
		/// Implements the following function 
		///    boolean subset(node-set, node-set) 
		/// </summary>
		/// <param name="nodeset1">An input nodeset</param>
		/// <param name="nodeset2">Another input nodeset</param>
		/// <returns>True if all the nodes in the first nodeset are contained 
		/// in the second nodeset</returns>
		/// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
		public bool subset(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2)
		{
			if(nodeset1.Count > 125 || nodeset2.Count > 125)
				return subset2(nodeset1, nodeset2);
			//else
            XPathNavigatorIterator nodelist1 = new XPathNavigatorIterator(nodeset1, true);
            XPathNavigatorIterator nodelist2 = new XPathNavigatorIterator(nodeset2, true);

			foreach(XPathNavigator nav in nodelist1)
			{
				if(!nodelist2.Contains(nav))
				{
					return false; 
				}
			}					
			return true; 
		} 


		/// <summary>
		/// Implements the following function 
		///    boolean subset(node-set, node-set) 
		/// This is an optimized version, using document identification
		/// and binary search techniques. 
		/// </summary>
		/// <param name="nodeset1">An input nodeset</param>
		/// <param name="nodeset2">Another input nodeset</param>
		/// <returns>True if all the nodes in the first nodeset are contained 
		/// in the second nodeset</returns>
		/// <author>Dimitre Novatchev</author>
		/// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
		public bool subset2(XPathNodeIterator nodeset1, XPathNodeIterator nodeset2)
		{
			List<Pair> arDocs = new List<Pair>();

			List<XPathNavigator> arNodes2 = new List<XPathNavigator>(nodeset2.Count);

			while(nodeset2.MoveNext())
			{
				arNodes2.Add(nodeset2.Current.Clone());
			}

			auxEXSLT.findDocs(arNodes2, arDocs);

			while(nodeset1.MoveNext())
			{
				XPathNavigator currNode = nodeset1.Current; 
				
				if(!auxEXSLT.findNode(arNodes2, arDocs, currNode) )
					return false;
			}

			return true;
		}
	}
}