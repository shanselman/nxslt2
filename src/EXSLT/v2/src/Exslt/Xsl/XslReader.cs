#region using
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Diagnostics;
using System.Threading; 
#endregion

namespace Mvp.Xml.Common.Xsl {

    /// <summary>
    /// <para>XslReader provides an efficient way to read results of an XSL 
    /// transformation via an <see cref="XmlReader"/> API. Due to 
    /// architectural and performance reasons the <see cref="XslCompiledTransform"/>
    /// class doesn't support transforming to an <see cref="XmlReader"/> as obsolete 
    /// <see cref="XslTransform"/> class did and XslReader's goal is to 
    /// supplement such functionality.</para>
    /// <para>XslReader has been developed and contributed to the Mvp.Xml project
    /// by Sergey Dubinets (Microsoft XML Team).</para>
    /// </summary>    
    /// <remarks>
    /// <para>XslReader can work in a singlethreaded (fully buffering) or a
    /// multithreaded mode.</para> 
    /// <para>In a multithreaded mode XslReader runs an XSL transformation 
    /// in a separate dedicated thread. XSLT output is being recorded into a buffer 
    /// and once the buffer is full, transformation thread gets suspended. In a main
    /// thread XslReader reads recorded XSLT output from a buffer as a client calls 
    /// XslReader methods. Whenever the buffer is read, the transformation thread
    /// is resumed to produce next portion of an XSLT output.<br/>
    /// In effect that means that an XSL transformation happens on demand portion by 
    /// portion as a client calls XslReader methods. In terms of memory footprint
    /// that means that at any time at most buffer size of XSLT output is buffered.</para>
    /// <para>In a singlethreaded mode XslReader runs an XSL transformation
    /// to the end and records full XSLT output into a buffer (using effective 
    /// binary representation though). After that it reads the buffer when a client 
    /// calls XslReader methods. So in this mode before first call to the
    /// XslReader.Read() method returns, XSL transformation is over and XSLT output 
    /// is buffered internally as a whole.    
    /// </para>
    /// <para>By default XslReader works in a multithreaded mode. You can choose the mode
    /// and the buffer size using <c>multiThread</c> and <c>initialBufferSize</c> arguments
    /// when instantiating XslReader object. On small XSLT outputs XslReader performs
    /// better in a singlethreaded mode, but on medium and big outputs multithreaded 
    /// mode is preferrable. You are adviced to measure performance in both modes to
    /// find out which suites better for your particular scenario.</para>
    /// <para>XslReader designed to be reused. Just provide another inoput XML or XSLT
    /// stylesheet, start transformation and read the output. If the <c>StartTransform()</c> 
    /// method is called when previous
    /// transformation isn't over yet, it will be aborted, the buffer cleaned and 
    /// the XslReader object will be reset to an initial state automatically.</para>
    /// <para>XslReader is not thread safe, keep separate instances of the XslReader
    /// for each thread.</para>
    /// </remarks>
    /// <example>
    /// <para>Here is an example of using XslReader class. First you need to create
    /// an <see cref="XslCompiledTransform"/> object and load XSLT stylesheet you want to
    /// execute. Then prepare XML input as <see cref="XmlInput"/> object providing
    /// XML source in a form of URI, <see cref="Stream"/>, <see cref="TextReader"/>, 
    /// <see cref="XmlReader"/> or <see cref="IXPathNavigable"/> along with an optional 
    /// <see cref="XmlResolver"/> object, which will be used to resolve URIs for 
    /// the XSLT document() function calls.<br/>
    /// After that create XslReader instance optionally choosing multithreaded or
    /// singlethreaded mode and initial buffer size.<br/>
    /// Finally start transformation by calling <c>StartTransform()</c> method and then
    /// you can read transformation output via XslReader object, which implements 
    /// <see cref="XmlReader"/> API.
    /// </para> 
    /// <para>
    /// Basic XslReader usage sample:
    /// <code>
    /// //Prepare XslCompiledTransform
    /// XslCompiledTransform xslt = new XslCompiledTransform();
    /// xslt.Load("catalog.xslt");
    /// //Prepare input XML
    /// XmlInput input = new XmlInput("books.xml");
    /// //Create XslReader
    /// XslReader xslReader = new XslReader(xslt);
    /// //Initiate transformation
    /// xslReader.StartTransform(input, null);
    /// //Now read XSLT output from the reader
    /// XPathDocument results = new XPathDocument(xslReader);
    /// </code>
    /// A more advanced sample:
    /// <code>
    /// //Prepare XslCompiledTransform
    /// XslCompiledTransform xslt = new XslCompiledTransform();
    /// xslt.Load("../../catalog.xslt");
    /// //Prepare XmlResolver to be used by the document() function
    /// XmlResolver resolver = new XmlUrlResolver();
    /// resolver.Credentials = new NetworkCredential("user42", "god");
    /// //Prepare input XML
    /// XmlInput input = new XmlInput("../../books.xml", resolver);
    /// //Create XslReader, multithreaded mode, initial buffer for 32 nodes
    /// XslReader xslReader = new XslReader(xslt, true, 32);
    /// //XSLT parameters
    /// XsltArgumentList prms = new XsltArgumentList();
    /// prms.AddParam("param2", "", "red");
    /// //Initiate transformation
    /// xslReader.StartTransform(input, prms);
    /// //Now read XSLT output from the reader
    /// XPathDocument results = new XPathDocument(xslReader);
    /// </code>
    /// </para>
    /// </example>
    public class XslReader : XmlReader {
        static string NsXml   = "http://www.w3.org/XML/1998/namespace";
        static string NsXmlNs = "http://www.w3.org/2000/xmlns/";
        static int defaultBufferSize = 256;
        XmlNameTable nameTable;
        TokenPipe    pipe;
        BufferWriter writer;
        ScopeManager scope;
        Thread       thread;

