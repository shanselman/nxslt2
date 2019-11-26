#region using

using System;
using System.Xml.XPath; 
using System.Xml;
using System.Text.RegularExpressions;

#endregion
	
namespace Mvp.Xml.Exslt 
{
    /// <summary>
    ///   This class implements additional functions in the 
    ///   http://gotdotnet.com/exslt/dynamic namespace.
    /// </summary>		
    public class GDNDynamic 
    {
        /// <summary>
        /// Implements the following function 
        ///    object dyn2:evaluate(node-set, string, string?) 
        /// </summary>
        /// <param name="contextNode">Context node</param>
        /// <param name="expression">Expression to evaluate</param>
        /// <returns>Result of evaluating given Xpath expression WRT to context node.</returns>
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>    
        public object evaluate(XPathNodeIterator contextNode, string expression) 
        {
            return evaluate(contextNode, expression, "");
        }
				
        /// <summary>
        /// Implements the following function 
        ///    object dyn2:evaluate(node-set, string, string?) 
        /// </summary>
        /// <param name="contextNode">Context node</param>
        /// <param name="expression">Expression to evaluate</param>
        /// <param name="namespaces">Namespace bindings</param>
        /// <returns>Result of evaluating given Xpath expression WRT to context node.</returns>
        /// <remarks>THIS FUNCTION IS NOT PART OF EXSLT!!!</remarks>
        public object evaluate(XPathNodeIterator contextNode, string expression, string namespaces)
        {                        
            if (expression == String.Empty || contextNode == null)
                return String.Empty;
            if (contextNode.MoveNext()) 
            {
                try 
                {                    
                    XPathExpression expr = contextNode.Current.Compile(expression);
                    ExsltContext context = new ExsltContext(contextNode.Current.NameTable);
                    XPathNavigator node = contextNode.Current.Clone();                    
                    if (node.NodeType != XPathNodeType.Element)
                        node.MoveToParent();
                    if (node.MoveToFirstNamespace()) 
                    {
                        do 
                        {
                            context.AddNamespace(node.Name, node.Value);
                        } while (node.MoveToNextNamespace());
                    }                    
                    if (namespaces != String.Empty) 
                    {                       
                        try 
                        {
                            Regex regexp = new Regex(@"xmlns:(?<p>\w+)\s*=\s*(('(?<n>.+)')|(""(?<n>.+)""))\s*");
                            Match m = regexp.Match(namespaces);
                            while (m.Success) 
                            {
                                try 
                                {                                    
                                    context.AddNamespace(m.Groups["p"].Value, 
                                        m.Groups["n"].Value);
                                } 
                                catch {}
                                m = m.NextMatch();
                            }
                        } 
                        catch {}
                    }                    
                    expr.SetContext(context);                    
                    return contextNode.Current.Evaluate(expr, contextNode);
                } 
                catch 
                {                    
                    //Any exception such as syntax error in XPath
                    return String.Empty;
                }                
            }
            //Empty nodeset as context node
            return String.Empty;
        } 
    }
}