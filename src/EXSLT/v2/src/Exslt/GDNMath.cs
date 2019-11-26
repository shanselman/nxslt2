#region using

using System;
using System.Xml;
using System.Xml.XPath;

#endregion

namespace Mvp.Xml.Exslt
{
    /// <summary>
    /// This class implements addditional functions in the http://gotdotnet.com/exslt/math namespace.
    /// </summary>
    public class GDNMath 
    {
        
        /// <summary>
        /// Implements the following function 
        ///    number avg(node-set)
        /// </summary>
        /// <param name="iterator"></param>
        /// <returns>The average of all the value of all the nodes in the 
        /// node set</returns>
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
        public double avg(XPathNodeIterator iterator)
        {

            double sum = 0; 
            int count = iterator.Count;

            if(count == 0)
            {
                return Double.NaN; 
            }

            try
            { 
                while(iterator.MoveNext())
                {
                    sum += XmlConvert.ToDouble(iterator.Current.Value);
                }
				
            }
            catch(FormatException)
            {
                return Double.NaN; 
            }			 

            return sum / count; 
        }
    }
}

