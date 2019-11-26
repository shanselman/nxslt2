#region using 

using System;
using System.Reflection;
using System.Xml.Xsl;
using System.Xml.XPath;

#endregion

namespace Mvp.Xml.Exslt 
{
	/// <summary>
	/// IXsltContextFunction wrapper around extension function.
	/// </summary>
	internal class ExsltContextFunction : IXsltContextFunction {
	    private MethodInfo _method;
	    private XPathResultType[] _argTypes;
	    private object _ownerObj;	    
	    
		public ExsltContextFunction(MethodInfo mi, XPathResultType[] argTypes, 
		    object owner) {
			_method = mi;
			_argTypes = argTypes;
			_ownerObj = owner;
		}
		
		#region IXsltContextFunction implementation
		public int Minargs {
		    get { return _argTypes.Length; }
		}
		
		public int Maxargs {
		    get { return _argTypes.Length; }
		}    
		
		public XPathResultType[] ArgTypes {
		    get { return _argTypes; }
		}
		
        public XPathResultType ReturnType {
            get { return ExsltContext.ConvertToXPathType(_method.ReturnType); }
        }
		
		public object Invoke(XsltContext xsltContext, object[] args, 
		    XPathNavigator docContext) {
		    return _method.Invoke(_ownerObj, args);
		}
								
		#endregion
	}
}

