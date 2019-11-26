using System;
using System.Collections.Specialized;
using System.Collections;
using System.IO;
using System.Text;
using System.Configuration;

namespace Mvp.Xml.Exslt.MethodRenamer
{
  /// <summary>
  /// An utility to rename methods in MSIL code.
  /// </summary>
  public class MethodRenamer
  {
    [STAThread]
    static void Main(string[] args)
    {
      IDictionary dictionary = (IDictionary)ConfigurationManager.GetSection("names");
      //Reads input IL code
      StreamReader reader = new StreamReader(args[0]);
      //Writes output IL code
      StreamWriter writer = new StreamWriter(args[1]);
      string line;
      //Go read line by line
      while ((line = reader.ReadLine()) != null)
      {
        //Method definition?
        if (line.Trim().StartsWith(".method"))
        {
          writer.WriteLine(line);
          line = reader.ReadLine();
          if (line.IndexOf("(") != -1)
          {
            string methodName = line.Trim().Substring(0, line.Trim().IndexOf("("));
            if (dictionary.Contains(methodName))
            {
              writer.WriteLine(line.Replace(methodName + "(",
                  "'" + (string)dictionary[methodName] + "'("));
              Console.WriteLine("Found '" + methodName + "' method, renamed to '" +
                  dictionary[methodName] + "'");
            }
            else
              writer.WriteLine(line);
          }
          else
            writer.WriteLine(line);
        }
        else
          writer.WriteLine(line);
      }
      reader.Close();
      writer.Close();
    }
  }
}