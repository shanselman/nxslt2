#region using

using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;

#endregion

namespace Mvp.Xml.Exslt
{
    /// <summary>
    /// This class implements additional functions in the 
    /// "http://gotdotnet.com/exslt/dates-and-times" namespace.    
    /// </summary>
    public class GDNDatesAndTimes : ExsltDatesAndTimes
    {                   
      
        #region date2:avg()
        /// <summary>
        /// Implements the following function 
        ///    string date2:avg(node-set)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-avg.xml  
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
        public string avg(XPathNodeIterator iterator)
        {
            TimeSpan sum = new TimeSpan(0,0,0,0); 
            int count = iterator.Count;
            if(count == 0)
            {
                return ""; 
            }
            try
            { 
                while(iterator.MoveNext())
                {
                    sum = XmlConvert.ToTimeSpan(iterator.Current.Value).Add(sum);
                }
				
            }
            catch(FormatException)
            {
                return ""; 
            }			 

            return duration(sum.TotalSeconds / count); 
        }

        #endregion

        #region date2:min()
        /// <summary>
        /// Implements the following function 
        ///    string date2:min(node-set)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-min.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
        public string min(XPathNodeIterator iterator)
        {

            TimeSpan min, t; 

            if(iterator.Count == 0)
            {
                return ""; 
            }

            try
            { 

                iterator.MoveNext(); 
                min = XmlConvert.ToTimeSpan(iterator.Current.Value);
			
                while(iterator.MoveNext())
                {
                    t = XmlConvert.ToTimeSpan(iterator.Current.Value);
                    min = (t < min)? t : min; 
                }
				
            }
            catch(FormatException)
            {
                return ""; 
            }		

            return XmlConvert.ToString(min); 
        }

        #endregion
		  
        #region date2:max()

        /// <summary>
        /// Implements the following function 
        ///    string date2:max(node-set)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-max.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
        public string max(XPathNodeIterator iterator)
        {

            TimeSpan max, t; 

            if(iterator.Count == 0)
            {
                return ""; 
            }
	
            try
            { 

                iterator.MoveNext(); 
                max = XmlConvert.ToTimeSpan(iterator.Current.Value);

			
                while(iterator.MoveNext())
                {
                    t = XmlConvert.ToTimeSpan(iterator.Current.Value);
                    max = (t > max)? t : max; 
                }
				
            }
            catch(FormatException)
            {
                return ""; 
            }		

            return XmlConvert.ToString(max); 
        }
        #endregion

        #region date2:day-abbreviation()
        
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dayAbbreviation_RENAME_ME(string d, string c) 
        {
            return dayAbbreviation(d, c);
        }

        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public new string dayAbbreviation_RENAME_ME(string c) 
        {
            return dayAbbreviation(c);
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:day-abbreviation(string)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-day-abbreviation.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The abbreviated current day name according to 
        /// specified culture or the empty string if the culture isn't 
        /// supported.</returns>
        public new string dayAbbreviation(string culture)
        {
            try
            {                
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetAbbreviatedDayName(DateTime.Now.DayOfWeek);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:day-abbreviation(string, string)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-day-abbreviation.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The abbreviated day name of the specified date according to 
        /// specified culture or the empty string if the input date is invalid or
        /// the culture isn't supported.</returns>
        public string dayAbbreviation(string d, string culture)
        {
            try
            {
                DateTZ date = new DateTZ(d); 
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetAbbreviatedDayName(date.d.DayOfWeek);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }    

        #endregion 

        #region date2:day-name()
    
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dayName_RENAME_ME(string d, string c) 
        {
            return dayName(d, c);
        }

        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public new string dayName_RENAME_ME(string c) 
        {
            return dayName(c);
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:day-name(string, string?)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-day-name.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The day name of the specified date according to 
        /// specified culture or the empty string if the input date is invalid or
        /// the culture isn't supported.</returns>
        public string dayName(string d, string culture)
        {
            try
            {                
                DateTZ date = new DateTZ(d);
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetDayName(date.d.DayOfWeek);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:day-name(string, string?)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-day-name.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The day name of the current date according to 
        /// specified culture or the empty string if
        /// the culture isn't supported.</returns>
        public new string dayName(string culture)
        {
            try
            {                                
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetDayName(DateTime.Now.DayOfWeek);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }
        
        #endregion 
      
        #region date2:month-abbreviation()
        
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string monthAbbreviation_RENAME_ME(string d, string c) 
        {
            return monthAbbreviation(d, c);
        }

        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public new string monthAbbreviation_RENAME_ME(string c) 
        {
            return monthAbbreviation(c);
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:month-abbreviation(string)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-month-abbreviation.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The abbreviated current month name according to 
        /// specified culture or the empty string if the culture isn't 
        /// supported.</returns>
        public new string monthAbbreviation(string culture)
        {
            try
            {                
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetAbbreviatedMonthName(DateTime.Now.Month);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:month-abbreviation(string, string)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-month-abbreviation.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The abbreviated month name of the specified date according to 
        /// specified culture or the empty string if the input date is invalid or
        /// the culture isn't supported.</returns>
        public string monthAbbreviation(string d, string culture)
        {
            try
            {
                DateTZ date = new DateTZ(d); 
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetAbbreviatedMonthName(date.d.Month);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }    

        #endregion

        #region date2:month-name()
            
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string monthName_RENAME_ME(string d, string c) 
        {
            return monthName(d, c);
        }

        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public new string monthName_RENAME_ME(string c) 
        {
            return monthName(c);
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:month-name(string, string?)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-month-name.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The month name of the specified date according to 
        /// specified culture or the empty string if the input date is invalid or
        /// the culture isn't supported.</returns>
        public string monthName(string d, string culture)
        {
            try
            {                
                DateTZ date = new DateTZ(d);
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetMonthName(date.d.Month);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }

        /// <summary>
        /// Implements the following function 
        ///    string date2:month-name(string, string?)
        /// See http://www.xmland.net/exslt/doc/GDNDatesAndTimes-month-name.xml
        /// </summary>        
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        /// <returns>The month name of the current date according to 
        /// specified culture or the empty string if
        /// the culture isn't supported.</returns>
        public new string monthName(string culture)
        {
            try
            {                                
                CultureInfo ci = new CultureInfo(culture);
                return ci.DateTimeFormat.GetMonthName(DateTime.Now.Month);
            }
            catch (Exception)
            {
                return ""; 
            }            
        }

        #endregion
    }
}
