using System;
using System.Xml;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using Mvp.Xml.XInclude;
using System.IO;

namespace XmlLab.nxslt
{
    internal static class Utils
    {
        /// <summary>
        /// Pretty prints XML document using XmlWriter's formatting functionality.
        /// </summary>
        /// <param name="reader">Source XML reader</param>
        /// <param name="options">Parsed command line options</param>
        public static void PrettyPrint(XmlReader reader, NXsltOptions options)
        {
            XmlWriterSettings writerSettings = new XmlWriterSettings();            
            writerSettings.Indent = true;
            writerSettings.NewLineOnAttributes = true;
            writerSettings.Encoding = new UTF8Encoding(false);            
            XmlWriter writer;
            if (options.OutFile != null)
            {
                //Pretty print to a file                
                writer = XmlWriter.Create(options.OutFile, writerSettings);
            }
            else
            {
                //Pretty print to the console                                
                writer = XmlWriter.Create(Console.Out, writerSettings);
            }
            while (reader.ReadState != ReadState.EndOfFile)
            {               
                writer.WriteNode(reader, false);
            }
            writer.Close();
            reader.Close();
        }

        /// <summary>
        /// Gets XmlResolver - default or custom, with user credentials or not.
        /// </summary>
        /// <param name="credentials">User credentials</param>    
        /// <param name="options">Parsed command line options</param>
        public static XmlResolver GetXmlResolver(NetworkCredential credentials, NXsltOptions options)
        {
            XmlResolver resolver;
            Type resolverType;
            if (options.ResolverTypeName != null)
            {
                //Custom resolver
                try
                {
                    resolverType = TypeUtils.FindType(options, options.ResolverTypeName);
                }
                catch (Exception e)
                {
                    throw new NXsltException(NXsltStrings.ErrorCreateResolver, options.ResolverTypeName, e.Message);
                }
                if (!typeof(XmlResolver).IsAssignableFrom(resolverType))
                {
                    //Type is not XmlResolver
                    throw new NXsltException(NXsltStrings.ErrorTypeNotXmlResolver, options.ResolverTypeName);
                }
                try
                {
                    resolver = (XmlResolver)Activator.CreateInstance(resolverType);
                }
                catch (Exception e)
                {
                    throw new NXsltException(NXsltStrings.ErrorCreateResolver, options.ResolverTypeName, e.Message);
                }
            }
            else
            {
                //Standard resolver
                resolver = new XmlUrlResolver();
            }
            //Set credentials if any
            if (credentials != null)
            {
                resolver.Credentials = credentials;
            }
            return resolver;
        }

        public static string ExtractStylsheetHrefFromPI(XPathNavigator pi)
        {
            Regex r = new Regex(@"href[ \n\t\r]*=[ \n\t\r]*""(.*)""|href[ \n\t\r]*=[ \n\t\r]*'(.*)'");
            Match m = r.Match(pi.Value);
            if (!m.Success)
            {
                //Absent href preudo attribute                
                throw new NXsltException(NXsltStrings.ErrorInvalidPI);
            }
            //Found href pseudo attribute value
            string href = m.Groups[1].Success ? m.Groups[1].Value :
              m.Groups[2].Value;
            return href;
        }

        public static XmlReader CreateReader(string filename, XmlReaderSettings settings, NXsltOptions options)
        {
            if (options.ProcessXInclude)
            {
                return XmlReader.Create(new XIncludingReader(filename), settings);
            }
            else
            {
                return XmlReader.Create(filename, settings);
            }
        }

        public static XmlReader CreateReader(Stream stream, XmlReaderSettings settings, NXsltOptions options)
        {
            if (options.ProcessXInclude)
            {
                return XmlReader.Create(new XIncludingReader(stream), settings);
            }
            else
            {
                return XmlReader.Create(stream, settings);
            }
        }
    }
}
