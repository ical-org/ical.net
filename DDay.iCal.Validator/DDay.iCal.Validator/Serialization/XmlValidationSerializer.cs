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

        virtual public IXmlDocumentProvider XmlDocProvider { get; set; }
        virtual public string DestinationFilepath { get; set; }
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

                    XmlElement results = doc.CreateNode(XmlNodeType.Element, "results", "http://icalvalid.wikidot.com/validation") as XmlElement;
                    results.SetAttribute("language", ResourceManager.CurrentLanguageIdentifier);
                    results.SetAttribute("datetime", now.ToString("yyyy-MM-dd") + "T" + now.ToString("hh:mm:ss"));

                    if (TestResults != null &&
                        TestResults.Length > 0)
                    {
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
