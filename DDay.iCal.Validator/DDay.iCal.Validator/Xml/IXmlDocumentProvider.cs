using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public interface IXmlDocumentProvider :
        IEnumerable<string>
    {
        /// <summary>
        /// Returns an XmlDocument using the provided <paramref name="path"/>.
        /// <paramref name="path"/> may be either a physical hard drive path
        /// or a virtual path, depending on the implementation.
        /// </summary>
        XmlDocument Load(string path);

        /// <summary>
        /// Returns a string containing the contents of the XML file. <see cref="Load"/>
        /// </summary>
        string LoadXml(string path);
    }
}
