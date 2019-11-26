#region using 

using System;
using System.Resources;
using System.Threading;

#endregion

namespace Mvp.Xml.XPointer
{           
    /// <summary>Contains resources for the application.</summary>
    internal static class SR
    {                
		private static ResourceManager resourceManager = 
			new ResourceManager(typeof(SR).FullName, typeof(SR).Module.Assembly );
		
		/// <summary>
		/// Gets the specified resource for the <see cref='Thread.CurrentUICulture'/>.
		/// </summary>
		/// <param name='key'>The key of the resource to retrieve.</param>
		/// <returns>The object resource.</returns>
		public static object GetObject(string key) 
		{
			return resourceManager.GetObject(key);
		}

		/// <summary>
		/// Gets the specified resource for the <see cref='Thread.CurrentUICulture'/>.
		/// </summary>
		/// <param name='key'>The key of the resource to retrieve.</param>
		/// <returns>The string resource.</returns>
		public static string GetString(string key) 
		{
			return resourceManager.GetString(key);
		}

		/// <summary>
		/// Gets the specified resource for the <see cref='Thread.CurrentUICulture'/> and 
		/// formats it with the arguments received.
		/// </summary>
		/// <param name='key'>The key of the resource to retrieve.</param>
		/// <param name='args'>The arguments to format the resource with.</param>
		/// <returns>The string resource.</returns>
		internal static string GetString (string key, params object[] args)
		{
			return String.Format(GetString(key), args);
		}
		
        /// <summary></summary>
        public static string InvalidTokenInElementSchemeWhileNumberExpected
        {
            get
            {
                return SR.GetString("InvalidTokenInElementSchemeWhileNumberExpected");
            }
        }                

        /// <summary></summary>
        public static string ZeroIndexInElementSchemechildSequence
        {
            get
            {
                return SR.GetString("ZeroIndexInElementSchemechildSequence");
            }
        }                

        /// <summary></summary>
        public static string InvalidTokenInElementSchemeWhileClosingRoundBracketExpected
        {
            get
            {
                return SR.GetString("InvalidTokenInElementSchemeWhileClosingRoundBracketExpected");
            }
        }                

        /// <summary></summary>
        public static string EmptyElementSchemeXPointer
        {
            get
            {
                return SR.GetString("EmptyElementSchemeXPointer");
            }
        }                                      

        /// <summary></summary>
        public static string InvalidTokenInXmlnsSchemeWhileNCNameExpected
        {
            get
            {
                return SR.GetString("InvalidTokenInXmlnsSchemeWhileNCNameExpected");
            }
        }                

        /// <summary></summary>
        public static string InvalidTokenInXmlnsSchemeWhileEqualsSignExpected
        {
            get
            {
                return SR.GetString("InvalidTokenInXmlnsSchemeWhileEqualsSignExpected");
            }
        }                             
        
        /// <summary></summary>
        public static string NullXPointer
        {
            get
            {
                return SR.GetString("NullXPointer");
            }
        }       

        /// <summary></summary>
        public static string CircumflexCharMustBeEscaped
        {
            get
            {
                return SR.GetString("CircumflexCharMustBeEscaped");
            }
        }                  

        /// <summary></summary>
        public static string UnexpectedEndOfSchemeData
        {
            get
            {
                return SR.GetString("UnexpectedEndOfSchemeData");
            }
        }       

        /// <summary></summary>
        public static string InvalidTokenAfterShorthandPointer
        {
            get
            {
                return SR.GetString("InvalidTokenAfterShorthandPointer");
            }
        }     
  
        /// <summary></summary>
        public static string InvalidToken
        {
            get
            {
                return SR.GetString("InvalidToken");
            }
        }        
    }
}
