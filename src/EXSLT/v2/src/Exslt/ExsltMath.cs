#region using

using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections.Generic;

using Mvp.Xml.Common.XPath;

#endregion

namespace Mvp.Xml.Exslt
{
    /// <summary>
    /// This class implements the EXSLT functions in the http://exslt.org/math namespace.
    /// </summary>
    public class ExsltMath
    {
        /// <summary>
        /// Implements the following function 
        ///    number min(node-set)
        /// </summary>
        /// <param name="iterator"></param>
        /// <returns></returns>        
        public double min(XPathNodeIterator iterator)
        {
            double min, t;

            if (iterator.Count == 0)
            {
                return Double.NaN;
            }

            try
            {

                iterator.MoveNext();
                min = XmlConvert.ToDouble(iterator.Current.Value);


                while (iterator.MoveNext())
                {
                    t = XmlConvert.ToDouble(iterator.Current.Value);
                    min = (t < min) ? t : min;
                }

            }
            catch
            {
                return Double.NaN;
            }

            return min;
        }


        /// <summary>
        /// Implements the following function 
        ///    number max(node-set)
        /// </summary>
        /// <param name="iterator"></param>
        /// <returns></returns>		
        public double max(XPathNodeIterator iterator)
        {
            double max, t;

            if (iterator.Count == 0)
            {
                return Double.NaN;
            }

            try
            {

                iterator.MoveNext();
                max = XmlConvert.ToDouble(iterator.Current.Value);

                while (iterator.MoveNext())
                {
                    t = XmlConvert.ToDouble(iterator.Current.Value);
                    max = (t > max) ? t : max;
                }

            }
            catch
            {
                return Double.NaN;
            }

            return max;
        }


        /// <summary>
        /// Implements the following function 
        ///    node-set highest(node-set)
        /// </summary>
        /// <param name="iterator">The input nodeset</param>
        /// <returns>All the nodes that contain the max value in the nodeset</returns>		
        public XPathNodeIterator highest(XPathNodeIterator iterator)
        {
            if (iterator.Count == 0)
            {
                return EmptyXPathNodeIterator.Instance;
            }            
            
            double max, t;
            List<XPathNavigator> newList = new List<XPathNavigator>();

            try
            {
                iterator.MoveNext();                
                max = XmlConvert.ToDouble(iterator.Current.Value);
                newList.Add(iterator.Current.Clone());

                while (iterator.MoveNext())
                {
                    t = XmlConvert.ToDouble(iterator.Current.Value);

                    if (t > max)
                    {
                        max = t;
                        newList.Clear();
                        newList.Add(iterator.Current.Clone());
                    }
                    else if (t == max)
                    {
                        newList.Add(iterator.Current.Clone());
                    }
                }
            }
            catch
            { 
                //return empty node set                
                return EmptyXPathNodeIterator.Instance;
            }

            return new XPathNavigatorIterator(newList);
        }


        /// <summary>
        /// Implements the following function 
        ///    node-set lowest(node-set)
        /// </summary>
        /// <param name="iterator">The input nodeset</param>
        /// <returns>All the nodes that contain the min value in the nodeset</returns>		
        public XPathNodeIterator lowest(XPathNodeIterator iterator)
        {
            if (iterator.Count == 0)
            {
                return EmptyXPathNodeIterator.Instance;
            }

            double max, t;
            List<XPathNavigator> newList = new List<XPathNavigator>();

            try
            {
                iterator.MoveNext();
                max = XmlConvert.ToDouble(iterator.Current.Value);
                newList.Add(iterator.Current.Clone());

                while (iterator.MoveNext())
                {
                    t = XmlConvert.ToDouble(iterator.Current.Value);

                    if (t < max)
                    {
                        max = t;
                        newList.Clear();
                        newList.Add(iterator.Current.Clone());
                    }
                    else if (t == max)
                    {
                        newList.Add(iterator.Current.Clone());
                    }
                }
            }
            catch
            {
                //return empty node set                
                return EmptyXPathNodeIterator.Instance;
            }

            return new XPathNavigatorIterator(newList);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number abs(number)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public double abs(double number)
        {
            return Math.Abs(number);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number sqrt(number)
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public double sqrt(double number)
        {
            if (number < 0)
                return 0;
            return Math.Sqrt(number);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number power(number, number)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double power(double x, double y)
        {
            return Math.Pow(x, y);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number log(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double log(double x)
        {
            return Math.Log(x);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number constant(string, number)
        /// </summary>        
        /// <returns>The specified constant or NaN</returns>
        /// <remarks>This method only supports the constants 
        /// E and PI. Also the precision parameter is ignored.</remarks>
        public double constant(string c, double precision)
        {
            switch (c.ToUpper())
            {
                case "E":
                    return Math.E;
                case "PI":
                    return Math.PI;
                case "SQRRT2":
                    return Math.Sqrt(2);
                case "LN2":
                    return Math.Log(2);
                case "LN10":
                    return Math.Log(10);
                case "LOG2E":
                    return Math.Log(Math.E, 2);
                case "SQRT1_2":
                    return Math.Sqrt(.5);
                default:
                    return Double.NaN;
            }
        }

        /// <summary>
        ///  Implements the following function 
        ///     number random()
        /// </summary>        
        /// <returns></returns>
        public double random()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            return rand.NextDouble();
        }

        /// <summary>
        ///  Implements the following function 
        ///     number sin(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double sin(double x)
        {
            return Math.Sin(x);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number asin(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double asin(double x)
        {
            return Math.Asin(x);
        }


        /// <summary>
        ///  Implements the following function 
        ///     number cos(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double cos(double x)
        {
            return Math.Cos(x);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number acos(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double acos(double x)
        {
            return Math.Acos(x);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number tan(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double tan(double x)
        {
            return Math.Tan(x);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number atan(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double atan(double x)
        {
            return Math.Atan(x);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number atan2(number, number)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public double atan2(double x, double y)
        {
            return Math.Atan2(x, y);
        }

        /// <summary>
        ///  Implements the following function 
        ///     number exp(number)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double exp(double x)
        {
            return Math.Exp(x);
        }
    }
}