        XslCompiledTransform xslCompiledTransform;
        bool                 multiThread = false;
        int                  initialBufferSize;

        private static XmlReaderSettings ReaderSettings;
        static XslReader() {
            ReaderSettings = new XmlReaderSettings();
            ReaderSettings.ProhibitDtd = true;
        }

        // Transform Parameters
        XmlInput             defaulDocument;
        XsltArgumentList     args;

        /// <summary>
        /// Creates new XslReader instance with given <see cref="XslCompiledTransform"/>, 
        /// mode (multithreaded/singlethreaded) and initial buffer size. The buffer will be
        /// expanded if necessary to be able to store any element start tag with all its 
        /// attributes.
        /// </summary>
        /// <param name="xslTransform">Loaded <see cref="XslCompiledTransform"/> object</param>
        /// <param name="multiThread">Defines in which mode (multithreaded or singlethreaded)
        /// this instance of XslReader will operate</param>
        /// <param name="initialBufferSize">Initial buffer size (number of nodes, not bytes)</param>
        public XslReader(XslCompiledTransform xslTransform, bool multiThread, int initialBufferSize) {
            this.xslCompiledTransform = xslTransform;
            this.multiThread          = multiThread;
            this.initialBufferSize    = initialBufferSize;

            nameTable = new NameTable();
            pipe      = this.multiThread ? new TokenPipeMultiThread(initialBufferSize) : new TokenPipe(initialBufferSize);
            writer    = new BufferWriter(pipe, nameTable);
            scope     = new ScopeManager(nameTable);
            SetUndefinedState(ReadState.Initial);
        }

        /// <summary>
        /// Creates new XslReader instance with given <see cref="XslCompiledTransform"/>,
        /// operating in a multithreaded mode and having default initial buffer size.
        /// </summary>
        /// <param name="xslTransform">Loaded <see cref="XslCompiledTransform"/> object</param>
        public XslReader(XslCompiledTransform xslTransform) : this(xslTransform, true, defaultBufferSize) { }

        /// <summary>
        /// Starts XSL transformation of given <see cref="XmlInput"/> object with
        /// specified <see cref="XsltArgumentList"/>. After this method returns
        /// you can read the transformation output out of XslReader object via 
        /// standard <see cref="XmlReader"/> methods such as Read() or MoveXXX().
        /// </summary>
        /// <remarks>If the <c>StartTransform()</c> method is called when previous
        /// transformation isn't over yet, it will be aborted, buffer cleaned and 
        /// XslReader object reset to an initial state automatically.</remarks>
        /// <param name="input">An input XML to be transformed</param>
        /// <param name="args">A collection of global parameter values and
        /// extension objects.</param>
        /// <returns></returns>
        public XmlReader StartTransform(XmlInput input, XsltArgumentList args) {
            this.defaulDocument = input;
            this.args  = args;
            Start();
            return this;
        }

        private void Start() {
            if (thread != null && thread.IsAlive) {
                // We can also reuse this thread or use ThreadPool. For simplicity we create new thread each time.
                // Some problem with TreadPool will be the need to notify transformation thread when user calls new Start() befor previous transformation completed
                thread.Abort();
                thread.Join();
            }
            this.writer.Reset();
            this.scope.Reset();
            this.pipe.Reset();
            this.depth = 0;
            SetUndefinedState(ReadState.Initial);
            if (multiThread) {
                this.thread = new Thread(new ThreadStart(this.StartTransform));
                this.thread.Start();
            } else {
                StartTransform();
            }
        }

        private void StartTransform() {
            try {
                while (true) {
                    XmlReader xmlReader = defaulDocument.source as XmlReader;
                    if (xmlReader != null) {
                        xslCompiledTransform.Transform(xmlReader, args, writer, defaulDocument.resolver);
                        break;
                    }
                    IXPathNavigable nav = defaulDocument.source as IXPathNavigable;
                    if (nav != null) {
                        xslCompiledTransform.Transform(nav, args, writer);
                        break;
                    }
                    string str = defaulDocument.source as string;
                    if (str != null) {
                        using (XmlReader reader = XmlReader.Create(str, ReaderSettings)) {
                            xslCompiledTransform.Transform(reader, args, writer, defaulDocument.resolver);
                        }
                        break;
                    }
                    Stream strm = defaulDocument.source as Stream;
                    if (strm != null) {
                        using (XmlReader reader = XmlReader.Create(strm, ReaderSettings)) {
                            xslCompiledTransform.Transform(reader, args, writer, defaulDocument.resolver);
                        }
                        break;
                    }
                    TextReader txtReader = defaulDocument.source as TextReader;
                    if (txtReader != null) {
                        using (XmlReader reader = XmlReader.Create(txtReader, ReaderSettings)) {
                            xslCompiledTransform.Transform(reader, args, writer, defaulDocument.resolver);
                        }
                        break;
                    }
                    throw new Exception("Unexpected XmlInput");
                }
                writer.Close();
            } catch (Exception e) {
                if (multiThread) {
                    // we need this exception on main thread. So pass it through pipe.
                    pipe.WriteException(e);
                } else {
                    throw;
                }
            }
        }

