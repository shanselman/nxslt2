using System;

namespace XmlLab.nxslt
{
  /// <summary>
  /// General nxslt error.
  /// </summary>
  internal class NXsltException : Exception 
  {
    public NXsltException(string msg)
      : base(msg) { }

    public NXsltException(string msg, string arg)
      : base(string.Format(msg, arg)) { }

    public NXsltException(string msg, params string[] args)
      : base(string.Format(msg, args)) { }
  }
}
