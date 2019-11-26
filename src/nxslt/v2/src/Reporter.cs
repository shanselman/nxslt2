using System;
using System.Reflection;
using System.IO;
using System.Xml;
using System.Text;

namespace XmlLab.nxslt
{
    /// <summary>
    /// nxslt reporter class.
    /// </summary>
    internal class Reporter
    {
        private static TextWriter stdout = Console.Out;
        private static TextWriter stderr = Console.Error;

        /// <summary>
        /// Reports command line parsing error.
        /// </summary>        
        /// <param name="msg">Error message</param>
        public static void ReportCommandLineParsingError(string msg)
        {
            stderr.WriteLine();
            ReportUsage();
            stderr.WriteLine(NXsltStrings.ErrorCommandLineParsing);
            stderr.WriteLine();
            stderr.WriteLine(msg);
        }

        /// <summary>
        /// Reports an error.
        /// </summary>        
        /// <param name="msg">Error message</param>
        public static void ReportError(string msg)
        {
            stderr.WriteLine();
            stderr.WriteLine();
            stderr.WriteLine(msg);
            stderr.WriteLine();
        }

        /// <summary>
        /// Reports an error.
        /// </summary>        
        /// <param name="msg">Error message</param>
        /// <param name="arg">Message argument</param>
        public static void ReportError(string msg, params string[] args)
        {
            stderr.WriteLine();
            stderr.WriteLine();
            stderr.WriteLine(msg, args);
            stderr.WriteLine();
        }

        /// <summary>
        /// Reports command line parsing error.
        /// </summary>        
        /// <param name="msg">Error message</param>
        /// <param name="arg">Message argument</param>
        public static void ReportCommandLineParsingError(string msg, params string[] args)
        {
            stderr.WriteLine();
            ReportUsage();
            stderr.WriteLine(NXsltStrings.ErrorCommandLineParsing);
            stderr.WriteLine();
            stderr.WriteLine(msg, args);
        }

        /// <summary>
        /// Prints nxslt usage info.
        /// </summary>        
        public static void ReportUsage()
        {
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            stderr.WriteLine(NXsltStrings.UsageHeader,
              ver.Major, ver.Minor, ver.Build,
              System.Environment.Version.Major, System.Environment.Version.Minor,
              System.Environment.Version.Build, System.Environment.Version.Revision);
            stderr.WriteLine();
            stderr.WriteLine(NXsltStrings.UsageBody);
        }

        /// <summary>
        /// Prints timing info.
        /// </summary>   
        public static void ReportTimings(ref NXsltTimings timings)
        {
            stderr.WriteLine();
            stderr.WriteLine();
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            stderr.WriteLine(NXsltStrings.UsageHeader,
              ver.Major, ver.Minor, ver.Build,
              System.Environment.Version.Major, System.Environment.Version.Minor,
              System.Environment.Version.Build, System.Environment.Version.Revision);
            stderr.WriteLine();
            stderr.WriteLine(NXsltStrings.Timings, timings.XsltCompileTime,
              timings.XsltExecutionTime, timings.TotalRunTime);
        }

        /// <summary>
        /// Returns full exception's message (including inner exceptions);
        /// </summary>    
        public static String GetFullMessage(Exception e)
        {
            Exception ex = e;
            StringBuilder msg = new StringBuilder();
            while (ex != null)
            {
                if (ex is NXsltException)
                {
                    msg.AppendFormat(ex.Message);
                }
                else
                {
                    msg.AppendFormat("{0}: {1}", ex.GetType().FullName, ex.Message);
                }
                if (ex.InnerException != null)
                {
                    msg.Append(" ---> ");
                }
                ex = ex.InnerException;
            }
            return msg.ToString();
        }
    }
}
