using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public interface IXmlDocumentProvider
    {
        XmlDocument Load(string path);
    }
}
