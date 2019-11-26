using System;

namespace XmlLab.nxslt
{
  /// <summary>
  /// nxslt command line parsing error.
  /// </summary>
  internal class NXsltCommandLineParsingException : NXsltException
  {
    public NXsltCommandLineParsingException(string msg)
      : base(msg) { }

    public NXsltCommandLineParsingException(string msg, string arg)
      : base(string.Format(msg, arg)) { }

    public NXsltCommandLineParsingException(string msg, params string[] args)
      : base(string.Format(msg, args)) { }
  }
}
