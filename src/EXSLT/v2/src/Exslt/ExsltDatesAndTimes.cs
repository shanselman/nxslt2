#region using

using System;
using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using System.Text;
using System.Text.RegularExpressions;

#endregion

namespace Mvp.Xml.Exslt
{
	/// <summary>
	/// This class implements the EXSLT functions in the http://exslt.org/dates-and-times namespace.
	/// </summary>
	public class ExsltDatesAndTimes
	{
		private CultureInfo ci = new CultureInfo("en-US");

		private class ExsltDateTimeFactory
		{
			/// <summary>
			/// Parse a date and time for format-date() 
			/// </summary>
			/// <param name="d"></param>
			/// <returns></returns>
			public static ExsltDateTime ParseDateTime(string d)
			{
				// First try any of the classes in ParseDate
				try
				{
					return ParseDate(d);
				}
				catch(FormatException)
				{
				}
				
				try
				{
					TimeTZ t = new TimeTZ(d);
					return t;
				}
				catch(FormatException)
				{
				}				

				try
				{
					MonthDay t = new MonthDay(d);
					return t;
				}
				catch(FormatException)
				{
				}				

				try
				{
					Month t = new Month(d);
					return t;
				}
				catch(FormatException)
				{
				}	

				// Finally day -- don't catch the exception
				{
					Day t = new Day(d);
					return t;
				}
			}

			/// <summary>
			/// Initialize the structure with the current date, time and timezone
			/// </summary>
			public static ExsltDateTime ParseDate(string d)
			{
				// Try each potential class, from most specific to least specific.

				// First DateTimeTZ
				try
				{
					DateTimeTZ t = new DateTimeTZ(d);
					return t; 
				}
				catch(FormatException)
				{
				}

				// Next Date
				try
				{
					DateTZ t = new DateTZ(d);
					return t; 
				}
				catch(FormatException)
				{
				}

				// Next YearMonth
				try
				{
					YearMonth t = new YearMonth(d);
					return t; 
				}
				catch(FormatException)
				{
				}

				// Finally Year -- don't catch the exception for the last type
				{	
					YearTZ t = new YearTZ(d);
					return t; 
				}
			}
		}

		internal abstract class ExsltDateTime
		{
			public DateTime	d;
			public TimeSpan	ts = new TimeSpan(TimeSpan.MinValue.Ticks);

			protected CultureInfo ci = new CultureInfo("en-US");

			/// <summary>
			/// Initialize the structure with the current date, time and timezone
			/// </summary>
			public ExsltDateTime()
			{
				d = DateTime.Now;
				TimeZone tz = TimeZone.CurrentTimeZone;
				ts = tz.GetUtcOffset(d);
			}

			/// <summary>
			/// Initialize the DateTimeTZ structure with the date, time and timezone in the string.
			/// </summary>
			/// <param name="inS">An ISO8601 string</param>
			public ExsltDateTime(string inS)
			{
				String s = inS.Trim();
				d = DateTime.ParseExact(s, expectedFormats,	ci, DateTimeStyles.AdjustToUniversal);

				if (s.EndsWith("Z"))
					ts = new TimeSpan(0, 0, 0);
				else if (s.Length > 6)
				{
					String zoneStr = s.Substring(s.Length-6, 6);
					if (zoneStr[3] == ':')
					{
						try
						{
							int	hours = Int32.Parse(zoneStr.Substring(0,3));
							int minutes = Int32.Parse(zoneStr.Substring(4,2));
							if (hours < 0)
								minutes = -minutes;

							ts = new TimeSpan(hours, minutes, 0);
							d = d.Add(ts);	// Adjust to time zone relative time						
						}
						catch(Exception)
						{
						}
					}
				}
			}

			/// <summary>
			/// Exslt Copy constructor
			/// Initialize the structure with the date, time and timezone in the string.
			/// </summary>
			/// <param name="inS">An ExsltDateTime</param>
			public ExsltDateTime(ExsltDateTime inS)
			{
				d = inS.d;
				ts = inS.ts;
			}

			public bool HasTimeZone()
			{
				return !(TimeSpan.MinValue.Ticks == ts.Ticks);
			}

			public DateTime ToUniversalTime()
			{
				if (!HasTimeZone())
					return d;
				else
					return d.Subtract(ts);
			}

			/// <summary>
			/// Output as a standard (ISO8601) string
			/// </summary>
			/// <returns>the date and time as an ISO8601 string.  includes timezone</returns>
			public override string ToString()
			{
				return this.ToString(outputFormat);
			}

			/// <summary>
			/// Output as a formatted string
			/// </summary>
			/// <returns>the date and time as a formatted string.  includes timezone</returns>
			public string ToString(String of)
			{
				StringBuilder retString = new StringBuilder("");
						
				retString.Append(d.ToString(of));
				retString.Append(GetTimeZone());

				return retString.ToString();
			}

			public string GetTimeZone()
			{
				StringBuilder retString = new StringBuilder();

				// if no ts specified, output without ts
				if (HasTimeZone())
				{
					if (0 == ts.Hours && 0 == ts.Minutes)
						retString.Append('Z');
					else if (ts.Hours >= 0 && ts.Minutes >= 0)
					{
						retString.Append('+');
						retString.Append(ts.Hours.ToString().PadLeft(2, '0'));
						retString.Append(':');
						retString.Append(ts.Minutes.ToString().PadLeft(2, '0'));				
					}
					else
					{
						retString.Append('-');
						retString.Append((-ts.Hours).ToString().PadLeft(2, '0'));
						retString.Append(':');
						retString.Append((-ts.Minutes).ToString().PadLeft(2, '0'));
					}
				}
				
				return retString.ToString();
			}

			public string GetGMTOffsetTimeZone()
			{
				StringBuilder retString = new StringBuilder();

				// if no ts specified, output without ts
				if (HasTimeZone())
				{
					retString.Append("GMT");
					if (0 != ts.Hours || 0 != ts.Minutes)
					{
						retString.Append(GetTimeZone());
					}
				}
				
				return retString.ToString();
			}

