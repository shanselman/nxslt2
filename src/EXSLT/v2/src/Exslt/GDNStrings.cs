namespace Mvp.Xml.Exslt {
    /// <summary>
    ///   This class implements additional functions in the http://gotdotnet.com/exslt/strings namespace.
    /// </summary>		
    public class GDNStrings {
        /// <summary>
        /// Implements the following function 
        ///		string uppercase(string)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks>THIS FUNCTION IS NOT IN EXSLT!!!</remarks>
        public string uppercase(string str)
        {
            return str.ToUpper(); 
        }

        /// <summary>
        /// Implements the following function 
        ///		string lowercase(string)
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        /// <remarks>THIS FUNCTION IS NOT IN EXSLT!!!</remarks>
        public string lowercase(string str)
        {
            return str.ToLower(); 
        }		        
    }   
}
