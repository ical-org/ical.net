using System;
using System.IO;
using System.Net;
using DDay.Util;
using DDay.iCal.Validator.Xml;
using System.Collections.Generic;
using DDay.iCal.Validator.RFC2445;

namespace DDay.iCal.Validator
{
    class Program
    {
        static CommandLineArgument _FileArgument = new CommandLineArgument(new string[] { "f", "file" });
        static CommandLineArgument _UriArgument = new CommandLineArgument(new string[] { "u", "uri" });
        static CommandLineArgument _ValidatorArgument = new CommandLineArgument(new string[] { "v", "validator" });
        static CommandLineArgument _UsernameArgument = new CommandLineArgument(new string[] { "un", "username" });
        static CommandLineArgument _PasswordArgument = new CommandLineArgument(new string[] { "pw", "password" });

        static CommandLineArgumentList _Arguments;

        static void Main(string[] args)
        {
            try
            {
                bool needsMoreArguments = false;

                _Arguments = new CommandLineArgumentList(args, StringComparison.CurrentCultureIgnoreCase);

                // Initialize our xml document provider
                XmlDocumentZipExtractor ze = new XmlDocumentZipExtractor("icalvalidSchema.zip");
                // Initialize our resource manager using the provider
                ResourceManager.Initialize(ze);

                // Load some rulesets
                XmlValidationRulesetLoader loader = new XmlValidationRulesetLoader(ze);
                List<IValidationRuleset> rulesets = new List<IValidationRuleset>(loader.Load());

                // Auto-select a ruleset
                // FIXME: allow the user to choose a ruleset
                IValidationRuleset selectedRuleset = rulesets.Find(
                    delegate(IValidationRuleset rs)
                    {
                        return string.Equals(rs.Name, "Strict2_0", StringComparison.CurrentCultureIgnoreCase);
                    }
                );

                // If we couldn't find the Strict 2.0 validation, then get the first
                // one available!
                if (selectedRuleset == null && rulesets.Count > 0)
                    selectedRuleset = rulesets[0];

                string iCalText = null;
                if (selectedRuleset != null)
                {
                    if (_Arguments.Contains(_FileArgument))
                    {
                        // Load the calendar from a local file
                        Console.Write("Loading calendar...");
                        FileStream fs = new FileStream(_Arguments[_FileArgument].Value, FileMode.Open, FileAccess.Read);
                        if (fs != null)
                        {
                            StreamReader sr = new StreamReader(fs);
                            iCalText = sr.ReadToEnd();
                            sr.Close();
                        }
                        Console.WriteLine("done.");
                    }
                    else if (_Arguments.Contains(_UriArgument))
                    {
                        // Load the calendar from a Uri
                        Console.Write("Loading calendar...");
                        Uri uri = new Uri(_Arguments[_UriArgument].Value);
                        string
                            username = null,
                            password = null;

                        if (_Arguments.Contains(_UsernameArgument))
                        {
                            username = _Arguments[_UsernameArgument].Value;
                            if (_Arguments.Contains(_PasswordArgument))
                                password = _Arguments[_PasswordArgument].Value;
                        }

                        WebClient client = new WebClient();
                        if (username != null && password != null)
                            client.Credentials = new System.Net.NetworkCredential(username, password);

                        iCalText = client.DownloadString(uri);
                        Console.WriteLine("done.");
                    }
                    else
                    {
                        needsMoreArguments = true;
                    }

                    if (needsMoreArguments)
                    {
                        WriteDescription();
                    }
                    else
                    {
                        if (iCalText == null)
                        {
                            throw new Exception("The calendar could not be found!");
                        }
                        else
                        {
                            RFC2445Validator rfc2445Validator = new RFC2445Validator(selectedRuleset, iCalText);

                            if (rfc2445Validator != null)
                            {
                                Console.WriteLine("Validating calendar...");

                                IValidationError[] errors = rfc2445Validator.Validate();
                                if (errors != null &&
                                    errors.Length > 0)
                                {
                                    foreach (IValidationError error in errors)
                                        Console.WriteLine(error.ToString());
                                }
                                else
                                {
                                    Console.WriteLine("The calendar is valid!");
                                }
                            }                            
                        }
                    }
                }
                else
                {
                    Console.WriteLine("A validation ruleset could not be determined!");
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static public void WriteDescription()
        {
            Console.WriteLine("iCalValid - iCalendar validator");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine("Usage: iCalValid.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine("============================================================================");
            Console.WriteLine("/f:<filename>  | Loads an iCalendar from a file, where <filename> is");
            Console.WriteLine("               | the file path of the iCalendar file to validate.");
            Console.WriteLine("/u:<uri>       | Loads an iCalendar from a uri, where <uri> is the");
            Console.WriteLine("               | Uri of the iCalendar file to validate.");
            Console.WriteLine("/un:<username> | The username to use when retrieving a calendar via Uri.");
            Console.WriteLine("/pw:<password> | The password to use when retrieving a calendar via Uri.");
            Console.WriteLine("/v:<validator> | The validator to use.  Possible values are:");
            Console.WriteLine("               |       Strict2_0");
            Console.WriteLine("               |       Other fully-qualified validator typenames");
        }

        static public string GetCalendarText(string calendarFilename)
        {
            string s = null;

            FileStream fs = new FileStream(@"Calendars\" + calendarFilename, FileMode.Open, FileAccess.Read);
            if (fs != null)
            {
                StreamReader sr = new StreamReader(fs);
                s = sr.ReadToEnd();
                sr.Close();
            }

            return s;
        }
    }
}
