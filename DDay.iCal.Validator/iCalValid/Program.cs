using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using DDay.iCal.Validator.RFC2445;
using DDay.iCal.Validator.Xml;
using DDay.Util;
using DDay.iCal.Validator.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace DDay.iCal.Validator
{
    class Program
    {
        static CommandLineArgument _FileArgument = new CommandLineArgument(new string[] { "f", "file" });
        static CommandLineArgument _UriArgument = new CommandLineArgument(new string[] { "uri" });
        static CommandLineArgument _ValidatorArgument = new CommandLineArgument(new string[] { "v", "validator" });
        static CommandLineArgument _UsernameArgument = new CommandLineArgument(new string[] { "u", "username" });
        static CommandLineArgument _PasswordArgument = new CommandLineArgument(new string[] { "p", "password" });
        static CommandLineArgument _TestArgument = new CommandLineArgument(new string[] { "t", "test", "selftest" });
        static CommandLineArgument _LanguageArgument = new CommandLineArgument(new string[] { "l", "language" });
        static CommandLineArgument _SchemaValidationArgument = new CommandLineArgument(new string[] { "s", "schema", "schemaValidate", "schemaValidation" });

        static CommandLineArgumentList _Arguments;

        static void Main(string[] args)
        {
            try
            {
                _Arguments = new CommandLineArgumentList(args, StringComparison.CurrentCultureIgnoreCase);

                IXmlDocumentProvider docProvider = null;

                // Initialize our xml document provider
                if (File.Exists("icalvalidSchema.zip"))
                    docProvider = new XmlDocumentZipExtractor("icalvalidSchema.zip");
                else if (Directory.Exists("icalvalidSchema"))
                    docProvider = new LocalXmlDocumentProvider("icalvalidSchema");
                else
                    throw new Exception("A valid schema directory or zip file could not be located!");
                
                bool foundLanguage = false;

                // Determine what language we're using...
                if (_Arguments.Contains(_LanguageArgument))
                {
                    // Initialize our resource manager with a user-defined culture
                    foundLanguage = ResourceManager.Initialize(docProvider, _Arguments[_LanguageArgument].Value);
                }
                else
                {
                    // Initialize our resource manager with our current culture
                    foundLanguage = ResourceManager.Initialize(docProvider, false);
                }

                if (!foundLanguage)
                {
                    // Force initialization to English
                    ResourceManager.Initialize(docProvider, true);
                    Console.WriteLine("Could not find the selected language; using English instead...");
                }                

                // Load some rulesets
                XmlValidationRulesetLoader loader = new XmlValidationRulesetLoader(docProvider);
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

                if (_Arguments.Contains(_SchemaValidationArgument))
                {
                    SchemaTest(docProvider);
                }
                else
                {
                    // Determine whether we are performing a self test, or
                    // validating an iCalendar file...
                    if (_Arguments.Contains(_TestArgument))
                    {
                        SelfTest(docProvider, selectedRuleset);
                    }
                    else
                    {
                        ValidateFile(docProvider, selectedRuleset);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void SchemaTest(IXmlDocumentProvider docProvider)
        {
            Console.WriteLine("Performing schema validation against XML files contained in icalvalidSchema...");
            try
            {
                List<string> filesToValidate = new List<string>();
                List<string> xsdFiles = new List<string>();

                foreach (string path in docProvider)
                {
                    if (path.EndsWith(".xml"))
                        filesToValidate.Add(path);
                    else if (path.EndsWith(".xsd"))
                        xsdFiles.Add(path);
                }

                List<StringReader> xsdContents = new List<StringReader>();
                foreach (string xsdFilepath in xsdFiles)
                    xsdContents.Add(new StringReader(docProvider.LoadXml(xsdFilepath)));

                XmlSchemaSet sc = new XmlSchemaSet();
                foreach (StringReader sr in xsdContents)
                    sc.Add(XmlSchema.Read(sr, null));
                                
                foreach (string xmlPath in filesToValidate)
                {
                    Console.Write(Environment.NewLine + "Validating '" + xmlPath + "'...");
                    string xmlText = docProvider.LoadXml(xmlPath);
                    
                    int errors = 0;
                    bool needsNewline = true;
                    XmlReaderSettings readSettings = new XmlReaderSettings();
                    readSettings.ValidationType = ValidationType.Schema;
                    readSettings.Schemas.Add(sc);
                    readSettings.ConformanceLevel = ConformanceLevel.Document;
                    readSettings.ValidationEventHandler += new ValidationEventHandler(
                        delegate(object s, ValidationEventArgs e)
                        {
                            errors++;
                            if (needsNewline)
                            {
                                Console.WriteLine();
                                needsNewline = false;
                            }
                            Console.Write(Environment.NewLine + e.Message + " (line " + e.Exception.LineNumber + " col " + e.Exception.LinePosition + ")");
                        }
                    );

                    XmlReader reader = XmlReader.Create(new StringReader(xmlText), readSettings);
                    while (reader.Read()) { }
                    reader.Close();

                    if (errors > 0)
                    {
                        Console.WriteLine(
                            Environment.NewLine +
                            Environment.NewLine +
                            (errors == 1 ? "There was 1 error" : "There were " + errors + " errors") +
                            " in '" + xmlPath + "'; please correct before using this file.");
                    }
                    else
                    {
                        Console.Write("OK.");
                    }
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine();
        }

        static void SelfTest(IXmlDocumentProvider docProvider, IValidationRuleset selectedRuleset)
        {
            IValidationSerializer serializer = new XmlValidationSerializer(docProvider);
            if (selectedRuleset != null)
            {
                serializer.Ruleset = selectedRuleset;

                RulesetValidator validator = new RulesetValidator(selectedRuleset);

                Console.WriteLine(string.Format(
                    ResourceManager.GetString("performingSelfTest"),
                    selectedRuleset.Description));

                serializer.TestResults = validator.Test();                
                
                //if (results != null &&
                //    results.Length > 0)
                //{
                //    int numTestsExpected = validator.Tests.Length;
                //    if (!object.Equals(numTestsExpected, results.Length))
                //    {
                //        Console.WriteLine(string.Format(
                //            ResourceManager.GetString("notAllTestsPerformed"),                            
                //            numTestsExpected,
                //            results.Length,
                //            numTestsExpected - results.Length
                //        ));
                //    }

                //    int passed = 0;
                //    foreach (ITestResult result in results)
                //    {
                //        if (BoolUtil.IsTrue(result.Passed))
                //            passed++;

                //        Console.WriteLine(result.ToString());
                //    }

                //    Console.WriteLine(string.Format(
                //        ResourceManager.GetString("passVsFail"),
                //        passed,
                //        results.Length,
                //        string.Format("{0:0.0}", ((double)passed / (double)results.Length) * 100)
                //    ));
                //}
                //else
                //{
                //    Console.WriteLine(ResourceManager.GetString("noTestsPerformed"));
                //}                
            }
            /*else
            {
                Console.WriteLine(ResourceManager.GetString("noValidationRuleset"));
            }*/

            FileStream fs = new FileStream(@"c:\test.xml", FileMode.Create, FileAccess.Write);
            try
            {
                serializer.Serialize(fs, UTF8Encoding.Default);
            }
            finally
            {
                fs.Close();
            }
        }

        static void ValidateFile(IXmlDocumentProvider docProvider, IValidationRuleset selectedRuleset)
        {
            IValidationSerializer serializer = new XmlValidationSerializer(docProvider);

            bool needsMoreArguments = false;

            string iCalText = null;
            if (selectedRuleset != null)
            {
                serializer.Ruleset = selectedRuleset;

                if (_Arguments.Contains(_FileArgument))
                {
                    // Load the calendar from a local file
                    Console.Write(ResourceManager.GetString("loadingCalendar"));
                    FileStream fs = new FileStream(_Arguments[_FileArgument].Value, FileMode.Open, FileAccess.Read);
                    if (fs != null)
                    {
                        StreamReader sr = new StreamReader(fs);
                        iCalText = sr.ReadToEnd();
                        sr.Close();
                    }
                    Console.WriteLine(ResourceManager.GetString("Done"));
                }
                else if (_Arguments.Contains(_UriArgument))
                {
                    // Load the calendar from a Uri
                    Console.Write(ResourceManager.GetString("loadingCalendar"));
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
                    Console.WriteLine(ResourceManager.GetString("Done"));
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
                        throw new Exception(ResourceManager.GetString("calendarNotFound"));
                    }
                    else
                    {
                        RulesetValidator rulesetValidator = new RulesetValidator(selectedRuleset, iCalText);

                        if (rulesetValidator != null)
                        {
                            Console.WriteLine(string.Format(
                                ResourceManager.GetString("validatingCalendar"),
                                selectedRuleset.Description
                            ));

                            serializer.ValidationResults = rulesetValidator.Validate();
                            //IValidationResult[] results = rulesetValidator.Validate();
                            //if (results != null &&
                            //    results.Length > 0)
                            //{
                            //    foreach (IValidationError error in results)
                            //        Console.WriteLine(error.ToString());
                            //}
                            //else
                            //{
                            //    Console.WriteLine(ResourceManager.GetString("calendarValid"));
                            //}
                        }
                    }
                }
            }
            //else
            //{
            //    Console.WriteLine(ResourceManager.GetString("noValidationRuleset"));
            //}

            FileStream sfs = new FileStream(@"c:\test.xml", FileMode.Create, FileAccess.Write);
            try
            {
                serializer.Serialize(sfs, UTF8Encoding.Default);
            }
            finally
            {
                sfs.Close();
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
            Console.WriteLine("/l:<language>  | The language to use when validating (i.e. en-US, es-MX, etc.)");
            Console.WriteLine("/s             | Performs schema validation against the icalvalidSchema.");
            Console.WriteLine("               | NOTE: most settings are ignored when using /s");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine();
            Console.WriteLine("icalvalid /t");
            Console.WriteLine(@"icalvalid /f:c:\calendar.ics");
            Console.WriteLine(@"icalvalid /uri:http://www.someuri.com/calendar.ics");
            Console.WriteLine(@"icalvalid /uri:http://www.someuri.com/calendar.ics /u:myuser /p:mypassword /l:es-MX");
        }
    }
}
