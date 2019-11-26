using System;
using System.Xml.Xsl;
using System.Net;

namespace XmlLab.nxslt
{
  /// <summary>
  /// This class represents parsed command line options.
  /// </summary>
  internal class NXsltOptions 
  {
    private bool stripWhiteSpace = false;
    private bool showHelp = false;  
    private string source;
    private string stylesheet;
    private string outFile; 
    private XsltArgumentList xslArgList;
    private bool showTiming = false;
    private bool getStylesheetFormPI = false;
    private bool validateDocs = false;
    private bool resolveExternals = true;
    private bool processXInclude = true;
    private bool loadSourceFromStdin = false;
    private bool loadStylesheetFromStdin = false;
    private string resolverTypeName;
    private string assemblyFileName;
    private string assemblyName;
    private bool multiOutput = false;
    private string[] extClasses;       
    private NetworkCredential sourceCredential;
    private NetworkCredential xslCredential;
    private bool noSourceXml;
    private bool prettyPrintMode;
    private bool identityTransformMode;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public NXsltOptions() {}

    /// <summary>
    /// Network credential for loading XML source
    /// </summary>
    public NetworkCredential SourceCredential 
    {
      get { return sourceCredential; }
      set { sourceCredential = value; }
    }
    
    /// <summary>
    /// Network credential for loading XSLT stylesheet
    /// </summary>
    public NetworkCredential XSLTCredential 
    {
      get { return xslCredential; }
      set { xslCredential = value; }
    }

    /// <summary>
    /// Load source XML from stdin.
    /// </summary>
    
    public bool LoadSourceFromStdin 
    {
      get { return loadSourceFromStdin; }
      set { loadSourceFromStdin = value; }
    }
        
    /// <summary>
    /// Load stylesheet from stdin.
    /// </summary>
    public bool LoadStylesheetFromStdin 
    {
      get { return loadStylesheetFromStdin; }
      set { loadStylesheetFromStdin = value; }
    }

    /// <summary>
    /// Resolve external definitions during parse phase.
    /// </summary>
    public bool ResolveExternals 
    {
      get { return resolveExternals; }
      set { resolveExternals = value; }
    }

    public bool ProcessXInclude 
    {
      get { return processXInclude; }
      set { processXInclude = value; }
    }

    /// <summary>
    /// Validate documents during parse phase.
    /// </summary>
    public bool ValidateDocs 
    {
      get { return validateDocs; }
      set { validateDocs = value; }
    }
        
    public string[] ExtClasses 
    {
      get { return extClasses; }
      set { extClasses = value; }
    }
        
    /// <summary>
    /// Get stylesheet URI from xml-stylesheet PI in source document.
    /// </summary>
    public bool GetStylesheetFromPI 
    {
      get { return getStylesheetFormPI; }
      set { getStylesheetFormPI = value; }
    }

    /// <summary>
    /// Show load and transformation timings.
    /// </summary>
    public bool ShowTiming 
    {
      get { return showTiming; }
      set { showTiming = value; }
    }

    /// <summary>
    /// List of XSLT parameters.
    /// </summary>
    public XsltArgumentList XslArgList 
    {
      get { return xslArgList; }
      set { xslArgList = value; }
    }

    /// <summary>
    /// Output file name.
    /// </summary>
    public string OutFile 
    {
      get { return outFile; }
      set { outFile = value; }
    }

    /// <summary>
    /// Strip non-significant whitespace from source and stylesheet.
    /// </summary>
    public bool StripWhiteSpace 
    {
      get { return stripWhiteSpace; }
      set { stripWhiteSpace = value; }            
    }
        
    /// <summary>
    /// Show usage message.
    /// </summary>
    public bool ShowHelp 
    {
      get { return showHelp; }
      set { showHelp = value; }
    }
        
    /// <summary>
    /// Source XML URI.
    /// </summary>
    public string Source 
    {
      get { return source; }
      set { source = value; }
    }
        
    /// <summary>
    /// Stylesheet URI.
    /// </summary>
    public string Stylesheet 
    {
      get { return stylesheet; }
      set { stylesheet = value; }
    }        
        
    /// <summary>
    /// URI resolver type name.
    /// </summary>
    public string ResolverTypeName 
    {
      get { return resolverTypeName; }
      set { resolverTypeName = value; }
    }
        
    /// <summary>
    /// Assembly file name.
    /// </summary>
    public string AssemblyFileName 
    {
      get { return assemblyFileName; }
      set { assemblyFileName = value; }
    }
        
    /// <summary>
    /// Assembly name.
    /// </summary>
    public string AssemblyName 
    {
      get { return assemblyName; }
      set { assemblyName = value; }
    }
        
    /// <summary>
    /// Multiple outputs support
    /// </summary>
    public bool MultiOutput 
    {
      get { return multiOutput; }
      set { multiOutput = value; }
    }    

    /// <summary>
    /// No source XML
    /// </summary>
    public bool NoSourceXml 
    {
      get { return noSourceXml; }
      set { noSourceXml = value; }
    }

    /// <summary>
    /// Pretty print mode
    /// </summary>
    public bool PrettyPrintMode 
    {
      get { return prettyPrintMode; }
      set { prettyPrintMode = value; }
    }

    /// <summary>
    /// Identity transform mode
    /// </summary>
    public bool IdentityTransformMode
    {
      get { return identityTransformMode; }
      set { identityTransformMode = value; }
    }  
  }
}
