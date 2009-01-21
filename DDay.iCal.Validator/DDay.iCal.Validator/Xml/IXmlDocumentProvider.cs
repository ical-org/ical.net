using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public interface IXmlDocumentProvider
    {
        /// <summary>
        /// Returns an XmlDocument using the provided <paramref name="path"/>.
        /// <paramref name="path"/> may be either a physical hard drive path
        /// or a virtual path, depending on the implementation.
        /// </summary>
        XmlDocument Load(string path);
    }
}
