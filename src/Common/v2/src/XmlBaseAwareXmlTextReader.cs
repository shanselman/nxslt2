#region using

using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;

#endregion

namespace Mvp.Xml.Common
{
    /// <summary>
    /// Extended <see cref="XmlTextReader"/> supporting <a href="http://www.w3.org/TR/xmlbase/">XML Base</a>.
    /// </summary>
    /// <remarks>
    /// <para>Author: Oleg Tkachenko, <a href="http://www.xmllab.net">http://www.xmllab.net</a>.</para>
    /// </remarks>
    public class XmlBaseAwareXmlTextReader : XmlTextReader
    {
        #region private

        private XmlBaseState _state = new XmlBaseState();
        private Stack<XmlBaseState> _states = null;

        #endregion

        #region constructors

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given URI.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(string uri)
            : base(uri)
        {
            _state.BaseUri = new Uri(base.BaseURI);
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given URI and 
        /// name table.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(string uri, XmlNameTable nt)
            : base(uri, nt)
        {
            _state.BaseUri = new Uri(base.BaseURI);
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given TextReader.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(TextReader reader)
            : base(reader) 
        {
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given uri and 
        /// TextReader.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(string uri, TextReader reader)
            : base(uri, reader)
        {
            _state.BaseUri = new Uri(base.BaseURI);
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given TextReader 
        /// and name table.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(TextReader reader, XmlNameTable nt)
            : base(reader, nt) 
        {
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given uri, name table
        /// and TextReader.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(string uri, TextReader reader, XmlNameTable nt)
            : base(uri, reader, nt)
        {
            _state.BaseUri = new Uri(base.BaseURI);
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given stream.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(Stream stream)
            : base(stream) 
        {
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given uri and stream.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(string uri, Stream stream)
            : base(uri, stream)
        {
            _state.BaseUri = new Uri(base.BaseURI);
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given stream 
        /// and name table.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(Stream stream, XmlNameTable nt)
            : base(stream, nt) 
        {
            
        }

        /// <summary>
        /// Creates XmlBaseAwareXmlTextReader instance for given stream,
        /// uri and name table.
        /// </summary>        
        public XmlBaseAwareXmlTextReader(string uri, Stream stream, XmlNameTable nt)
            : base(uri, stream, nt)
        {
            _state.BaseUri = new Uri(base.BaseURI);
            
        }

        #endregion

        #region XmlTextReader overrides

        /// <summary>
        /// See <see cref="XmlTextReader.BaseURI"/>.
        /// </summary>
        public override string BaseURI
        {
            get
            {
                return _state.BaseUri == null ? "" : _state.BaseUri.AbsoluteUri;
            }
        }

        /// <summary>
        /// See <see cref="XmlTextReader.Read"/>.
        /// </summary>
        public override bool Read()
        {
            bool baseRead = base.Read();
            if (baseRead)
            {
                if (base.NodeType == XmlNodeType.Element &&
                    base.HasAttributes)
                {
                    string baseAttr = GetAttribute("xml:base");
                    if (baseAttr == null)
                        return baseRead;
                    Uri newBaseUri = null;
                    if (_state.BaseUri == null)
                        newBaseUri = new Uri(baseAttr);
                    else
                        newBaseUri = new Uri(_state.BaseUri, baseAttr);
                    if (_states == null)
                        _states = new Stack<XmlBaseState>();
                    //Push current state and allocate new one
                    _states.Push(_state);
                    _state = new XmlBaseState(newBaseUri, base.Depth);
                }
                else if (base.NodeType == XmlNodeType.EndElement)
                {
                    if (base.Depth == _state.Depth && _states != null && _states.Count > 0)
                    {
                        //Pop previous state
                        _state = _states.Pop();
                    }
                }
            }
            return baseRead;
        }

        #endregion
    }

    internal class XmlBaseState
    {
        public XmlBaseState() { }

        public XmlBaseState(Uri baseUri, int depth)
        {
            this.BaseUri = baseUri;
            this.Depth = depth;
        }

        public Uri BaseUri;
        public int Depth;
    }
}
