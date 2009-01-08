using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DDay.Util;
using DDay.iCal.Validator.RFC2445;
using System.Reflection;
using System.Net;

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

                Type validatorType = typeof(Strict2_0Validator);
                if (_Arguments.Contains(_ValidatorArgument))
                {
                    string validatorTypeString = _Arguments[_ValidatorArgument].Value;
                    validatorType = Type.GetType(validatorTypeString, false);
                    if (validatorType == null)
                        validatorType = Type.GetType("DDay.iCal.Validator.RFC2445." + validatorTypeString + ", DDay.iCal.Validator", false);

                    if (validatorType == null)
                        throw new Exception("A validator of type '" + validatorTypeString + "' could not be determined!");
                    else if (!typeof(IValidator).IsAssignableFrom(validatorType))
                        throw new Exception("The validator type '" + validatorTypeString + "' was not a validator type!");
                }

                string iCalText = null;

                if (_Arguments.Contains(_FileArgument))
                {
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
                        ConstructorInfo ci = validatorType.GetConstructor(new Type[] { typeof(string) });
                        if (ci != null)
                        {
                            IValidator validator = ci.Invoke(new object[] { iCalText }) as IValidator; 
                            if (validator != null)
                            {
                                Console.WriteLine("Validating calendar...");

                                IValidationError[] errors = validator.Validate();
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
                        // FIXME: throw an exception
                    }
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
