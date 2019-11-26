using System;
using System.Resources;
using System.Xml;
using System.Collections.Generic;
using System.Xml.Xsl;
using System.Net;
using System.Text.RegularExpressions;

namespace XmlLab.nxslt
{
  ///	<summary>
  ///	Command	line arguments parser.
  ///	</summary>
  internal class NXsltArgumentsParser
  {
    private XmlNamespaceManager namespaceManager = null;
    private Queue<string> paramQueue = null;
    private NameTable nameTable = null;

    ///	<summary>
    ///	Parses command line	arguments into NXsltOptions collection.
    ///	</summary>
    ///	<param name="args">Command line	arguments.</param>        
    ///	<returns>Parsed	collection of options.</returns>
    public NXsltOptions ParseArguments(string[] args)
    {
      NXsltOptions options = new NXsltOptions();
      for (int i = 0; i < args.Length; i++)
      {
        string arg = args[i];
        switch (arg)
        {
          //Show help
          case "-?":
            options.ShowHelp = true;
            break;
          //Strip whitespace
          case "-xw":
            options.StripWhiteSpace = true;
            break;
          //Output filename
          case "-o":
            if (i == args.Length - 1)
            {
              //Absent file name
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingOutFileName);
            }
            else
            {
              //Next argument must be filename
              options.OutFile = args[++i];
              break;
            }          
          //Show timings
          case "-t":
            options.ShowTiming = true;
            break;
          //No source XML
          case "-xs":
            if (options.GetStylesheetFromPI)
            {
              //Both -xs and -pi cannot be specified
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBothNoSourceAndPI);
            }
            else if ((options.Source != null && options.Stylesheet != null) ||
                    options.LoadSourceFromStdin)
            {
              //When using -xs no source shoold be specified
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBothNoSourceAndSource);
            }
            else if (options.Source != null && options.Stylesheet == null)
            {
              //That was stylesheet, not source
              options.Stylesheet = options.Source;
              options.Source = null;
            }
            options.NoSourceXml = true;
            break;
          //Get stylesheet URI from xml-stylesheet PI
          case "-pi":
            if (options.Stylesheet != null || options.LoadStylesheetFromStdin)
            {
              //Both -pi and stylesheet cannot be specified
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBothStylesheetAndPI);
            }
            else if (options.NoSourceXml)
            {
              //Both -xs and -pi cannot be specified
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBothNoSourceAndPI);

            }
            else
              options.GetStylesheetFromPI = true;
            break;
          //Network credentials for source XML
          case "-xmlc":
            if (i == args.Length - 1)
            {
              //Absent credentials
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingXMLCredentials);
            }
            else
            {
              //Next argument must be credentials              
              string credstring = args[++i];
              if (options.SourceCredential != null)
              {
                //Duplicate credentials
                throw new NXsltCommandLineParsingException(NXsltStrings.ErrorDuplicateCredentials);
              }
              options.SourceCredential = ParseCredentials(credstring);
              break;
            }
          //Network credentials for stylesheet
          case "-xslc":
            if (i == args.Length - 1)
            {
              //Absent credentials
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingXSLTCredentials);
            }
            else
            {
              //Next argument must be credentials              
              string credstring = args[++i];
              if (options.XSLTCredential != null)
              {
                //Duplicate credentials
                throw new NXsltCommandLineParsingException(NXsltStrings.ErrorDuplicateCredentials);
              }
              options.XSLTCredential = ParseCredentials(credstring);
              break;
            }
          //Use named URI resolver type
          case "-r":
            if (i == args.Length - 1)
            {
              //Absent resolver type name
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingResolverTypeName);
            }
            else
            {
              //Next argument must be resolver type
              options.ResolverTypeName = args[++i];
              break;
            }
          //Assembly file name
          case "-af":
            if (options.AssemblyName != null)
            {
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBothAssemblyFileNameAndName);
            }
            if (i == args.Length - 1)
            {
              //Absent assembly file name
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingAssemblyFileName);
            }
            else
            {
              //Next argument must be assembly file name
              options.AssemblyFileName = args[++i];
              break;
            }
          //Assembly name
          case "-an":
            if (options.AssemblyFileName != null)
            {
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBothAssemblyFileNameAndName);
            }
            if (i == args.Length - 1)
            {
              //Absent assembly name
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingAssemblyName);
            }
            else
            {
              //Next argument must be assembly name
              options.AssemblyName = args[++i];
              break;
            }
          //Allow multiple output documents
          case "-mo":
            options.MultiOutput = true;
            break;
          //Validate documents
          case "-v":
            options.ValidateDocs = true;
            break;
          //Do not resolve externals
          case "-xe":
            options.ResolveExternals = false;
            break;
          //Do not process XInclude
          case "-xi":
            options.ProcessXInclude = false;
            break;
          //Pretty print source XML
          case "-pp":
            options.PrettyPrintMode = true;
            break;
          //Extension class names
          case "-ext":
            if (i == args.Length - 1)
            {
              //Absent class names
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingExtClassNames);
            }
            else
            {
              //Next argument must be ext class names list
              options.ExtClasses = args[++i].Split(',');
              break;
            }
          //Source or Stylesheet to be read from stdin
          case "-":
            if (options.Source == null && !options.LoadSourceFromStdin)
            {
              options.LoadSourceFromStdin = true;
            }
            else if (options.Stylesheet == null && !options.LoadStylesheetFromStdin)
            {
              //Check out that both source and stylesheet are not "-"
              if (options.LoadSourceFromStdin)
              {
                //Both source and stylesheet cannot be read from stdin
                throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBothStdin);
              }
              else
                options.LoadStylesheetFromStdin = true;
            }
            else
            {
              //Unrecognized "-"
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorUnrecognizedOption, arg);
            }
            break;
          //Other argment - source URI, stylesheet URI, parameter or namespace declaration
          default:
            //namespace declaration?
            if ((arg.StartsWith("xmlns:") || arg.StartsWith("xmlns=")) && arg.Contains("="))
            {
              ParseNamespaceDeclaration(arg);
            }
            //Parameter?            
            else if (arg.Contains("="))
            {
              //Parameter - put to the queue till all namespaces are not processed
              if (arg.StartsWith("="))
              {
                throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingName);
              }
              else if (arg.EndsWith("="))
              {
                throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingValue, arg.Remove(arg.Length - 1, 1));
              }
              //Enqueue param till all namespace declarations parsed
              EnqueueParameter(args, arg);
            }
            else if (arg.StartsWith("-"))
            {
              //Unrecognized option
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorUnrecognizedOption, arg);
            }
            //Source URI?
            else if (options.Source == null && !options.LoadSourceFromStdin && !options.NoSourceXml)
            {
              options.Source = arg;
            }
            //Stylesheet URI?
            else if (options.Stylesheet == null && !options.LoadStylesheetFromStdin && !options.GetStylesheetFromPI)
            {
              options.Stylesheet = arg;
            }
            //Unrecognized argument
            else
            {
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorUnrecognizedOption, arg);
            }
            break;
        }
      }
      //Resolve parameters
      ResolveParameters(options);
      //Instantiate specified extension objects
      ParseExtObjects(options);
      return options;
    }

    /// <summary>
    /// Saves param in a queue.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <param name="param">Parameter</param>
    /// <param name="paramQueue">Queue of paramerers</param>
    private void EnqueueParameter(string[] args, string param)
    {
      //Lazy param queue
      if (paramQueue == null)
      {
        //Creates queue	with max capacity =	max	possible number	of params
        paramQueue = new Queue<string>(args.Length - 2);
      }
      paramQueue.Enqueue(param);
    } // ParseArguments        

    /// <summary>
    /// Auxilary method for parsing credentials
    /// </summary>    
    /// <param name="value">Passed credentials in username:password@domain form</param>
    private NetworkCredential ParseCredentials(string value)
    {
      NetworkCredential creds = new NetworkCredential();
      Regex r = new Regex(NXsltStrings.PatternCredentials);
      Match m = r.Match(value);
      if (!m.Success)
      {
        //Credentials doesn't match pattern
        throw new NXsltCommandLineParsingException(NXsltStrings.ErrorBadCredentials, value);
      }
      creds.UserName = m.Groups["username"].Value;
      if (m.Groups["psw"] != null)
        creds.Password = m.Groups["psw"].Value;
      if (m.Groups["domain"] != null)
        creds.Domain = m.Groups["domain"].Value;
      return creds;
    } //SetCredentials

    /// <summary>
    /// Parses XML namespace declaration.
    /// </summary>
    /// <param name="arg">Namespace declaration</param>
    private void ParseNamespaceDeclaration(string arg)
    {
      if (arg.EndsWith("="))
      {
        throw new NXsltCommandLineParsingException(NXsltStrings.ErrorMissingURI, arg.Remove(arg.Length - 1, 1));
      }
      int eqIndex = arg.IndexOf('=');
      //Lazy XmlNamespaceManager
      if (namespaceManager == null)
      {
        nameTable = new NameTable();
        namespaceManager = new XmlNamespaceManager(nameTable);
      }
      //qname is xmlns:prefix or xmlns
      string qname = arg.Substring(0, eqIndex);
      int colonIndex = qname.IndexOf(':');
      if (colonIndex != -1)
        //xmlns:prefix="value" case
        namespaceManager.AddNamespace(nameTable.Add(qname.Substring(colonIndex + 1)),
          arg.Substring(eqIndex + 1));
      else
        //xmlns="value" case - default namespace
        namespaceManager.AddNamespace(String.Empty,
          arg.Substring(eqIndex + 1));
    }// ParseNamespaceDeclaration

    /// <summary>
    /// Resolves enqueued parameters.
    /// </summary>
    /// <param name="options">nxslt options collection</param>
    private void ResolveParameters(NXsltOptions options)
    {
      if (paramQueue != null)
      {
        options.XslArgList = new XsltArgumentList();
        foreach (string param in paramQueue)
        {
          int eqIndex = param.IndexOf('=');
          //qname is prefix:localname or localname
          string qname = param.Substring(0, eqIndex);
          int colonIndex = qname.IndexOf(':');
          if (colonIndex != -1)
          {
            //prefix:localname="value" case
            string prefix = qname.Substring(0, colonIndex);
            string uri;
            //Resolve prefix
            if (namespaceManager == null ||
              (uri = namespaceManager.LookupNamespace(nameTable.Get(prefix))) == null)
            {
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorUnboundedPrefix, prefix);
            }
            else
              options.XslArgList.AddParam(param.Substring(colonIndex + 1,
                eqIndex - colonIndex - 1), uri, param.Substring(eqIndex + 1));
          }
          else
            //localname="value"	case - possible default namespace
            options.XslArgList.AddParam(qname, namespaceManager == null ? String.Empty : namespaceManager.DefaultNamespace,
              param.Substring(eqIndex + 1));
        }
      }
    }// ResolveParameters

    /// <summary>
    /// Parses and instantiates extention objects.
    /// </summary>
    /// <param name="options">nxslt options collection</param>    
    private void ParseExtObjects(NXsltOptions options)
    {
      if (options.ExtClasses != null)
      {
        if (options.XslArgList == null)
          options.XslArgList = new XsltArgumentList();
        foreach (string typeDecl in options.ExtClasses)
        {
          string[] parts = typeDecl.Split(new char[] { ':' }, 2);
          if (parts.Length == 1)
          {
            Type extObjType = TypeUtils.FindType(options, parts[0]);
            try
            {
              object o = Activator.CreateInstance(extObjType);
              if (namespaceManager == null || namespaceManager.DefaultNamespace == String.Empty)
              {
                //Extension object not bound to a namespace
                throw new NXsltException(NXsltStrings.ErrorExtNoNamespace, parts[0]);
              }
              string ns = namespaceManager.DefaultNamespace;
              if (options.XslArgList.GetExtensionObject(ns) != null)
              {
                //More than one extension object in the same namespace URI
                throw new NXsltException(NXsltStrings.ErrorExtNamespaceClash, ns);

              }
              options.XslArgList.AddExtensionObject(ns, o);
            }
            catch (Exception e)
            {
              //Type cannot be instantiated
              throw new NXsltException(NXsltStrings.ErrorCreateResolver, parts[0], e.Message);
            }
          }
          else
          {
            string prefix = parts[0];
            string uri;
            //Resolve prefix
            if (namespaceManager == null ||
              (uri = namespaceManager.LookupNamespace(nameTable.Get(prefix))) == null)
            {
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorUnboundedPrefix, prefix);
            }

            Type extObjType = TypeUtils.FindType(options, parts[1]);
            try
            {
              object o = Activator.CreateInstance(extObjType);
              if (options.XslArgList.GetExtensionObject(uri) != null)
              {
                //More than one extension object in the same namespace URI
                throw new NXsltCommandLineParsingException(NXsltStrings.ErrorExtNamespaceClash, uri);
              }
              options.XslArgList.AddExtensionObject(uri, o);
            }
            catch (Exception e)
            {
              //Type cannot be instantiated
              throw new NXsltCommandLineParsingException(NXsltStrings.ErrorCreateResolver, parts[1], e.Message);
            }
          }
        }
      }
    }// ParseExtObjects

  } // NXsltArgumentsParser 

} // XmlLab.nxslt namespace
