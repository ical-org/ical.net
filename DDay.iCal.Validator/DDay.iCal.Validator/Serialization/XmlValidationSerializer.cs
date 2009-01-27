using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using DDay.iCal.Validator.Xml;

namespace DDay.iCal.Validator.Serialization
{
    public class XmlValidationSerializer :
        IValidationSerializer
    {
        #region Constructors

        public XmlValidationSerializer(
            IXmlDocumentProvider docProvider)
        {
            XmlDocProvider = docProvider;
        }

        public XmlValidationSerializer(
            IXmlDocumentProvider docProvider,
            IValidationRuleset ruleset,
            ITestResult[] testResults) :
            this(docProvider)
        {
            Ruleset = ruleset;
            TestResults = testResults;
        }

        public XmlValidationSerializer(
            IXmlDocumentProvider docProvider,
            IValidationRuleset ruleset,
            IValidationResult[] validationResults) :
            this(docProvider)
        {
            Ruleset = ruleset;
            ValidationResults = validationResults;
        }

        #endregion

        #region IValidationSerializer Members

        public string DefaultExtension
        {
            get { return "xml"; }
        }

        public Encoding DefaultEncoding
        {
            get { return Encoding.UTF8; }
        }

        virtual public IXmlDocumentProvider XmlDocProvider { get; set; }
        virtual public IValidationRuleset Ruleset { get; set; }
        virtual public ITestResult[] TestResults { get; set; }
        virtual public IValidationResult[] ValidationResults { get; set; }

        virtual public void Serialize(Stream stream, Encoding encoding)
        {
            XmlWriter xw = null;
            try
            {
                if (XmlDocProvider != null)
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.CloseOutput = false;
                    settings.ConformanceLevel = ConformanceLevel.Document;
                    settings.Encoding = encoding;
                    settings.Indent = true;
                    settings.IndentChars = "    ";
                    settings.NewLineChars = "\r\n";
                    settings.NewLineOnAttributes = true;

                    xw = XmlWriter.Create(stream, settings);

                    XmlDocument doc = new XmlDocument();
                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("i", "http://icalvalid.wikidot.com/validation");

                    // FIXME: add a schema!
                    // doc.Schemas.Add("http://icalvalid.wikidot.com/validation", "schema.xsd");

                    DateTime now = DateTime.Now;

                    XmlElement results = doc.CreateElement("results", "http://icalvalid.wikidot.com/validation");
                    results.SetAttribute("language", ResourceManager.CurrentLanguageIdentifier);
                    results.SetAttribute("datetime", now.ToString("yyyy-MM-dd") + "T" + now.ToString("hh:mm:ss"));

                    if (TestResults != null &&
                        TestResults.Length > 0)
                    {
                        XmlElement testResults = doc.CreateElement("testResults", "http://icalvalid.wikidot.com/validation");
                        testResults.SetAttribute("ruleset", Ruleset.Name);

                        foreach (ITestResult result in TestResults)
                        {
                            XmlElement testResult = doc.CreateElement("testResult", "http://icalvalid.wikidot.com/validation");
                            testResult.SetAttribute("rule", result.Rule);
                            testResult.SetAttribute("expected", result.Test.Type == TestType.Pass ? "pass" : "fail");
                            if (result.Passed != null && result.Passed.HasValue)
                            {
                                string pass = (result.Test.Type == TestType.Pass ? "pass" : "fail");
                                string fail = (result.Test.Type == TestType.Pass ? "fail" : "pass");
                                testResult.SetAttribute("actual", BoolUtil.IsTrue(result.Passed) ? pass : fail);
                            }
                            else
                            {
                                testResult.SetAttribute("actual", "none");
                            }
                            
                            if (!string.IsNullOrEmpty(result.Test.ExpectedError))
                                testResult.SetAttribute("errorName", result.Test.ExpectedError);
                            if (result.Error != null)
                                testResult.SetAttribute("error", result.Error.ToString());

                            testResults.AppendChild(testResult);
                        }

                        results.AppendChild(testResults);
                    }

                    if (ValidationResults != null &&
                        ValidationResults.Length > 0)
                    {
                        XmlElement validationResults = doc.CreateElement("validationResults", "http://icalvalid.wikidot.com/validation");
                        validationResults.SetAttribute("ruleset", Ruleset.Name);

                        foreach (IValidationResult result in ValidationResults)
                        {
                            XmlElement validationResult = doc.CreateElement("validationResult", "http://icalvalid.wikidot.com/validation");
                            validationResult.SetAttribute("rule", result.Rule);
                            if (result.Passed != null && result.Passed.HasValue)
                                validationResult.SetAttribute("result", result.Passed.Value ? "pass" : "fail");
                            else
                                validationResult.SetAttribute("result", "none");

                            if (result.Errors != null &&
                                result.Errors.Length > 0)
                            {
                                XmlElement validationErrors = doc.CreateElement("errors", "http://icalvalid.wikidot.com/validation");
                                foreach (IValidationError error in result.Errors)
                                {
                                    XmlElement validationError = doc.CreateElement("error", "http://icalvalid.wikidot.com/validation");
                                    validationError.SetAttribute("name", error.Name);
                                    validationError.SetAttribute("type", error.Type.ToString().ToLower());
                                    validationError.SetAttribute("message", error.Message);
                                    validationError.SetAttribute("isFatal", error.IsFatal.ToString().ToLower());
                                    if (error.Line != default(int))
                                        validationError.SetAttribute("line", error.Line.ToString());
                                    if (error.Col != default(int))
                                        validationError.SetAttribute("col", error.Col.ToString());

                                    validationErrors.AppendChild(validationError);
                                }

                                validationResult.AppendChild(validationErrors);
                            }

                            validationResults.AppendChild(validationResult);                            
                        }

                        results.AppendChild(validationResults);
                    }

                    doc.AppendChild(results);

                    doc.Save(xw);
                }
            }
            finally
            {
                if (xw != null)
                {
                    xw.Close();
                    xw = null;
                }
            }
        }

        #endregion
    }
}