        /// <summary>
        /// Loaded <see cref="XslCompiledTransform"/> object, which is used
        /// to run XSL transformations. You can reuse XslReader for running
        /// another transformation by replacing <see cref="XslCompiledTransform"/> 
        /// object.
        /// </summary>
        public XslCompiledTransform XslCompiledTransform {
            get { return this.xslCompiledTransform; }
            set { this.xslCompiledTransform = value; }
        }

        /// <summary>
        /// Initial buffer size. The buffer will be
        /// expanded if necessary to be able to store any element start tag with 
        /// all its attributes.
        /// </summary>
        public int InitialBufferSize {
            get { return initialBufferSize; }
            set { initialBufferSize = value; }
        }
        
#region XmlReader Implementation
        int attOffset = 0; // 0 - means reader is positioned on element, when reader potitionrd on the first attribute attOffset == 1
        int attCount;
        int depth;
        XmlNodeType nodeType = XmlNodeType.None;
        ReadState   readState = ReadState.Initial;
        QName       qname;
        string      value;

        void SetUndefinedState(ReadState readState) {
            this.qname = writer.QNameEmpty;
            this.value = string.Empty;
            this.nodeType = XmlNodeType.None;
            this.attCount = 0;
            this.readState = readState;
        }
        bool IsWhitespace(string s) {
            // Because our xml is presumably valid only and all ws chars <= ' '
            foreach (char c in s) {
                if (' ' < c) {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// See <see cref="XmlReader.Read()"/>.
        /// </summary>        
        public override bool Read() {
            // Leave Current node
            switch (nodeType) {
            case XmlNodeType.None :
                if (readState == ReadState.EndOfFile || readState == ReadState.Closed) {
                    return false;
                }
                readState = ReadState.Interactive;
                break;
            case XmlNodeType.Attribute:
                attOffset = 0;
                depth--;
                goto case XmlNodeType.Element;
            case XmlNodeType.Element:
                pipe.FreeTokens(1 + attCount);
                depth++;
                break;
            case XmlNodeType.EndElement :
                scope.PopScope();
                pipe.FreeTokens(1);
                break;
            case XmlNodeType.Text :
                if (attOffset != 0) {
                    // We are on text node inside of the attribute
                    attOffset = 0;
                    depth -= 2;
                    goto case XmlNodeType.Element;
                }
                pipe.FreeTokens(1);
                break;
            case XmlNodeType.ProcessingInstruction :
            case XmlNodeType.Comment:
            case XmlNodeType.SignificantWhitespace:
            case XmlNodeType.Whitespace:
                pipe.FreeTokens(1);
                break;
            default :
                throw new InvalidProgramException("Internal Error: unexpected node type");
            }
            Debug.Assert(attOffset == 0);
            Debug.Assert(readState == ReadState.Interactive);
            attCount = 0;
            // Step on next node
            pipe.Read(out nodeType, out qname, out value);
            if (nodeType == XmlNodeType.None) {
                SetUndefinedState(ReadState.EndOfFile);
                return false;
            }
            
            switch (nodeType) {
            case XmlNodeType.Element:
                for (attCount = 0; true; attCount ++) {
                    XmlNodeType attType;
                    QName attName;
                    string attText;
                    pipe.Read(out attType, out attName, out attText);
                    if (attType != XmlNodeType.Attribute) {
                        break; // We are done with attributes for this element
                    }
                    if (RefEquals(attName.Prefix, "xmlns")  ) { scope.AddNamespace(attName.Local , attText); } 
                    else if (RefEquals(attName, writer.QNameXmlNs   )) { scope.AddNamespace(attName.Prefix, attText); }  // prefix is atomized empty string
                    else if (RefEquals(attName, writer.QNameXmlLang )) { scope.AddLang(attText);                      } 
                    else if (RefEquals(attName, writer.QNameXmlSpace)) { scope.AddSpace(attText);                     }
                }
                scope.PushScope(qname);
                break;
            case XmlNodeType.EndElement :
                qname = scope.Name;
                depth--;
                break;
            case XmlNodeType.Comment:
            case XmlNodeType.ProcessingInstruction :
                break;
            case XmlNodeType.Text:
                if (IsWhitespace(value)) {
                    nodeType = XmlSpace == XmlSpace.Preserve ? XmlNodeType.SignificantWhitespace : XmlNodeType.Whitespace;
                }
                break;
            default :
                throw new InvalidProgramException("Internal Error: unexpected node type");
            }
            return true;
        }

        /// <summary>See <see cref="XmlReader.AttributeCount"/>.</summary>
        public override int AttributeCount { get { return attCount; } }
        // issue: What should be BaseURI in XslReader? xslCompiledTransform.BaseURI ?
        /// <summary>See <see cref="XmlReader.BaseURI"/>.</summary>
        public override string BaseURI { get { return string.Empty; } }
        /// <summary>See <see cref="XmlReader.NameTable"/>.</summary>
        public override XmlNameTable NameTable { get { return nameTable; } }
        /// <summary>See <see cref="XmlReader.Depth"/>.</summary>
        public override int Depth { get { return depth; } }
        /// <summary>See <see cref="XmlReader.EOF"/>.</summary>
        public override bool EOF { get { return ReadState == ReadState.EndOfFile; } }
        /// <summary>See <see cref="XmlReader.HasValue"/>.</summary>
        public override bool HasValue { get { return 0 != (/*HasValueBitmap:*/0x2659C & (1 << (int)nodeType)); } }
        /// <summary>See <see cref="XmlReader.NodeType"/>.</summary>
        public override XmlNodeType NodeType { get { return nodeType; } }
        // issue: We may want return true if element doesn't have content. Iteresting to know what 
        /// <summary>See <see cref="XmlReader.IsEmptyElement"/>.</summary>
        public override bool IsEmptyElement { get { return false; } }
        /// <summary>See <see cref="XmlReader.LocalName"/>.</summary>
        public override string LocalName    { get { return qname.Local; } }
        /// <summary>See <see cref="XmlReader.NamespaceURI"/>.</summary>
        public override string NamespaceURI { get { return qname.NsUri; } }
        /// <summary>See <see cref="XmlReader.Prefix"/>.</summary>
        public override string Prefix       { get { return qname.Prefix; } }
        /// <summary>See <see cref="XmlReader.Value"/>.</summary>
        public override string Value        { get { return value;  } }
        /// <summary>See <see cref="XmlReader.ReadState"/>.</summary>
        public override ReadState ReadState { get { return readState; } }

        /// <summary>See <see cref="XmlReader.Close()"/>.</summary>
        public override void Close() {
            SetUndefinedState(ReadState.Closed);
        }
        /// <summary>See <see cref="XmlReader.GetAttribute(int)"/>.</summary>
        public override string GetAttribute(int i) {
            if (IsInsideElement()) {
                if (0 <= i && i < attCount) {
                    QName attName;
                    string attValue;
                    pipe.GetToken(i + 1, out attName, out attValue);
                    return value;
                }
            }
            throw new ArgumentOutOfRangeException("i");
        }
        static char[] qnameSeparator = new char[] { ':' };
        private int FindAttribute(string name) {
            if (IsInsideElement()) {
                string prefix, local;
                string[] strings = name.Split(qnameSeparator, StringSplitOptions.None);
                switch(strings.Length) {
                case 1:
                    prefix = string.Empty;
                    local = name;
                    break;
                case 2:
                    if (strings[0].Length == 0) {
                        return 0; // ":local-name"
                    }
                    prefix = strings[0];
                    local  = strings[1];
                    break;
                default :
                    return 0;
                }
                for (int i = 1; i <= attCount; i++) {
                    QName attName;
                    string attValue;
                    pipe.GetToken(i, out attName, out attValue);
                    if (attName.Local == local && attName.Prefix == prefix) {
                        return i;
                    }
                }
            }
            return 0;
        }
        /// <summary>See <see cref="XmlReader.GetAttribute(string)"/>.</summary>
        public override string GetAttribute(string name) {
            int attNum = FindAttribute(name);
            if (attNum != 0) {
                return GetAttribute(attNum - 1);
            }
            return null;
        }
        /// <summary>See <see cref="XmlReader.GetAttribute(string, string)"/>.</summary>
        public override string GetAttribute(string name, string ns) {
            if (IsInsideElement()) {
                for (int i = 1; i <= attCount; i++) {
                    QName attName;
                    string attValue;
                    pipe.GetToken(i, out attName, out attValue);
                    if (attName.Local == name && attName.NsUri == ns) {
                        return attValue;
                    }
                }
            }
            return null;
        }
        /// <summary>See <see cref="XmlReader.LookupNamespace(string)"/>.</summary>
        public override string LookupNamespace(string prefix) { return scope.LookupNamespace(prefix); }
        /// <summary>See <see cref="XmlReader.Close()"/>.</summary>
        public override bool MoveToAttribute(string name) {
            int attNum = FindAttribute(name);
            if (attNum != 0) {
                MoveToAttribute(attNum - 1);
                return true;
            }
            return false;
        }
        /// <summary>See <see cref="XmlReader.MoveToAttribute(int)"/>.</summary>
        public override void MoveToAttribute(int i) {
            if (IsInsideElement()) {
                if (0 <= i && i < attCount) {
                    ChangeDepthToElement();
                    attOffset = i + 1;
                    depth++;
                    pipe.GetToken(attOffset, out qname, out value);
                    nodeType = XmlNodeType.Attribute;
                }
            }
            throw new ArgumentOutOfRangeException("i");
        }
        /// <summary>See <see cref="XmlReader.MoveToAttribute(string, string)"/>.</summary>
        public override bool MoveToAttribute(string name, string ns) {
            if (IsInsideElement()) {
                for (int i = 1; i <= attCount; i ++) {
                    QName attName;
                    string attValue;
                    pipe.GetToken(i , out attName, out attValue);
                    if (attName.Local == name && attName.NsUri == ns) {
                        ChangeDepthToElement();
                        nodeType = XmlNodeType.Attribute;
                        attOffset = i;
                        qname = attName;
                        depth++;
                        value = attValue;
                    }
                }
            }
            return false;
        }
        private bool IsInsideElement() {
            return (
                nodeType == XmlNodeType.Element ||
                nodeType == XmlNodeType.Attribute ||
                nodeType == XmlNodeType.Text && attOffset != 0
            );
        }
        private void ChangeDepthToElement() {
            switch (nodeType) {
            case XmlNodeType.Attribute :
                depth--;
                break;
            case XmlNodeType.Text :
                if (attOffset != 0) {
                    depth -= 2;
                }
                break;
            }
        }
        /// <summary>See <see cref="XmlReader.MoveToElement()"/>.</summary>
        public override bool MoveToElement() {
            if (
                nodeType == XmlNodeType.Attribute ||
                nodeType == XmlNodeType.Text && attOffset != 0
            ) {
                ChangeDepthToElement();
                nodeType = XmlNodeType.Element;
                attOffset = 0;
                pipe.GetToken(0, out qname, out value);
                return true;
            }
            return false;
        }
        /// <summary>See <see cref="XmlReader.MoveToFirstAttribute()"/>.</summary>
        public override bool MoveToFirstAttribute() {
            ChangeDepthToElement();
            attOffset = 0;
            return MoveToNextAttribute();
        }
        /// <summary>See <see cref="XmlReader.MoveToNextAttribute()"/>.</summary>
        public override bool MoveToNextAttribute() {
            if (attOffset < attCount) {
                ChangeDepthToElement();
                depth++;
                attOffset++;
                pipe.GetToken(attOffset, out qname, out value);
                nodeType = XmlNodeType.Attribute;
                return true;
            }
            return false;
        }
        /// <summary>See <see cref="XmlReader.ReadAttributeValue()"/>.</summary>
        public override bool ReadAttributeValue() { 
            if (nodeType == XmlNodeType.Attribute) {
                nodeType = XmlNodeType.Text;
                depth++;
                return true;
            }
            return false;
        }
        /// <summary>See <see cref="XmlReader.ResolveEntity()"/>.</summary>
        public override void ResolveEntity() { throw new InvalidOperationException(); }

        /// <summary>See <see cref="XmlReader.XmlLang"/>.</summary>
        public override string XmlLang { get { return scope.Lang; } }
        /// <summary>See <see cref="XmlReader.XmlSpace"/>.</summary>
        public override XmlSpace XmlSpace { get { return scope.Space; } }
#endregion  // XmlReader Implementation

#region  ------------------------------- Supporting classes ------------------------------
        private static bool RefEquals(string strA, string strB) {
            Debug.Assert(
                ((object)strA == (object)strB) || !String.Equals(strA, strB),
                "String atomization Failure: '" + strA + "'"
            );
            return (object)strA == (object)strB;
        }

        private static bool RefEquals(QName qnA, QName qnB) {
            Debug.Assert(
                ((object)qnA == (object)qnB) || qnA.Local != qnB.Local || qnA.NsUri != qnB.NsUri || qnA.Prefix != qnB.Prefix,
                "QName atomization Failure: '" + qnA.ToString() + "'"
            );
            return (object)qnA == (object)qnB;
        }

        // QName is imutable. 
        private class QName {
            string local;
            string nsUri;
            string prefix;
            public QName(string local, string nsUri, string prefix) {
                this.local  = local ;
                this.nsUri  = nsUri ;
                this.prefix = prefix;
            }
            public string Local  { get { return this.local ; } }
            public string NsUri  { get { return this.nsUri ; } }
            public string Prefix { get { return this.prefix; } }

            public override string ToString() {
                return (Prefix != null && Prefix.Length != 0) ? (Prefix + ':' + Local) : Local;
            }
        }

        // BufferWriter records information written to it in sequence of WriterEvents: 
        [DebuggerDisplay("{NodeType}: name={Name}, Value={Value}")]
        private struct XmlToken {
            public XmlNodeType NodeType;
            public QName       Name    ;
            public string      Value   ;

            // it seams that it faster to set fields of structure in one call.
            // This trick is workaround of the C# limitation of declaring variable as ref to a struct.
            public static void Set(ref XmlToken evnt, XmlNodeType nodeType, QName name, string value) {
                evnt.NodeType = nodeType;
                evnt.Name     = name    ;
                evnt.Value    = value   ;
            }
            public static void Get(ref XmlToken evnt, out XmlNodeType nodeType, out QName name, out string value) {
                nodeType = evnt.NodeType;
                name     = evnt.Name    ;
                value    = evnt.Value   ;
            }
        }

        private class BufferWriter : XmlWriter {
            QNameTable    qnameTable;
            TokenPipe pipe;
            string firstText;
            StringBuilder sbuilder;
            QName curAttribute;

            public QName         QNameXmlSpace;
            public QName         QNameXmlLang;
            public QName         QNameEmpty;
            public QName         QNameXmlNs;

            public BufferWriter(TokenPipe pipe, XmlNameTable nameTable) {
                this.pipe = pipe;
                this.qnameTable = new QNameTable(nameTable);
                this.sbuilder = new StringBuilder();
                QNameXmlSpace = qnameTable.GetQName("space", NsXml  , "xml"  ); // xml:space
                QNameXmlLang  = qnameTable.GetQName("lang" , NsXml  , "xml"  ); // xml:lang
                QNameXmlNs    = qnameTable.GetQName("xmlns", NsXmlNs, ""     ); // xmlsn=""
                QNameEmpty    = qnameTable.GetQName(""     , ""     , ""     );
            }

            public void Reset() {
                this.firstText = null;
                this.sbuilder.Length = 0;
            }

            private void AppendText(string text) {
                if (firstText == null) {
                    Debug.Assert(sbuilder.Length == 0);
                    firstText = text;
                } else if (sbuilder.Length == 0) {
                    sbuilder.Append(firstText);
                }
                sbuilder.Append(text);
            }
            private string MergeText() {
                if (firstText == null) {
                    return string.Empty; // There was no text ouptuted
                }
                if (sbuilder.Length != 0) {
                    // merge content of sbuilder into firstText
                    Debug.Assert(firstText != null);
                    firstText = sbuilder.ToString();
                    sbuilder.Length = 0;
                }
                string result = firstText;
                firstText = null;
                return result;
            }

            private void FinishTextNode() {
                string text = MergeText();
                if (text.Length != 0) {
                    pipe.Write(XmlNodeType.Text, QNameEmpty, text);
                }
            }

           public override void WriteComment(string text) {
                FinishTextNode();
                pipe.Write(XmlNodeType.Comment, QNameEmpty, text);
            }

            public override void WriteProcessingInstruction(string name, string text) {
                FinishTextNode();
                pipe.Write(XmlNodeType.ProcessingInstruction, qnameTable.GetQName(name, string.Empty, string.Empty), text);
            }

            public override void WriteStartElement(string prefix, string name, string ns) {
                FinishTextNode();
                pipe.Write(XmlNodeType.Element, qnameTable.GetQName(name, ns, prefix), "");
            }
            public override void WriteEndElement() {
                FinishTextNode();
                pipe.Write(XmlNodeType.EndElement, QNameEmpty, "");
            }
            public override void WriteStartAttribute(string prefix, string name, string ns) {
                curAttribute = qnameTable.GetQName(name, ns, prefix);
            }
            public override void WriteEndAttribute(){
                pipe.Write(XmlNodeType.Attribute, curAttribute, MergeText());
            }
            public override void WriteString(string text) {
                AppendText(text);
            }

            public override void WriteFullEndElement() {
                WriteEndElement();
            }
            public override void WriteRaw(string data) {
                WriteString(data); // In XslReader output we ignore disable-output-escaping
            }

            public override void Close() {
                FinishTextNode();
                pipe.Close();
            }
            public override void Flush() { }

            // XsltCompiledTransform never calls these methods and properties:
            public override void WriteStartDocument() { throw new NotSupportedException(); }
            public override void WriteStartDocument(bool standalone) { throw new NotSupportedException(); }
            public override void WriteEndDocument() { throw new NotSupportedException(); }
            public override void WriteDocType(string name, string pubid, string sysid, string subset) { throw new NotSupportedException(); }
            public override void WriteEntityRef(string name) { throw new NotSupportedException(); }
            public override void WriteCharEntity(char ch) { throw new NotSupportedException(); }
            public override void WriteSurrogateCharEntity(char lowChar, char highChar) { throw new NotSupportedException(); }
            public override void WriteWhitespace(string ws) { throw new NotSupportedException(); }
            public override void WriteChars(char[] buffer, int index, int count) { throw new NotSupportedException(); }
            public override void WriteRaw(char[] buffer, int index, int count) { throw new NotSupportedException(); }
            public override void WriteBase64(byte[] buffer, int index, int count) { throw new NotSupportedException(); }
            public override void WriteCData(string text) { throw new NotSupportedException(); }
            public override string LookupPrefix(string ns) { throw new NotSupportedException(); }
            public override WriteState WriteState { get { throw new NotSupportedException(); } }
            public override XmlSpace XmlSpace { get { throw new NotSupportedException(); } }
            public override string XmlLang { get { throw new NotSupportedException(); } }

            private class QNameTable {
                // This class atomizes QNames.
                XmlNameTable nameTable;
                Dictionary<string, List<QName>> qnames   = new Dictionary<string, List<QName>>();
                
                public QNameTable(XmlNameTable nameTable) {
                    this.nameTable = nameTable;
                }

                public QName GetQName(string local, string nsUri, string prefix) {
                    nsUri  = nameTable.Add(nsUri );
                    prefix = nameTable.Add(prefix);
                    List<QName> list;
                    if (! qnames.TryGetValue(local, out list)) {
                        list = new List<QName>();
                        qnames.Add(local, list);
                    } else {
                        foreach(QName qn in list) {
                            Debug.Assert(qn.Local == local, "Atomization Failure: '" + local + "'");
                            if (RefEquals(qn.Prefix, prefix) && RefEquals(qn.NsUri, nsUri)) {
                                return qn;
                            }
                        }
                    }
                    QName qname = new QName(nameTable.Add(local), nsUri, prefix);
                    list.Add(qname);
                    return qname;
                }

                private static string Atomize(string s, Dictionary<string, string> dic) {
                    string atom;
                    if (dic.TryGetValue(s, out atom)) {
                        return atom;
                    } else {
                        dic.Add(s, s);
                        return s;
                    }
                }
            }
        }

        private class ScopeManager {
            // We need the scope for the following reasons:
            // 1. Report QName on EndElement  (local, nsUri, prefix )
            // 2. Keep scope of Namespaces    (null , nsUri, prefix )
            // 3. Keep scope of xml:lang      (null , lang , "lang" )
            // 4. Keep scope of xml:space     (null , space, "space")
            // On each StartElement we adding record(s) to the scope, 
            // Its convinient to add QName last becuase in this case it will be directly available for EndElement
            static string atomLang      = new String("lang" .ToCharArray());
            static string atomSpace     = new String("space".ToCharArray());
            XmlNameTable nameTable;
            string       stringEmpty;
            QName[] records = new QName[32];
            int lastRecord;
            XmlSpace currentSpace;
            string   currentLang;

            public ScopeManager(XmlNameTable nameTable) {
                this.nameTable = nameTable;
                this.stringEmpty = nameTable.Add(string.Empty);
                this.currentLang  = this.stringEmpty;
                this.currentSpace = XmlSpace.None;
                Reset();
            }

            public void Reset() {
                lastRecord = 0;
                records[lastRecord++] = new QName(null       , nameTable.Add(NsXml), nameTable.Add("xml"));  // xmlns:xml="http://www.w3.org/XML/1998/namespace"
                records[lastRecord++] = new QName(null       , stringEmpty, stringEmpty);                    // xml=""
                records[lastRecord++] = new QName(stringEmpty, stringEmpty, stringEmpty);                    // --  lookup barier
            }

            public void PushScope(QName qname) {
                Debug.Assert(qname.Local != null, "Scope is Element Name");
                AddRecord(qname);
            }

            public void PopScope() {
                Debug.Assert(records[lastRecord - 1].Local != null, "LastRecord in each scope is expected to be ElementName");
                do {
                    lastRecord--;
                    Debug.Assert(0 < lastRecord, "Push/Pop balance error");
                    QName record = records[lastRecord-1];
                    if (record.Local != null) {
                        break; //  this record is Element QName
                    }
                    if (RefEquals(record.Prefix, atomLang)) {
                        currentLang = record.NsUri;
                    } else if (RefEquals(record.Prefix, atomSpace)) {
                        currentSpace = Str2Space(record.NsUri);
                    }
                } while (true);
            }

            private void AddRecord(QName qname) {
                if (lastRecord == records.Length) {
                    QName[] temp = new QName[records.Length * 2];
                    records.CopyTo(temp, 0);
                    records = temp;
                }
                records[lastRecord++] = qname;
            }
            
            public void AddNamespace(string prefix, string uri) {
                Debug.Assert(prefix != null);
                Debug.Assert(uri    != null);
                Debug.Assert(prefix == nameTable.Add(prefix), "prefixes are expected to be already atomized in this NameTable");
                uri    = nameTable.Add(uri   );
                Debug.Assert(
                    ! RefEquals(prefix, atomLang ) &&
                    ! RefEquals(prefix, atomSpace)
                ,
                    "This assumption is important to distinct NsDecl from xml:space and xml:lang"
                );
                AddRecord(new QName(null, uri, prefix));
            }

            public void AddLang(string lang) {
                Debug.Assert(lang != null);
                lang = nameTable.Add(lang);
                if (RefEquals(lang, currentLang)) {
                    return;
                }
                AddRecord(new QName(null, currentLang, atomLang));
                currentLang = lang;
            }

            public void AddSpace(string space) {
                Debug.Assert(space != null);
                XmlSpace xmlSpace = Str2Space(space);
                if (xmlSpace == XmlSpace.None) {
                    throw new Exception("Unexpected value for xml:space attribute");
                }
                if (xmlSpace == currentSpace) {
                    return;
                }
                AddRecord(new QName(null, Space2Str(currentSpace), atomSpace));
                currentSpace = xmlSpace;
            }
            private string Space2Str(XmlSpace space) {
                switch(space) {
                case XmlSpace.Preserve : return "preserve";
                case XmlSpace.Default  : return "default";
                default                : return "none";
                }
            }
            private XmlSpace Str2Space(string space) {
                switch(space) {
                case "preserve" : return XmlSpace.Preserve;
                case "default"  : return XmlSpace.Default;
                default         : return XmlSpace.None;
                }
            }

            public string LookupNamespace(string prefix) {
                Debug.Assert(prefix != null);
                prefix = nameTable.Get(prefix);
                for (int i = lastRecord - 2; 0 <= i; i -- ) {
                    QName record = records[i];
                    if (record.Local == null && RefEquals(record.Prefix, prefix)) {
                        return record.NsUri;
                    }
                }
                return null;
            }
            public string   Lang  { get { return currentLang; } }
            public XmlSpace Space { get { return currentSpace; } }
            public QName    Name  { get {
                Debug.Assert(records[lastRecord-1].Local != null, "Element Name is expected");
                return records[lastRecord - 1]; 
            } }
        }

        private class TokenPipe {
            protected XmlToken[] buffer;
            protected int writePos;                // position after last wrote token
            protected int readStartPos;            // 
            protected int readEndPos;              // 
            protected int mask;                    // used in TokenPipeMultiThread

            public TokenPipe(int bufferSize) {
                /*BuildMask*/ {
                    if (bufferSize < 2) {
                        bufferSize = defaultBufferSize;
                    }
                    // To make or round buffer work bufferSize should be == 2 power N and mask == bufferSize - 1
                    bufferSize--;
                    mask = bufferSize;
                    while ((bufferSize = bufferSize >> 1) != 0) {
                        mask |= bufferSize;
                    }
                }
                this.buffer = new XmlToken[mask + 1];
            }

            public virtual void Reset() {
                readStartPos = readEndPos = writePos = 0;
            }

            public virtual void Write(XmlNodeType nodeType, QName name, string value) {
                Debug.Assert(writePos <= buffer.Length);
                if (writePos == buffer.Length) {
                    XmlToken[] temp = new XmlToken[buffer.Length * 2];
                    buffer.CopyTo(temp, 0);
                    buffer = temp;
                }
                Debug.Assert(writePos < buffer.Length);
                XmlToken.Set(ref buffer[writePos], nodeType, name, value);
                writePos++;
            }

            public virtual void WriteException(Exception e) {
                throw e;
            }

            public virtual void Read(out XmlNodeType nodeType, out QName name, out string value) {
                Debug.Assert(readEndPos < buffer.Length);
                XmlToken.Get(ref buffer[readEndPos], out nodeType, out name, out value);
                readEndPos++;
            }

            public virtual void FreeTokens(int num) {
                readStartPos += num;
                readEndPos = readStartPos;
            }

            public virtual void Close() {
                Write(XmlNodeType.None, null, null);
            }

            public virtual void GetToken(int attNum, out QName name, out string value) {
                Debug.Assert(0 <= attNum && attNum < readEndPos - readStartPos - 1);
                XmlNodeType nodeType;
                XmlToken.Get(ref buffer[readStartPos + attNum], out nodeType, out name, out value);
                Debug.Assert(nodeType == (attNum == 0 ? XmlNodeType.Element : XmlNodeType.Attribute), "We use GetToken() only to access parts of start element tag.");
            }
        }

        private class TokenPipeMultiThread : TokenPipe {
            Exception exception;

            public TokenPipeMultiThread(int bufferSize) : base(bufferSize) {}

            public override void Reset() {
                base.Reset();
                exception = null;
            }

            private void ExpandBuffer() {
                // Buffer is too smal for this amount of attributes.
                Debug.Assert(writePos == readStartPos + buffer.Length, "no space to write next token");
                Debug.Assert(writePos == readEndPos, "all tokens ware read");
                int newMask = (mask << 1) | 1;
                XmlToken[] newBuffer = new XmlToken[newMask + 1];
                for (int i = readStartPos; i < writePos; i ++) {
                    newBuffer[i & newMask] = buffer[i & mask];
                }
                buffer = newBuffer;
                mask   = newMask;
                Debug.Assert(writePos < readStartPos + buffer.Length, "we should have now space to next write token");
            }

            public override void Write(XmlNodeType nodeType, QName name, string value) {
                lock (this) {
                    Debug.Assert(readEndPos <= writePos && writePos <= readStartPos + buffer.Length);
                    if (writePos == readStartPos + buffer.Length) {
                        if (writePos == readEndPos) {
                            ExpandBuffer();
                        } else {
                            Monitor.Wait(this);
                        }
                    }

                    Debug.Assert(writePos < readStartPos + buffer.Length);
                    XmlToken.Set(ref buffer[writePos & mask], nodeType, name, value);

                    writePos++;
                    if (readStartPos + buffer.Length <= writePos) {
                        // This "if" is some heuristics, it may wrk or may not:
                        // To minimize task switching we wakeup reader ony if we wrote enouph tokens.
                        // So if reader already waits, let it sleep before we fill up the buffer. 
                        Monitor.Pulse(this);
                    }
                }
            }

            public override void WriteException(Exception e) {
                lock (this) {
                    exception = e;
                    Monitor.Pulse(this);
                }
            }

            public override void Read(out XmlNodeType nodeType, out QName name, out string value) {
                lock (this) {
                    Debug.Assert(readEndPos <= writePos && writePos <= readStartPos + buffer.Length);
                    if (readEndPos == writePos) {
                        if (readEndPos == readStartPos + buffer.Length) {
                            ExpandBuffer();
                            Monitor.Pulse(this);
                        }
                        Monitor.Wait(this);
                    }
                    if (exception != null) {
                        throw new XsltException("Exception happened during transformation. See inner exception for details:\n", exception);
                    }
                }
                Debug.Assert(readEndPos < writePos);
                XmlToken.Get(ref buffer[readEndPos & mask], out nodeType, out name, out value);
                readEndPos++;
            }

            public override void FreeTokens(int num) {
                lock (this) {
                    readStartPos += num;
                    readEndPos = readStartPos;
                    Monitor.Pulse(this);
                }
            }

            public override void Close() {
                Write(XmlNodeType.None, null, null);
                lock (this) {
                    Monitor.Pulse(this);
                }
            }

            public override void GetToken(int attNum, out QName name, out string value) {
                Debug.Assert(0 <= attNum && attNum < readEndPos - readStartPos - 1);
                XmlNodeType nodeType;
                XmlToken.Get(ref buffer[(readStartPos + attNum) & mask], out nodeType, out name, out value);
                Debug.Assert(nodeType == (attNum == 0 ? XmlNodeType.Element : XmlNodeType.Attribute), "We use GetToken() only to access parts of start element tag.");
            }
        }
#endregion  ------------------------------- Supporting classes ------------------------------
    }
}
