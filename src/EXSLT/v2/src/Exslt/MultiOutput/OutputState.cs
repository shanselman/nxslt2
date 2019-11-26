#region using

using System;
using System.Xml;
using System.Text;
using System.IO;

#endregion

namespace Mvp.Xml.Exslt 
{   

    /// <summary>
    /// XSLT output method enumeration, see W3C XSLT 1.0 Recommendation at 
    /// <a href="http://www.w3.org/TR/xslt.html#output">http://www.w3.org/TR/xslt.html#output</a>.    
    /// </summary>
    /// <remarks>Only <c>xml</c> and <c>text</c> methods are supported by this version of 
    /// the <c>MultiXmlTextWriter</c>.</remarks>
    internal enum OutputMethod { Xml, Text };
    
    /// <summary>
    /// This class represents redirected output state and properties.
    /// </summary>    
    internal class OutputState {            
        private XmlTextWriter xmlWriter;                        
        private StreamWriter textWriter;
        private int depth;
        private string href;                           
        private Encoding encoding;        
        private bool indent;        
        private string publicDoctype;        
        private string systemDoctype;        
        private bool standalone;        
        private string storedDir;
        private OutputMethod method;
        private bool omitXmlDecl;
                                                                                                     
        /// <summary>
        /// Creates new <c>OutputState</c> with default properties values:
        /// UTF8 encoding, no indentation, nonstandalone document, XML output
        /// method.
        /// </summary>                                    
        public OutputState() {
            encoding = System.Text.Encoding.UTF8;
            indent = false;
            standalone = false;
            omitXmlDecl = false;
            method = OutputMethod.Xml;
        }                                   
         
        /// <summary>
        /// Initializes the writer to write redirected output. 
        /// </summary>              
        /// <remarks>Depending on the <c>method</c> attribute value, 
        /// <c>XmlTextWriter</c> or <c>StreamWriter</c> is created. 
        /// <c>XmlTextWriter</c> is used for outputting XML and  
        /// <c>StreamWriter</c> - for plain text.
        /// </remarks>
        public void InitWriter() {
            // Save current directory
            storedDir = Directory.GetCurrentDirectory();
            DirectoryInfo dir = Directory.GetParent(href);
            if (!dir.Exists)
                dir.Create();
            // Create writer
            if (method == OutputMethod.Xml) {
                xmlWriter = new XmlTextWriter(href, encoding);
                if (indent)
                    xmlWriter.Formatting = Formatting.Indented;
                if (!omitXmlDecl) 
                {
                    if (standalone)    
                        xmlWriter.WriteStartDocument(true);
                    else
                        xmlWriter.WriteStartDocument();
                }
            } else
                textWriter = new StreamWriter(href, false, encoding);
            // Set new current directory            
            Directory.SetCurrentDirectory(dir.ToString());                                    
        }      
        
        /// <summary>
        /// Closes the writer that was used to write redirected output.
        /// </summary>
        public void CloseWriter() {
            if (method == OutputMethod.Xml) {
                if (!omitXmlDecl) 
                {
                    xmlWriter.WriteEndDocument();
                }
                xmlWriter.Close();    
            } else
                textWriter.Close();
            // Restore previous current directory
            Directory.SetCurrentDirectory(storedDir);
        }
                                                   
        /// <summary>
        /// Specifies whether the result document should be written with 
        /// a standalone XML document declaration.
        /// </summary>
        /// <value>Standalone XML declaration as per W3C XSLT 1.0 Recommendation (see 
        /// <a href="http://www.w3.org/TR/xslt.html#output">http://www.w3.org/TR/xslt.html#output</a>
        /// for more info).</value>
        /// <remarks>The property does not affect output while output method is <c>text</c>.</remarks>            
        public bool Standalone {
            get { return standalone; }
            set { standalone = value; }
        }        
        
        /// <summary>
        /// Specifies output method.
        /// </summary>
        /// <value>Output Method as per W3C XSLT 1.0 Recommendation (see 
        /// <a href="http://www.w3.org/TR/xslt.html#output">http://www.w3.org/TR/xslt.html#output</a>
        /// for more info).</value>        
        public OutputMethod Method {
            get { return method; }
            set { method = value; }
        }
        
        /// <summary>
        /// Specifies the URI where the result document should be written to.
        /// </summary>                            
        /// <value>Absolute or relative URI of the output document.</value>
        public string Href {
            get { return href; }
            set { href = value; }
        }
        
        /// <summary>
        /// Specifies the preferred character encoding of the result document.
        /// </summary>
        /// <value>Output encoding as per W3C XSLT 1.0 Recommendation (see 
        /// <a href="http://www.w3.org/TR/xslt.html#output">http://www.w3.org/TR/xslt.html#output</a>
        /// for more info).</value>
        public Encoding Encoding {
            get { return encoding; }
            set { encoding = value; }
        }
        
        /// <summary>
        /// Specifies whether the result document should be written in the 
        /// indented form.
        /// </summary>
        /// <value>Output document formatting as per W3C XSLT 1.0 Recommendation (see 
        /// <a href="http://www.w3.org/TR/xslt.html#output">http://www.w3.org/TR/xslt.html#output</a>
        /// for more info).</value>
        /// <remarks>The property does not affect output while output method is <c>text</c>.</remarks>
        public bool Indent {
            get { return indent; }
            set { indent = value; }
        }
        
        /// <summary>
        /// Specifies the public identifier to be used in the document 
        /// type declaration.
        /// </summary>
        /// <value>Public part of the output document type definition as per W3C XSLT 1.0 Recommendation (see 
        /// <a href="http://www.w3.org/TR/xslt.html#output">http://www.w3.org/TR/xslt.html#output</a>
        /// for more info).</value>
        /// <remarks>The property does not affect output while output method is <c>text</c>.</remarks>
        public string PublicDoctype {
            get { return publicDoctype; }
            set { publicDoctype = value; }
        }
        
        /// <summary>
        /// Specifies the system identifier to be used in the document 
        /// type declaration.
        /// </summary>
        /// <value>System part of the output document type definition as per W3C XSLT 1.0 Recommendation (see 
        /// <a href="http://www.w3.org/TR/xslt.html#output">http://www.w3.org/TR/xslt.html#output</a>
        /// for more info).</value>
        /// <remarks>The property does not affect output while output method is <c>text</c>.</remarks>
        public string SystemDoctype {
            get { return systemDoctype; }
            set { systemDoctype = value; }
        }                                    
                                    
        /// <summary>
        /// Actual <c>XmlTextWriter</c> used to write the redirected 
        /// result document.
        /// </summary>              
        /// <value><c>XmlWriter</c>, which is used to write the output document in XML method.</value>                      
        public XmlTextWriter XmlWriter {
            get { return xmlWriter; }                                
        }
        
        /// <summary>
        /// Actual <c>TextWriter</c> used to write the redirected 
        /// result document in text output method.
        /// </summary>                                    
        /// <value><c>StreamWriter</c>, which is used to write the output document in text method.</value>
        public StreamWriter TextWriter {
            get { return textWriter; }                                
        }    
            
        /// <summary>
        /// Tree depth (used to detect end tag of the <c>exsl:document</c>).
        /// </summary>
        /// <value>Current output tree depth.</value>
        public int Depth {
            get { return depth; }
            set { depth = value; }
        }
        
        /// <summary>
        /// Specifies whether the XSLT processor should output an XML declaration.        
        /// </summary>        
        public bool OmitXmlDeclaration
        {
            get { return omitXmlDecl; }
            set { omitXmlDecl = value; }
        }                                 
    }
}
