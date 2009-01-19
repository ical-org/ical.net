using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public class XmlDocumentZipExtractor : 
        ZipExtractor,
        IXmlDocumentProvider
    {
        public XmlDocumentZipExtractor(string zipFilepath) : base(zipFilepath)
        {
        }

        #region IXmlDocumentProvider Members

        public XmlDocument Load(string pathToFileWithinZip)
        {
            string contents = GetFileContents(pathToFileWithinZip);
            if (contents != null)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(contents);
                return xmlDoc;
            }

            return null;
        }

        #endregion
    }
}
