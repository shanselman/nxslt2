#region using 

using System;
using System.Resources;
using System.Threading;

#endregion

namespace Mvp.Xml.XInclude
{           
    /// <summary>Contains resources for the application.</summary>
    internal sealed class SR
    {                
		private static ResourceManager resourceManager = 
			new ResourceManager(typeof(SR).FullName, typeof(SR).Module.Assembly );

		private SR() {}

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
        public static string AttributeOrNamespaceInIncludeLocationError
        {
            get
            {
                return SR.GetString("AttributeOrNamespaceInIncludeLocationError");
            }
        }                

        /// <summary></summary>
        public static string IntradocumentReferencesNotSupported
        {
            get
            {
                return SR.GetString("IntradocumentReferencesNotSupported");
            }
        }        
        
        /// <summary></summary>
        public static string CustomXmlResolverError
        {
            get
            {
                return SR.GetString("CustomXmlResolverError");
            }
        }        

        /// <summary></summary>
        public static string CustomXmlResolverReturnedNull
        {
            get
            {
                return SR.GetString("CustomXmlResolverError");
            }
        }                
        
        /// <summary></summary>
        public static string MalformedXInclusionResult
        {
            get
            {
                return SR.GetString("MalformedXInclusionResult");
            }
        }        
        

            /// <summary></summary>
        public static string FragmentIDInHref
        {
            get
            {
                return SR.GetString("FragmentIDInHref");
            }
        }

    }
}
