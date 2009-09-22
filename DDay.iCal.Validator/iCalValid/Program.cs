using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using DDay.iCal.Validator.RFC2445;
using DDay.iCal.Validator.Serialization;
using DDay.iCal.Validator.Xml;
using DDay.Util;

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
        static CommandLineArgument _OutputArgument = new CommandLineArgument(new string[] { "o", "output" });
        static CommandLineArgument _FormatArgument = new CommandLineArgument(new string[] { "format" });
        static CommandLineArgument _OutputFile = new CommandLineArgument(new string[] { "of", "outputFile" });

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

                IValidationRuleset selectedRuleset = null;
                bool successfulLoad = true;
                try
                {
                    // Setup the language to use for validation/tests
                    SetupLanguage(docProvider);
                    selectedRuleset = LoadRuleset(docProvider);
                }
                catch (ValidationRuleLoadException)
                {
                    successfulLoad = false;
                }

                if (_Arguments.Contains(_SchemaValidationArgument))
                {
                    SchemaTest(docProvider);
                    LanguageKeyValidation(docProvider);
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

        static void SetupLanguage(IXmlDocumentProvider docProvider)
        {
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
                Console.WriteLine("Could not find the selected language; using English instead...");

                // Force initialization to English
                ResourceManager.Initialize(docProvider, true);                
            }
        }

        static IValidationRuleset LoadRuleset(IXmlDocumentProvider docProvider)
        {
            // Load some rulesets
            XmlValidationRulesetLoader loader = new XmlValidationRulesetLoader(docProvider);
            List<IValidationRuleset> rulesets = new List<IValidationRuleset>(loader.Load());

            // Determine a validation ruleset to use
            string validatorName = "Strict_2_0";
            if (_Arguments.Contains(_ValidatorArgument))
                validatorName = _Arguments[_ValidatorArgument].Value;

            // Select the ruleset
            IValidationRuleset selectedRuleset = rulesets.Find(
                delegate(IValidationRuleset rs)
                {
                    return string.Equals(rs.Name, validatorName, StringComparison.CurrentCultureIgnoreCase);
                }
            );

            return selectedRuleset;
        }

        static void SetupStream(IValidationSerializer serializer, out Stream stream, out Encoding encoding)
        {            
            bool useConsole = true;
            bool unknown = false;

            if (_Arguments.Contains(_OutputArgument))
            {
                if (_Arguments[_OutputArgument].Value.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                    useConsole = false;
                else if (_Arguments[_OutputArgument].Value.Equals("console", StringComparison.InvariantCultureIgnoreCase) ||
                    _Arguments[_OutputArgument].Value.Equals("screen", StringComparison.InvariantCultureIgnoreCase) ||
                    _Arguments[_OutputArgument].Value.Equals("stdout", StringComparison.InvariantCultureIgnoreCase))
                    useConsole = true;
                else
                    unknown = true;
            }

            // FIXME: what to do with an unknown output?
            if (useConsole)
            {
                stream = Console.OpenStandardOutput();
                encoding = Console.OutputEncoding;
            }
            else
            {
                // Determine the filename for our output
                string filename = "output." + serializer.DefaultExtension;
                if (_Arguments.Contains(_OutputFile))
                {
                    filename = _Arguments[_OutputFile].Value;
                    if (object.Equals(filename, Path.GetFileNameWithoutExtension(filename)))
                        filename = filename + "." + serializer.DefaultExtension;
                }

                // FIXME: what if the file exists?
                stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
                encoding = serializer.DefaultEncoding;
            }
        }

        static IValidationSerializer GetSerializer(IXmlDocumentProvider docProvider)
        {
            string format = "Text";
            if (_Arguments.Contains(_FormatArgument))            
                format = _Arguments[_FormatArgument].Value;

            Type type = Type.GetType("DDay.iCal.Validator.Serialization." + format + "ValidationSerializer, DDay.iCal.Validator", false, true);
            if (type != null)
            {
                ConstructorInfo ci = type.GetConstructor(Type.EmptyTypes);
                if (ci != null)
                    return ci.Invoke(new object[0]) as IValidationSerializer;
                else
                {
                    ci = type.GetConstructor(new Type[] { typeof(IXmlDocumentProvider) });
                    if (ci != null)
                        return ci.Invoke(new object[] { docProvider }) as IValidationSerializer;
                }
            }

            return null;
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
                    try
                    {
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
                    catch
                    {
                        Console.Write("Error loading file.");
                    }
                }                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine();
        }

        static void LanguageKeyValidation(IXmlDocumentProvider docProvider)
        {
            Console.Write(Environment.NewLine + "Validating language files for missing entries...");
            try
            {
                List<string> languageFiles = new List<string>();
                foreach (string path in docProvider)
                {
                    if (path.Contains("languages"))
                        languageFiles.Add(path);                    
                }

                List<string> masterEntries = new List<string>();
                Dictionary<string, List<string>> entries = new Dictionary<string, List<string>>();
                foreach (string xmlPath in languageFiles)
                {
                    XmlDocument doc = docProvider.Load(xmlPath);
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);

                    string prefix = nsmgr.LookupPrefix("http://icalvalid.wikidot.com/validation");
                    if (string.IsNullOrEmpty(prefix))
                    {
                        prefix = "v";
                        nsmgr.AddNamespace("v", "http://icalvalid.wikidot.com/validation");
                    }

                    entries[xmlPath] = new List<string>();
                    foreach (XmlNode str in doc.SelectNodes("/" + prefix + ":language/" + prefix + ":string", nsmgr))
                    {
                        string name = str.Attributes["name"].Value;
                        entries[xmlPath].Add("string/" + name);
                        if (!masterEntries.Contains("string/" + name))
                            masterEntries.Add("string/" + name);
                    }
                    foreach (XmlNode str in doc.SelectNodes("/" + prefix + ":language/" + prefix + ":errors/" + prefix + ":error", nsmgr))
                    {
                        string name = str.Attributes["name"].Value;
                        entries[xmlPath].Add("error/" + name);
                        if (!masterEntries.Contains("error/" + name))
                            masterEntries.Add("error/" + name);
                    }
                    foreach (XmlNode str in doc.SelectNodes("/" + prefix + ":language/" + prefix + ":resolutions/" + prefix + ":resolution", nsmgr))
                    {
                        string name = str.Attributes["error"].Value;
                        entries[xmlPath].Add("resolution/" + name);
                        if (!masterEntries.Contains("resolution/" + name))
                            masterEntries.Add("resolution/" + name);
                    }
                }

                foreach (KeyValuePair<string, List<string>> kvp in entries)
                {
                    Console.Write(Environment.NewLine + "Validating '" + kvp.Key + "'...");
                    List<string> missingEntries = new List<string>();
                    foreach (string masterEntry in masterEntries)
                    {
                        if (!kvp.Value.Contains(masterEntry))
                            missingEntries.Add(masterEntry);
                    }

                    if (missingEntries.Count > 0)
                    {
                        Console.Write("Missing " + missingEntries.Count + " entries!");
                        foreach (string me in missingEntries)
                            Console.Write(Environment.NewLine + "'" + me + "'");
                    }
                    else Console.Write("OK");
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
            IValidationSerializer serializer = GetSerializer(docProvider);
            if (serializer != null)
            {
                Stream stream;
                Encoding encoding;
                SetupStream(serializer, out stream, out encoding);

                if (selectedRuleset != null)
                {
                    serializer.Ruleset = selectedRuleset;

                    RulesetValidator validator = new RulesetValidator(selectedRuleset);

                    Console.Write(string.Format(
                        ResourceManager.GetString("performingSelfTest"),
                        ResourceManager.GetString(selectedRuleset.NameString))
                    );

                    serializer.TestResults = validator.Test();

                    Console.WriteLine(ResourceManager.GetString("done"));
                }

                try
                {
                    serializer.Serialize(stream, encoding);
                }
                finally
                {
                    stream.Close();
                }
            }            
        }

        static void ValidateFile(IXmlDocumentProvider docProvider, IValidationRuleset selectedRuleset)
        {
            IValidationSerializer serializer = GetSerializer(docProvider);
            bool needsMoreArguments = false;

            if (serializer != null)
            {
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
                                Console.Write(string.Format(
                                    ResourceManager.GetString("validatingCalendar"),
                                    ResourceManager.GetString(selectedRuleset.NameString)
                                ));

                                serializer.ValidationResults = rulesetValidator.Validate();

                                Console.WriteLine(ResourceManager.GetString("done"));
                            }
                        }
                    }
                }

                Stream stream;
                Encoding encoding;
                SetupStream(serializer, out stream, out encoding);

                try
                {
                    serializer.Serialize(stream, encoding);
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        static public void WriteDescription()
        {
            Console.WriteLine();
            Console.WriteLine("icalvalid - iCalendar validator");
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine("Usage: icalvalid.exe [options]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine("============================================================================");
            Console.WriteLine("/f:<filename>    | Loads an iCalendar from a file, where <filename> is");
            Console.WriteLine("                 | the file path of the iCalendar file to validate.");
            Console.WriteLine("/uri:<uri>       | Loads an iCalendar from a uri, where <uri> is the");
            Console.WriteLine("                 | Uri of the iCalendar file to validate.");
            Console.WriteLine("/u:<username>    | The username to use when retrieving a calendar via Uri.");
            Console.WriteLine("/p:<password>    | The password to use when retrieving a calendar via Uri.");
            Console.WriteLine("/t               | Performs a self test on the validator to ensure it is");
            Console.WriteLine("                 | properly validating calendar files.");
            Console.WriteLine("                 | NOTE: /f, /uri, /u, and /p are ignored when using /t.");
            Console.WriteLine("/v:<validator>   | The validator to use.  Possible values are:");
            Console.WriteLine("                 |       Strict_2_0");
            Console.WriteLine("                 |       Other validator names");
            Console.WriteLine("/l:<language>    | The language to use when validating (i.e. en-US, es-MX, etc.)");
            Console.WriteLine("/s               | Performs schema validation against the icalvalidSchema.");
            Console.WriteLine("                 | NOTE: most settings are ignored when using /s");
            Console.WriteLine("/o:<output>      | Sets the output to use.  Possible values are: file, console");
            Console.WriteLine("/format:<format> | Sets the output format.  Possible values are: text, xml");
            Console.WriteLine("/of:<format>     | Sets the file path to use when /o is set to 'file'.");
            Console.WriteLine("                 | Default is 'output.txt' or 'output.xml'.");
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
