using System;
using System.Reflection;

namespace XmlLab.nxslt
{
  /// <summary>
  /// Type utility methods.
  /// </summary>
  internal class TypeUtils
  {
    public static Type FindType(NXsltOptions options, string typeName)
    {
      Type type;
      Assembly assembly;
      if (options.AssemblyFileName != null)
      {
        //
        //Load assembly from a file
        //                
        try
        {
          assembly = Assembly.LoadFrom(options.AssemblyFileName);
        }
        catch
        {
          //Assembly cannot be loaded
          throw new NXsltException(NXsltStrings.ErrorLoadAssembly, options.AssemblyFileName);          
        }
        try
        {
          type = assembly.GetType(typeName, true);
        }
        catch
        {
          //Type not found in the specified assembly
          throw new NXsltException(NXsltStrings.ErrorGetTypeFromAssembly, typeName, options.AssemblyFileName);          
        }
      }
      else if (options.AssemblyName != null)
      {
        //
        //Load assembly by name
        //                
        try
        {
          assembly = Assembly.LoadWithPartialName(options.AssemblyName);          
        }
        catch
        {
          //Assembly cannot be loaded
          throw new NXsltException(NXsltStrings.ErrorLoadAssembly, options.AssemblyName);          
        }
        try
        {
          type = assembly.GetType(typeName, true);
        }
        catch
        {
          throw new NXsltException(NXsltStrings.ErrorGetType, typeName);          
        }
      }
      else
      {
        //
        //Assembly not specified                        
        //
        try
        {
          type = Type.GetType(typeName, true);
        }
        catch
        {
          throw new NXsltException(NXsltStrings.ErrorGetType, typeName);          
        }
      }
      return type;
    } // FindType()        
  }
}