			public string Get822TimeZone()
			{
				StringBuilder retString = new StringBuilder();

				// if no ts specified, output without ts
				if (HasTimeZone())
				{
					if (0 == ts.Hours && 0 == ts.Minutes)
						retString.Append("GMT");
					else if (ts.Hours >= 0 && ts.Minutes >= 0)
					{
						retString.Append('+');
						retString.Append(ts.Hours.ToString().PadLeft(2, '0'));
						retString.Append(ts.Minutes.ToString().PadLeft(2, '0'));				
					}
					else
					{
						retString.Append('-');
						retString.Append((-ts.Hours).ToString().PadLeft(2, '0'));
						retString.Append((-ts.Minutes).ToString().PadLeft(2, '0'));
					}
				}
				
				return retString.ToString();
			}

			protected abstract string[] expectedFormats {get;}
			protected abstract string outputFormat {get;}
		}

		internal class DateTimeTZ : ExsltDateTime
		{
			public DateTimeTZ() : base(){}
			public DateTimeTZ(string inS) : base(inS){}
			public DateTimeTZ(ExsltDateTime inS) : base(inS){}

			protected override string[] expectedFormats
			{
				get 
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
											"yyyy-MM-dd\"T\"HH:mm:ssZ", 
											"yyyy-MM-dd\"T\"HH:mm:ss",
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f"  
};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "yyyy-MM-dd\"T\"HH:mm:ss";
				}
			}
		}


		internal class DateTZ : ExsltDateTime
		{
			public DateTZ() : base(){}
			public DateTZ(string inS) : base(inS){}
			public DateTZ(ExsltDateTime inS) : base(inS){}

			protected override string[] expectedFormats
			{
				get
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
											"yyyy-MM-dd\"T\"HH:mm:ssZ", 
											"yyyy-MM-dd\"T\"HH:mm:ss",
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f",
											"yyyy-MM-ddzzz",
											"yyyy-MM-ddZ",
											"yyyy-MM-dd"};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "yyyy-MM-dd";
				}
			}
		}

		internal class TimeTZ : ExsltDateTime
		{
			public TimeTZ(string inS) : base(inS){}
			public TimeTZ() : base(){}

			protected override string[] expectedFormats
			{
				get
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
											"yyyy-MM-dd\"T\"HH:mm:ssZ", 
											"yyyy-MM-dd\"T\"HH:mm:ss",
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f",
											"HH:mm:sszzz",
											"HH:mm:ssZ",
											"HH:mm:ss"};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "HH:mm:ss";
				}
			}
		}

		internal class YearMonth : ExsltDateTime
		{
			public YearMonth() : base(){}
			public YearMonth(string inS) : base(inS){}
			public YearMonth(ExsltDateTime inS) : base(inS){}

			protected override string[] expectedFormats
			{
				get
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
											"yyyy-MM-dd\"T\"HH:mm:ssZ", 
											"yyyy-MM-dd\"T\"HH:mm:ss",
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f",
											"yyyy-MM-dd",
											"yyyy-MM"};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "yyyy-MM";
				}
			}
		}

		internal class YearTZ : ExsltDateTime
		{
			public YearTZ() : base(){}
			public YearTZ(string inS) : base(inS){}
			public YearTZ(ExsltDateTime inS) : base(inS){}

			protected override string[] expectedFormats
			{
				get
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
											"yyyy-MM-dd\"T\"HH:mm:ssZ", 
											"yyyy-MM-dd\"T\"HH:mm:ss",
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f",
											"yyyy-MM-dd",
											"yyyy-MM",
											"yyyy"};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "yyyy";
				}
			}
		}

		internal class Month : ExsltDateTime
		{
			public Month() : base(){}
			public Month(string inS) : base(inS){}
			public Month(ExsltDateTime inS) : base(inS){}

			protected override string[] expectedFormats
			{
				get
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
										 "yyyy-MM-dd\"T\"HH:mm:ssZ", 
										 "yyyy-MM-dd\"T\"HH:mm:ss",
                                         "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f",   
										 "yyyy-MM-dd",
										 "yyyy-MM",
										 "--MM--"};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "--MM--";
				}
			}
		}

		internal class Day : ExsltDateTime
		{
			public Day() : base(){}
			public Day(string inS) : base(inS){}
			public Day(ExsltDateTime inS) : base(inS){}

			protected override string[] expectedFormats
			{
				get
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
											"yyyy-MM-dd\"T\"HH:mm:ssZ", 
											"yyyy-MM-dd\"T\"HH:mm:ss",
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f",
											"yyyy-MM-dd",
											"---dd",
											"--MM-dd"};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "---dd";
				}
			}
		}

		internal class MonthDay : ExsltDateTime
		{
			public MonthDay() : base(){}
			public MonthDay(string inS) : base(inS){}
			public MonthDay(ExsltDateTime inS) : base(inS){}

			protected override string[] expectedFormats
			{
				get
				{
					return new string[] {"yyyy-MM-dd\"T\"HH:mm:sszzz", 
											"yyyy-MM-dd\"T\"HH:mm:ssZ", 
											"yyyy-MM-dd\"T\"HH:mm:ss",
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ffZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.ff",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fzzz",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.fZ",  
                                            "yyyy-MM-dd\"T\"HH:mm:ss.f",
											"yyyy-MM-dd",
											"--MM-dd"};
				}
			}

			protected override string outputFormat
			{
				get
				{
					return "--MM-dd";
				}
			}
		}


		private string[] dayAbbrevs = {"Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"};
		private string[] dayNames = {"Sunday", "Monday", "Tuesday", 
										"Wednesday", "Thursday", "Friday", "Saturday"};

		private string[] monthAbbrevs = {"Jan", "Feb", "Mar", "Apr", "May", "Jun",
											"Jul", "Aug", "Sep", "Oct", "Nov", "Dec"};
		private string[] monthNames = {"January", "February", "March", "April", "May", "June",
										  "July", "August", "September", 
										  "October", "November", "December"};

		
		/// <summary>
		/// Implements the following function
		///   string date:date-time()
		/// Output format is ISO 8601 (YYYY-MM-DDThh:mm:ss{Z | {+ | -}zz:zz}).
		/// YYYY - year with century
		/// MM - month in numbers with leading zero
		/// DD - day in numbers with leading zero
		/// T - the letter T
		/// hh - hours in numbers with leading zero (00-23).
		/// mm - minutes in numbers with leading zero (00-59).
		/// ss - seconds in numbers with leading zero (00-59).
		/// +/-zzzz - time zone expressed as hours and minutes from UTC.
		///		If UTC, then this is the letter Z
		///		If east of Greenwich, then -zz:zz (e.g. Pacific standard time is -08:00)
		///		If west of Greenwich, then +zz:zz (e.g. Tokyo is +09:00)
		/// </summary>
		/// <returns>The current time.</returns>		
		public string dateTime()
		{		
			DateTimeTZ	d = new DateTimeTZ();
			return dateTimeImpl(d);
		}
        
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dateTime_RENAME_ME() 
        {
            return dateTime();
        }    

		/// <summary>
		/// Implements the following function
		///   string date:date-time()
		/// </summary>
		/// <returns>The current date and time or the empty string if the 
		/// date is invalid </returns>        
		public string dateTime(string s){		
			try
			{
				DateTimeTZ d = new DateTimeTZ(s);
				return dateTimeImpl(d);
			}
			catch(FormatException)
			{
				return ""; 
			}
		}
		
		/// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dateTime_RENAME_ME(string d) 
        {
            return dateTime(d);
        }

		/// <summary>
		/// Internal function to format the date based on a date, rather than a string
		/// </summary>
		/// <returns>The formtted date and time as a ISO8601 string</returns>        

		internal string dateTimeImpl(DateTimeTZ dtz)
		{
			return dtz.ToString();
		}

		/// <summary>
		/// Implements the following function
		///   string date:date()
		/// </summary>
		/// <returns>The current date</returns>        
		public string date()
		{
			DateTZ dtz = new DateTZ();
			return dtz.ToString();
		}

		/// <summary>
		/// Implements the following function
		///   string date:date(string)
		/// </summary>
		/// <returns>The date part of the specified date or the empty string if the 
		/// date is invalid</returns>        
		public string date(string d)
		{
			try
			{
				DateTZ dtz = new DateTZ(d);				
				return dtz.ToString(); 
			}
			catch(FormatException)
			{
				return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///   string date:time()
		/// </summary>
		/// <returns>The current time</returns>        
		public string time()
		{
			TimeTZ t = new TimeTZ();
			return t.ToString();
		}

		/// <summary>
		/// Implements the following function
		///   string date:time(string)
		/// </summary>
		/// <returns>The time part of the specified date or the empty string if the 
		/// date is invalid</returns>        
		public string time(string d)
		{
			try
			{
				TimeTZ t = new TimeTZ(d);
				return t.ToString(); 
			}
			catch(FormatException)
			{
				return ""; 
			}
		}
		

		/// <summary>
		/// Implements the following function
		///   number date:year()
		/// </summary>
		/// <returns>The current year</returns>        
		public double year()
		{
			return DateTime.Now.Year;
		}

		/// <summary>
		/// Implements the following function
		///   number date:year(string)
		/// </summary>
		/// <returns>The year part of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types</remarks>        
		public double year(string d)
		{
			try	
			{
				YearTZ date = new YearTZ(d);
				return date.d.Year; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}

		/// <summary>
		/// Helper method for calculating whether a year is a leap year. Algorithm 
		/// obtained from http://mindprod.com/jglossleapyear.html
		/// </summary>        
		private static bool IsLeapYear (int year) 
		{ 	
            try 
            {
                return CultureInfo.CurrentCulture.Calendar.IsLeapYear(year); 
            } 
            catch 
            {
                return false;
            }
		}


		/// <summary>
		/// Implements the following function
		///   boolean date:leap-year()
		/// </summary>
		/// <returns>True if the current year is a leap year.</returns>        
		public bool leapYear()
		{
			return IsLeapYear((int) year());
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public bool leapYear_RENAME_ME() 
        {
            return leapYear();
        }

		/// <summary>
		/// Implements the following function
		///   boolean date:leap-year(string)
		/// </summary>
		/// <returns>True if the specified year is a leap year</returns>
		/// <remarks>Note that the spec says we should return NaN for a badly formatted input
		/// string.  This is impossible; we return false for a badly formatted input string.
		/// </remarks>        
		public bool leapYear(string d)
		{
			double y = year(d); 

			if (y == System.Double.NaN)
				return false;
			else
				return IsLeapYear((int)y);
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public bool leapYear_RENAME_ME(string d) 
        {
            return leapYear(d);
        }

		/// <summary>
		/// Implements the following function
		///   number date:month-in-year()
		/// </summary>
		/// <returns>The current month</returns>        
		public double monthInYear()
		{
			return DateTime.Now.Month;
		}
        
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double monthInYear_RENAME_ME() 
        {
            return monthInYear();
        }

		/// <summary>
		/// Implements the following function
		///   number date:month-in-year(string)
		/// </summary>
		/// <returns>The month part of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of xs:gYear</remarks>        
		public double monthInYear(string d)
		{
			try
			{
				Month date = new Month(d);
				return date.d.Month; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double monthInYear_RENAME_ME(string d) 
        {
            return monthInYear(d);
        }

		/// <summary>
		/// Helper funcitno to calculate the week number
		/// </summary>
		/// <returns>
		/// Returns the week in the year.  Obeys EXSLT spec, which specifies that counting follows 
		/// ISO 8601: week 1 in a year is the week containing the first Thursday of the year, with 
		/// new weeks beginning on a Monday
		/// </returns>
		/// <param name="d">The date for which we want to find the week</param>
		private double weekInYear(DateTime d)
		{
		    return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(d, 
										  System.Globalization.CalendarWeekRule.FirstFourDayWeek, 
										  System.DayOfWeek.Monday);
		}		        

		/// <summary>
		/// Implements the following function
		///   number date:week-in-year()
		/// </summary>
		/// <returns>
		/// The current week. Obeys EXSLT spec, which specifies that counting follows 
		/// ISO 8601: week 1 in a year is the week containing the first Thursday of the year, with 
		/// new weeks beginning on a Monday
		/// </returns>        
		public double weekInYear()
		{
			return this.weekInYear(DateTime.Now);
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double weekInYear_RENAME_ME() 
        {
            return weekInYear();
        }

		/// <summary>
		/// Implements the following function
		///   number date:week-in-year(string)
		/// </summary>
		/// <returns>The week part of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types. This method uses the Calendar.GetWeekOfYear() method 
		/// with the CalendarWeekRule and FirstDayOfWeek of the current culture.
		/// THE RESULTS OF CALLING THIS FUNCTION VARIES ACROSS CULTURES</remarks>        
		public double weekInYear(string d)
		{
			try
			{
				DateTZ dtz = new DateTZ(d);
				return weekInYear(dtz.d);
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double weekInYear_RENAME_ME(string d) 
        {
            return weekInYear(d);
        }

        /// <summary>
        /// Helper method. 
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private double weekInMonth(DateTime d)
		{
			//
			// mon = 1
			// tue = 2
			// sun = 7
			// week = ceil(((date-day) / 7)) + 1

			double offset = (d.DayOfWeek == DayOfWeek.Sunday) ? 7 : (double)d.DayOfWeek;
			return System.Math.Ceiling((d.Day-offset) / 7)+1;
        }

        /// <summary>
        /// Implements the following function
        ///   number date:week-in-month()
        /// </summary>
		/// <remarks>
		/// The current week in month as a number.  For the purposes of numbering, the first 
		/// day of the month is in week 1 and new weeks begin on a Monday (so the first and 
		/// last weeks in a month will often have less than 7 days in them). 
		/// </remarks>        
		public double weekInMonth()
        {
            return this.weekInMonth(DateTime.Now);
        }
        
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double weekInMonth_RENAME_ME() 
        {
            return weekInMonth();
        }

        /// <summary>
        /// Implements the following function
        ///   number date:week-in-month(string)
        /// </summary>
        /// <returns>The week in month of the specified date or NaN if the 
        /// date is invalid</returns>
		/// <remarks>
		/// The current week in month as a number.  For the purposes of numbering, the first 
		/// day of the month is in week 1 and new weeks begin on a Monday (so the first and 
		/// last weeks in a month will often have less than 7 days in them). 
		/// </remarks>        
        public double weekInMonth(string d)
        {
            try
            {
                DateTZ date = new DateTZ(d); 
                return this.weekInMonth(date.d); 
            }
            catch(FormatException)
            {
                return System.Double.NaN; 
            }
        }
        
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double weekInMonth_RENAME_ME(string d) 
        {
            return weekInMonth(d);
        }


		/// <summary>
		/// Implements the following function
		///   number date:day-in-year()
		/// </summary>
		/// <returns>The current day. </returns>        
		public double dayInYear()
		{
			return DateTime.Now.DayOfYear;
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayInYear_RENAME_ME() 
        {
            return dayInYear();
        }

		/// <summary>
		/// Implements the following function
		///   number date:day-in-year(string)
		/// </summary>
		/// <returns>
		/// The day part of the specified date or NaN if the date is invalid
		/// </returns>        
		public double dayInYear(string d)
		{
			try
			{
				DateTZ date = new DateTZ(d); 
				return date.d.DayOfYear; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayInYear_RENAME_ME(string d) 
        {
            return dayInYear(d);
        }

		/// <summary>
		/// Implements the following function
		///   number date:day-in-week()
		/// </summary>
		/// <returns>The current day in the week. 1=Sunday, 2=Monday,...,7=Saturday</returns>        
		public double dayInWeek()
		{
			return ((int) DateTime.Now.DayOfWeek) + 1;
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayInWeek_RENAME_ME() 
        {
            return dayInWeek();
        }

		/// <summary>
		/// Implements the following function
		///   number date:day-in-week(string)
		/// </summary>
		/// <returns>The day in the week of the specified date or NaN if the 
		/// date is invalid. The current day in the week. 1=Sunday, 2=Monday,...,7=Saturday
		/// </returns>        
		public double dayInWeek(string d){
			try
			{
				DateTZ date = new DateTZ(d); 
				return ((int)date.d.DayOfWeek) + 1; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayInWeek_RENAME_ME(string d) 
        {
            return dayInWeek(d);
        }


		/// <summary>
		/// Implements the following function
		///   number date:day-in-month()
		/// </summary>
		/// <returns>The current day. </returns>        
		public double dayInMonth()
		{
			return DateTime.Now.Day;
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayInMonth_RENAME_ME() 
        {
            return dayInMonth();
        }

		/// <summary>
		/// Implements the following function
		///   number date:day-in-month(string)
		/// </summary>
		/// <returns>The day part of the specified date or the empty string if the 
		/// date is invalid</returns>
		public double dayInMonth(string d)
		{
			try
			{
				Day date = new Day(d);
				return date.d.Day; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayInMonth_RENAME_ME(string d) 
        {
            return dayInMonth(d);
        }

		/// <summary>
		/// Helper method.
		/// </summary>
		/// <param name="day"></param>
		/// <returns></returns>
		private double dayOfWeekInMonth(int day)
		{
			// day of week in month = floor(((date-1) / 7)) + 1
			return ((day-1)/7) + 1;
		}

		/// <summary>
		/// Implements the following function
		///   number date:day-of-week-in-month()
		/// </summary>
		/// <returns>The current day of week in the month as a number. For instance 
		/// the third Friday of the month returns 3</returns>        
		public double dayOfWeekInMonth()
		{
			return this.dayOfWeekInMonth(DateTime.Now.Day);
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayOfWeekInMonth_RENAME_ME() 
        {
            return dayOfWeekInMonth();
        }

		/// <summary>
		/// Implements the following function
		///   number date:day-of-week-in-month(string)
		/// </summary>
		/// <returns>The day part of the specified date or NaN if the 
		/// date is invalid</returns>        
		public double dayOfWeekInMonth(string d)
		{
			try
			{
				DateTZ date = new DateTZ(d); 
				return this.dayOfWeekInMonth(date.d.Day); 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double dayOfWeekInMonth_RENAME_ME(string d) 
        {
            return dayOfWeekInMonth(d);
        }
	
		/// <summary>
		/// Implements the following function
		///   number date:hour-in-day()
		/// </summary>
		/// <returns>The current hour of the day as a number.</returns>        
		public double hourInDay()
		{
			return DateTime.Now.Hour;
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double hourInDay_RENAME_ME() 
        {
            return hourInDay();
        }

		/// <summary>
		/// Implements the following function
		///   number date:hour-in-day(string)
		/// </summary>
		/// <returns>The current hour of the specified time or NaN if the 
		/// date is invalid</returns>        
		public double hourInDay(string d)
		{
			try
			{
				TimeTZ date = new TimeTZ(d); 
				return date.d.Hour; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double hourInDay_RENAME_ME(string d) 
        {
            return hourInDay(d);
        }

		/// <summary>
		/// Implements the following function
		///   number date:minute-in-hour()
		/// </summary>
		/// <returns>The minute of the current hour as a number. </returns>        
		public double minuteInHour()
		{
			return DateTime.Now.Minute;
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double minuteInHour_RENAME_ME() 
        {
            return minuteInHour();
        }

		/// <summary>
		/// Implements the following function
		///   number date:minute-in-hour(string)
		/// </summary>
		/// <returns>The minute of the hour of the specified time or NaN if the 
		/// date is invalid</returns>        
		public double minuteInHour(string d)
		{
			try
			{
				TimeTZ date = new TimeTZ(d); 
				return date.d.Minute; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}

        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double minuteInHour_RENAME_ME(string d) 
        {
            return minuteInHour(d);
        }

		/// <summary>
		/// Implements the following function
		///   number date:second-in-minute()
		/// </summary>
		/// <returns>The seconds of the current minute as a number. </returns>        
		public double secondInMinute()
		{
			return DateTime.Now.Second;
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double secondInMinute_RENAME_ME() 
        {
            return secondInMinute();
        }		

		/// <summary>
		/// Implements the following function
		///   number date:second-in-minute(string)
		/// </summary>
		/// <returns>The seconds of the minute of the specified time or NaN if the 
		/// date is invalid</returns>        
		public double secondInMinute(string d)
		{
			try
			{
				TimeTZ date = new TimeTZ(d); 
				return date.d.Second; 
			}
			catch(FormatException)
			{
				return System.Double.NaN; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public double secondInMinute_RENAME_ME(string d) 
        {
            return secondInMinute(d);
        }

		/// <summary>
		/// Helper function for 
		///   string date:day-name()
		/// </summary>
		/// <returns>The Engish name of the current day</returns>        
		private string dayName(int dow)
		{
            if (dow <0 || dow >= dayNames.Length)
                return String.Empty;
			return dayNames[dow];
		}

		/// <summary>
		/// Implements the following function
		///   string date:day-name()
		/// </summary>
		/// <returns>The Engish name of the current day</returns>        
		public string dayName()
		{
			return dayName((int) DateTime.Now.DayOfWeek);
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dayName_RENAME_ME() 
        {
            return dayName();
        }

		/// <summary>
		/// Implements the following function
		///   string date:day-name(string)
		/// </summary>
		/// <returns>The English name of the day of the specified date or the empty string if the 
		/// date is invalid</returns>        
		public string dayName(string d)
		{
			try
			{
				DateTZ date = new DateTZ(d); 
				return dayName((int) date.d.DayOfWeek);
			}
			catch(FormatException)
			{
				return ""; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dayName_RENAME_ME(string d) 
        {
            return dayName(d);
        }

		/// <summary>
		/// Helper function for 
		///   string date:day-abbreviation()
		/// </summary>
		/// <returns>The abbreviated English name of the current day</returns>        
		private string dayAbbreviation(int dow)
		{
            if (dow < 0 || dow >= dayAbbrevs.Length)
                return String.Empty;
			return dayAbbrevs[dow];
		}


		/// <summary>
		/// Implements the following function
		///   string date:day-abbreviation()
		/// </summary>
		/// <returns>The abbreviated English name of the current day</returns>        
		public string dayAbbreviation()
		{
			return dayAbbreviation((int)DateTime.Now.DayOfWeek);
		}

        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dayAbbreviation_RENAME_ME() 
        {
            return dayAbbreviation();
        }

		/// <summary>
		/// Implements the following function
		///   string date:day-abbreviation(string)
		/// </summary>
		/// <returns>The abbreviated English name of the day of the specified date or the 
		/// empty string if the input date is invalid</returns>        
		public string dayAbbreviation(string d)
		{
			try
			{
				DateTZ date = new DateTZ(d); 
				return dayAbbreviation((int)date.d.DayOfWeek);
			}
			catch(FormatException)
			{
				return ""; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string dayAbbreviation_RENAME_ME(string d) 
        {
            return dayAbbreviation(d);
        }

		/// <summary>
		/// Helper Function for 
		///   string date:month-name()
		/// </summary>
		/// <returns>The name of the current month</returns>        
		private string monthName(int month)
		{
            if (month < 1 || month > monthNames.Length)
                return String.Empty;
			return monthNames[month-1];
		}

		/// <summary>
		/// Implements the following function
		///   string date:month-name()
		/// </summary>
		/// <returns>The name of the current month</returns>        
		public string monthName()
		{
			return monthName((int)monthInYear());
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string monthName_RENAME_ME() 
        {
            return monthName();
        }

		/// <summary>
		/// Implements the following function
		///   string date:month-name(string)
		/// </summary>
		/// <returns>The name of the month of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of xs:gYear types</remarks>        
		public string monthName(string d)
		{
			double month = monthInYear(d);
			if (month == System.Double.NaN)
				return "";
			else
				return monthName((int)month);
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string monthName_RENAME_ME(string d) 
        {
            return monthName(d);
        }

		/// <summary>
		/// Helper function for 
		///   string date:month-abbreviation()
		/// </summary>
		/// <returns>The abbreviated name of the current month</returns>        
		private string monthAbbreviation(int month)
		{
            if (month < 1 || month > monthAbbrevs.Length)
                return String.Empty;
			return monthAbbrevs[month-1];
		}

		/// <summary>
		/// Implements the following function
		///   string date:month-abbreviation()
		/// </summary>
		/// <returns>The abbreviated name of the current month</returns>        
		public string monthAbbreviation()
		{
			return monthAbbreviation((int)monthInYear());
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string monthAbbreviation_RENAME_ME() 
        {
            return monthAbbreviation();
        }

		/// <summary>
		/// Implements the following function
		///   string date:month-abbreviation(string)
		/// </summary>
		/// <returns>The abbreviated name of the month of the specified date or the empty string if the 
		/// date is invalid</returns>
		/// <remarks>Does not support dates in the format of the xs:yearMonth or 
		/// xs:gYear types</remarks>        
		public string monthAbbreviation(string d)
		{
			double month = monthInYear(d);
			if (month == System.Double.NaN)
				return "";
			else
				return monthAbbreviation((int)month);
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string monthAbbreviation_RENAME_ME(string d) 
        {
            return monthAbbreviation(d);
        }

		/// <summary>
		/// Implements the following function
		///   string date:format-date(string, string)
		/// </summary>
		/// <param name="d">The date to format</param>
		/// <param name="format">One of the format strings understood by the 
		/// Java 1.1 SimpleDateFormat method:
		/// 
		///  Symbol   Meaning                 Presentation        Example
		///------   -------                 ------------        -------
		///G        era designator          (Text)              AD
		///y        year                    (Number)            1996
		///M        month in year           (Text &amp; Number)     July &amp; 07
		///d        day in month            (Number)            10
		///h        hour in am/pm (1~12)    (Number)            12
		///H        hour in day (0~23)      (Number)            0
		///m        minute in hour          (Number)            30
		///s        second in minute        (Number)            55
		///S        millisecond             (Number)            978
		///E        day in week             (Text)              Tuesday
		///D        day in year             (Number)            189
		///F        day of week in month    (Number)            2 (2nd Wed in July)
		///w        week in year            (Number)            27
		///W        week in month           (Number)            2
		///a        am/pm marker            (Text)              PM
		///k        hour in day (1~24)      (Number)            24
		///K        hour in am/pm (0~11)    (Number)            0
		///z        time zone               (Text)              Pacific Standard Time
		///'        escape for text         (Delimiter)
		///''       single quote            (Literal)           '
		///</param>
		/// <returns>The formated date</returns>        
		public string formatDate(string d, string format)
		{
			try
			{
				ExsltDateTime oDate = ExsltDateTimeFactory.ParseDateTime(d);
				StringBuilder retString = new StringBuilder("");

				for (int i=0; i < format.Length;)
				{
					int s = i;
					switch(format[i])
					{
						case 'G'://        era designator          (Text)              AD
							while (i < format.Length && format[i]=='G'){i++;}

							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								if (oDate.d.Year < 0)
								{
									retString.Append("BC");
								}
								else
								{
									retString.Append("AD");
								}
							}
							break;

						case 'y'://        year                    (Number)            1996
							while (i < format.Length && format[i]=='y'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								if (i-s == 2)
								{
									retString.Append((oDate.d.Year % 100).ToString().PadLeft(i-s, '0'));

								}
								else
								{
									retString.Append(oDate.d.Year.ToString().PadLeft(i-s, '0'));
								}
							}
							break;
						case 'M'://        month in year           (Text &amp; Number)     July &amp; 07
							while (i < format.Length && format[i]=='M'){i++;}

							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(Month)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(MonthDay)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								if (i-s <= 2)
									retString.Append(oDate.d.Month.ToString().PadLeft(i-s, '0'));
								else if (i-s == 3)
									retString.Append(monthAbbreviation(oDate.d.Month));
								else
									retString.Append(monthName(oDate.d.Month));
							}
							break;
						case 'd'://        day in month            (Number)            10
							while (i < format.Length && format[i]=='d'){i++;}

							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(MonthDay)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(Day)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(oDate.d.Day.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'h'://        hour in am/pm (1~12)    (Number)            12
							while (i < format.Length && format[i]=='h'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								int hour = oDate.d.Hour % 12;
								if (0 == hour)
									hour = 12;
								retString.Append(hour.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'H'://        hour in day (0~23)      (Number)            0
							while (i < format.Length && format[i]=='H'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(oDate.d.Hour.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'm'://        minute in hour          (Number)            30
							while (i < format.Length && format[i]=='m'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(oDate.d.Minute.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 's'://        second in minute        (Number)            55
							while (i < format.Length && format[i]=='s'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(oDate.d.Second.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'S'://        millisecond             (Number)            978
							while (i < format.Length && format[i]=='S'){i++;}
                            if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) ||
                                Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
                                Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
                                Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
                                Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
                            {                                
                                retString.Append(oDate.d.Millisecond.ToString().PadLeft(i-s, '0'));
                            }							
							break;
						case 'E'://        day in week             (Text)              Tuesday
							while (i < format.Length && format[i]=='E'){i++;}

							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								if (i-s <= 3)
								{
									retString.Append(dayAbbreviation((int)oDate.d.DayOfWeek));
								}
								else
								{
									retString.Append(dayName((int)oDate.d.DayOfWeek));
								}
							}
							break;
						case 'D'://        day in year             (Number)            189
							while (i < format.Length && format[i]=='D'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(oDate.d.DayOfYear.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'F'://        day of week in month    (Number)            2 (2nd Wed in July)
							while (i < format.Length && format[i]=='F'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(MonthDay)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(Day)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(dayOfWeekInMonth(oDate.d.Day).ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'w'://        week in year            (Number)            27
							while (i < format.Length && format[i]=='w'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(weekInYear(oDate.d));
							}
							break;
						case 'W'://        week in month           (Number)            2
							while (i < format.Length && format[i]=='W'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								retString.Append(weekInMonth(oDate.d));
							}
							break;
						case 'a'://        am/pm marker            (Text)              PM
							while (i < format.Length && format[i]=='a'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								if (oDate.d.Hour < 12)
									retString.Append("AM");
								else
									retString.Append("PM");
							}
							break;
						case 'k'://        hour in day (1~24)      (Number)            24
							while (i < format.Length && format[i]=='k'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								int hour = oDate.d.Hour + 1;
								retString.Append(hour.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'K'://        hour in am/pm (0~11)    (Number)            0
							while (i < format.Length && format[i]=='K'){i++;}
							if (Object.ReferenceEquals(oDate.GetType(), typeof(DateTimeTZ)) || 
								Object.ReferenceEquals(oDate.GetType(), typeof(DateTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearMonth)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(TimeTZ)) ||
								Object.ReferenceEquals(oDate.GetType(), typeof(YearTZ)))
							{
								int hour = oDate.d.Hour % 12;
								retString.Append(hour.ToString().PadLeft(i-s, '0'));
							}
							break;
						case 'z'://        time zone               (Text)              Pacific Standard Time
							while (i < format.Length && format[i]=='z'){i++;}
							//
							// BUGBUG: Need to convert to full timezone names or timezone abbrevs
							// if they are available.  Now cheating by using GMT offsets.
							retString.Append(oDate.GetGMTOffsetTimeZone());
							break;
						case 'Z'://			rfc 822 time zone
							while (i < format.Length && format[i]=='Z'){i++;}
							retString.Append(oDate.Get822TimeZone());
							break;
						case '\''://        escape for text         (Delimiter)
							if (i < format.Length && format[i+1] == '\'')
							{
								i++;
								while (i < format.Length && format[i]=='\''){i++;}
								retString.Append("'");
							}
							else
							{
								i++;
								while (i < format.Length && format[i]!='\'' && i <= format.Length){retString.Append(format.Substring(i++, 1));}
								if (i >= format.Length)return "";
								i++;
							}
							break;
						default:
							retString.Append(format[i]);
							i++;
							break;
					}
				}
	
				return retString.ToString();
			}

		
			catch(FormatException)
			{
				return ""; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string formatDate_RENAME_ME(string d, string format) 
        {
            return formatDate(d, format);
        }


		/// <summary>
		/// Implements the following function
		///   string date:parse-date(string, string)
		/// BUGBUG: should use Java formatting strings, not Windows.
		/// </summary>
		/// <param name="d">The date to parse</param>
		/// <param name="format">One of the format strings understood by the 
		/// DateTime.ToString(string) method.</param>
		/// <returns>The parsed date</returns>        
		public string parseDate(string d, string format){
			try{
				DateTime date = DateTime.ParseExact(d, format, CultureInfo.CurrentCulture); 
				return XmlConvert.ToString(date, XmlDateTimeSerializationMode.RoundtripKind);
			}catch(FormatException){
				return ""; 
			}
		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string parseDate_RENAME_ME(string d, string format) 
        {
            return parseDate(d, format);
        }
	
		/// <summary>
		/// Implements the following function 
		///    string:date:difference(string, string)
		/// </summary>
		/// <param name="start">The start date</param>
		/// <param name="end">The end date</param>
		/// <returns>A positive difference if start is before end otherwise a negative
		/// difference. The difference is in the ISO 8601 date difference format as either
		/// [-]P[yY][mM]
		/// or
		/// [-]P[dD][T][hH][mM][sS]
		/// P means a difference and is required
		/// At least one of Y M D H M S is required
		/// If a higher order component is 0, it is suppressed (for example, if there are 0 years and
		/// 1 month, then Y is suppressed.  If there are 1 years and 0 months, M is not suppressed)
		/// If the input format is yyyy-MM or yyyy, then the output is [-]P[yY][mM]
		/// If the input format includes days, hours, minutes or seconds, then the output is
		/// [-]P[dD][T][hH][mM][sS].
		/// If there H M and S are all 0, the T is suppressed.
		/// </returns>        
		public string difference(string start, string end)
		{		
			try
			{
				ExsltDateTime startdate = ExsltDateTimeFactory.ParseDate(start); 
				ExsltDateTime enddate   = ExsltDateTimeFactory.ParseDate(end); 

				// The rules are pretty tricky.  basically, interpret both strings as the least-
				// specific format
				if (Object.ReferenceEquals(startdate.GetType(), typeof(YearTZ)) || 
					Object.ReferenceEquals(enddate.GetType(), typeof(YearTZ)))
				{
					StringBuilder retString = new StringBuilder("");

					int	yearDiff = enddate.d.Year - startdate.d.Year;

					if (yearDiff < 0)
						retString.Append('-');

					retString.Append('P');
					retString.Append(Math.Abs(yearDiff));
					retString.Append('Y');

					return retString.ToString();
				}					
				else if (Object.ReferenceEquals(startdate.GetType(), typeof(YearMonth)) || 
					Object.ReferenceEquals(enddate.GetType(), typeof(YearMonth)))
				{
					StringBuilder retString = new StringBuilder("");

					int	yearDiff = enddate.d.Year - startdate.d.Year;
					int monthDiff = enddate.d.Month - startdate.d.Month;

					// Borrow from the year if necessary
					if ((yearDiff > 0) && (Math.Sign(monthDiff) == -1))
					{
						yearDiff--;
						monthDiff += 12;
					} 
					else if ((yearDiff < 0) && (Math.Sign(monthDiff) == 1))
					{
						yearDiff++;
						monthDiff -= 12;
					}


					if ((yearDiff < 0) || ((yearDiff == 0) && (monthDiff < 0)))
					{
						retString.Append('-');						
					}
					retString.Append('P');
					if (yearDiff != 0)
					{
						retString.Append(Math.Abs(yearDiff));
						retString.Append('Y');
					}
					retString.Append(Math.Abs(monthDiff));
					retString.Append('M');
	
					return retString.ToString();
				}
				else
				{
					// Simulate casting to the most truncated format.  i.e. if one 
					// Arg is DateTZ and the other is DateTimeTZ, get rid of the time
					// for both.
					if (Object.ReferenceEquals(startdate.GetType(), typeof(DateTZ)) || 
						Object.ReferenceEquals(enddate.GetType(), typeof(DateTZ)))
					{
						startdate = new DateTZ(startdate.d.ToString("yyyy-MM-dd"));
						enddate = new DateTZ(enddate.d.ToString("yyyy-MM-dd"));
					}

					TimeSpan ts = enddate.d.Subtract(startdate.d);
					return XmlConvert.ToString(ts);
				}
			}
			catch(FormatException)
			{
				return ""; 
			}
		}

		/// <summary>
		/// Implements the following function
		///    date:add(string, string)
		/// </summary>
		/// <param name="datetime">An ISO8601 date/time</param>
		/// <param name="duration">the duration to add</param>
		/// <returns>The new time</returns>        
		public string add(string datetime, string duration)
		{			
			try
			{
				ExsltDateTime date = ExsltDateTimeFactory.ParseDate(datetime); 
				//TimeSpan timespan = System.Xml.XmlConvert.ToTimeSpan(duration); 

				Regex durationRE = new Regex(
					@"^(-)?" +				// May begin with a - sign
					@"P" +					// Must contain P as first or 2nd char
					@"(?=\d+|(?:T\d+))" +		// Must contain at least one digit after P or after PT
					@"(?:(\d+)Y)?" +		// May contain digits plus Y for year
					@"(?:(\d+)M)?" +		// May contain digits plus M for month
					@"(?:(\d+)D)?" +		// May contain digits plus D for day
					@"(?=T\d+)?" +			// If there is a T here, must be digits afterwards
					@"T?" +					// May contain a T
					@"(?:(\d+)H)?" +		// May contain digits plus H for hours
					@"(?:(\d+)M)?" +		// May contain digits plus M for minutes
					@"(?:(\d+)S)?" +		// May contain digits plus S for seconds
					@"$", 
					RegexOptions.IgnoreCase | RegexOptions.Singleline
					);

				Match m = durationRE.Match(duration);

				int negation = 1, years = 0, months = 0, days = 0,
					hours = 0, minutes = 0, seconds = 0;

				if (m.Success)
				{
					//date.d = date.d.Add(timespan);
					// According to the XML datetime spec at 
					// http://www.w3.org/TR/xmlschema-2/#adding-durations-to-dateTimes, 
					// we need to first add the year/month part, then we can add the 
					// day/hour/minute/second part

					if (CultureInfo.InvariantCulture.CompareInfo.Compare(m.Groups[1].Value, "-") == 0)
						negation = -1;

					if (m.Groups[2].Length > 0)
						years = negation * int.Parse(m.Groups[2].Value);

					if (m.Groups[3].Length > 0)
						months = negation * int.Parse(m.Groups[3].Value);

					if (m.Groups[4].Length > 0)
						days = negation * int.Parse(m.Groups[4].Value);

					if (m.Groups[5].Length > 0)
						hours = negation * int.Parse(m.Groups[5].Value);

					if (m.Groups[6].Length > 0)
						minutes = negation * int.Parse(m.Groups[6].Value);

					if (m.Groups[7].Length > 0)
						seconds = negation * int.Parse(m.Groups[7].Value);

					date.d = date.d.AddYears(years);
					date.d = date.d.AddMonths(months);
					date.d = date.d.AddDays(days);
					date.d = date.d.AddHours(hours);
					date.d = date.d.AddMinutes(minutes);
					date.d = date.d.AddSeconds(seconds);

					// we should return the same format as passed in

					// return date.ToString("yyyy-MM-dd\"T\"HH:mm:ss");			
					return date.ToString();
				}
				else
				{
					return "";
				}
			}
			catch(FormatException)	
			{
				return ""; 
			}
		}


		/// <summary>
		/// Implements the following function
		///    string:date:add-duration(string, string)
		/// </summary>
        /// <param name="duration1">Initial duration</param>
        /// <param name="duration2">the duration to add</param>
		/// <returns>The new time</returns>        
		public string addDuration(string duration1, string duration2)
		{			
			try
			{
				TimeSpan timespan1 = XmlConvert.ToTimeSpan(duration1);
				TimeSpan timespan2 = XmlConvert.ToTimeSpan(duration2); 
				return XmlConvert.ToString(timespan1.Add(timespan2));
			}
			catch(FormatException)
			{
				return ""; 
			}

		}
		
        /// <summary>
        /// This wrapper method will be renamed during custom build 
        /// to provide conformant EXSLT function name.
        /// </summary>
        public string addDuration_RENAME_ME(string duration1, string duration2) 
        {
            return addDuration(duration1, duration2);
        }

		/// <summary>
		/// Helper method for date:seconds() that takes an ExsltDateTime. 
		/// </summary>
		/// <param name="d"></param>
		/// <returns>difference in seconds between the specified date and the
		/// epoch (1970-01-01T00:00:00Z)</returns>
		private double seconds(ExsltDateTime d)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0,0,0,0, CultureInfo.InvariantCulture.Calendar);
			return d.ToUniversalTime().Subtract(epoch).TotalSeconds;
		}

		/// <summary>
		/// Implements the following function
		///		number date:seconds()
		/// </summary>
		/// <returns>The amount of seconds since the epoch (1970-01-01T00:00:00Z)</returns>        
		public double seconds()
		{		 
			return seconds(new DateTimeTZ());
		}


		/// <summary>
		/// Implements the following function
		///		number date:seconds(string)
		/// </summary>
		/// <returns>If date passed in, the amount of seconds between the specified date and the 
		/// epoch (1970-01-01T00:00:00Z).  If timespan passed in, returns the number of seconds
		/// in the timespan.</returns>
		public double seconds(string datetime)
		{		 
			try
			{
				return seconds(ExsltDateTimeFactory.ParseDate(datetime));
			}
			catch(FormatException){ ; } //might be a duration

			try
			{
				TimeSpan duration = XmlConvert.ToTimeSpan(datetime); 
				return duration.TotalSeconds;
			}
			catch(FormatException)
			{
				return System.Double.NaN;
			}
		}

		/// <summary>
		/// Implements the following function 
		///		string date:sum(node-set)
		/// </summary>
		/// <param name="iterator">Nodeset of timespans</param>
		/// <returns>The sum of the timespans within a node set.</returns>        
		public string sum(XPathNodeIterator iterator)
		{
			
			TimeSpan sum = new TimeSpan(0,0,0,0); 
 
			if(iterator.Count == 0)
			{
				return ""; 
			}

			try
			{ 
				while(iterator.MoveNext())
				{
					sum = XmlConvert.ToTimeSpan(iterator.Current.Value).Add(sum);
				}
				
			}
			catch(FormatException)
			{
				return ""; 
			}

			return XmlConvert.ToString(sum) ; //XmlConvert.ToString(sum);
		}

		/// <summary>
		/// Implements the following function 
		///    string date:duration()
		/// </summary>
		/// <returns>seconds since the beginning of the epoch until now</returns>        
		public string duration()
		{
			return duration(seconds()); 
		}

		/// <summary>
		/// Implements the following function 
		///    string date:duration(number)
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>        
		public string duration(double seconds)
		{
			return XmlConvert.ToString(TimeSpan.FromSeconds(seconds)); 
		}	
	}
}
