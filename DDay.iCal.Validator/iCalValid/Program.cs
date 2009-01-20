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
        static CommandLineArgument _UriArgument = new CommandLineArgument(new string[] { "uri" });
        static CommandLineArgument _ValidatorArgument = new CommandLineArgument(new string[] { "v", "validator" });
        static CommandLineArgument _UsernameArgument = new CommandLineArgument(new string[] { "u", "username" });
        static CommandLineArgument _PasswordArgument = new CommandLineArgument(new string[] { "p", "password" });
        static CommandLineArgument _TestArgument = new CommandLineArgument(new string[] { "t", "test" });

        static CommandLineArgumentList _Arguments;

        static void Main(string[] args)
        {
            try
            {
                _Arguments = new CommandLineArgumentList(args, StringComparison.CurrentCultureIgnoreCase);

                // Initialize our xml document provider
                XmlDocumentZipExtractor ze = new XmlDocumentZipExtractor("icalvalidSchema.zip");
                // Initialize our resource manager using the provider
                ResourceManager.Initialize(ze);

                // Load some rulesets
                XmlValidationRulesetLoader loader = new XmlValidationRulesetLoader(ze);
                List<IValidationRuleset> rulesets = new List<IValidationRuleset>(loader.Load());

                // Determine a validation ruleset to use
                string validatorName = "Strict2_0";
                if (_Arguments.Contains(_ValidatorArgument))
                    validatorName = _Arguments[_ValidatorArgument].Value;

                // Select the ruleset
                IValidationRuleset selectedRuleset = rulesets.Find(
                    delegate(IValidationRuleset rs)
                    {
                        return string.Equals(rs.Name, validatorName, StringComparison.CurrentCultureIgnoreCase);
                    }
                );

                // Determine whether we are performing a self test, or
                // validating an iCalendar file...
                if (_Arguments.Contains(_TestArgument))
                {
                    SelfTest(selectedRuleset);
                }
                else
                {
                    ValidateFile(selectedRuleset);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void SelfTest(IValidationRuleset selectedRuleset)
        {
            if (selectedRuleset != null)
            {
                RulesetValidator validator = new RulesetValidator(selectedRuleset);
                
                Console.WriteLine("Performing self test...");
                ICalendarTestResult[] results = validator.Test();
                if (results != null &&
                    results.Length > 0)
                {
                    int numTestsExpected = validator.Tests.Length;
                    if (!object.Equals(numTestsExpected, results.Length))
                    {
                        Console.WriteLine(string.Format("There were {0} tests, and only {1} were run; {2} tests did not run.",
                            numTestsExpected,
                            results.Length,
                            numTestsExpected - results.Length
                        ));
                    }
                    
                    foreach (ICalendarTestResult result in results)
                        Console.WriteLine(result.ToString());
                }
                else
                {
                    Console.WriteLine("No tests were performed!");
                }                
            }
            else
            {
                Console.WriteLine("A validation ruleset could not be determined!");
            }
        }

        static void ValidateFile(IValidationRuleset selectedRuleset)
        {
            bool needsMoreArguments = false;

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
                        RulesetValidator rulesetValidator = new RulesetValidator(selectedRuleset, iCalText);

                        if (rulesetValidator != null)
                        {
                            Console.WriteLine("Validating calendar using '" + selectedRuleset.Description + "' ruleset...");

                            IValidationError[] errors = rulesetValidator.Validate();
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

        static public void WriteDescription()
        {
            Console.WriteLine();
            Console.WriteLine("icalvalid - iCalendar validator");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine("Usage: iCalValid.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine("============================================================================");
            Console.WriteLine("/f:<filename>  | Loads an iCalendar from a file, where <filename> is");
            Console.WriteLine("               | the file path of the iCalendar file to validate.");
            Console.WriteLine("/uri:<uri>     | Loads an iCalendar from a uri, where <uri> is the");
            Console.WriteLine("               | Uri of the iCalendar file to validate.");
            Console.WriteLine("/u:<username>  | The username to use when retrieving a calendar via Uri.");
            Console.WriteLine("/p:<password>  | The password to use when retrieving a calendar via Uri.");
            Console.WriteLine("/t             | Performs a self test on the validator to ensure it is");
            Console.WriteLine("               | properly validating calendar files.");
            Console.WriteLine("               | NOTE: /f, /uri, /u, and /p are ignored when using /t.");
            Console.WriteLine("/v:<validator> | The validator to use.  Possible values are:");
            Console.WriteLine("               |       Strict2_0");
            Console.WriteLine("               |       Other validator names");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine();
            Console.WriteLine("icalvalid /t");
            Console.WriteLine(@"icalvalid /f:c:\calendar.ics");
            Console.WriteLine(@"icalvalid /uri:http://www.someuri.com/calendar.ics");
            Console.WriteLine(@"icalvalid /uri:http://www.someuri.com/calendar.ics /u:myuser /p:mypassword");
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
