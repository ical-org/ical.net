using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using DDay.iCal.Validator.Xml;
using System.Xml;

namespace DDay.iCal.Validator
{
    static public class ResourceManager
    {
        static private IXmlDocumentProvider _XmlDocumentProvider;
        static private XmlDocument _XmlDocument;
        static private XmlNamespaceManager _Nsmgr;
        static private string _Prefix;

        static ResourceManager()
        {            
        }

        static public void Initialize(IXmlDocumentProvider docProvider)
        {
            _XmlDocumentProvider = docProvider;
        }

        static private string ToCamelCase(string s)
        {
            if (s != null)
            {
                if (s.Length > 1)
                    return s.Substring(0, 1).ToLower() + s.Substring(1);
                else
                    return s.ToLower();
            }
            return s;
        }

        static private bool EnsureXmlDocument()
        {
            if (_XmlDocument == null &&
                _XmlDocumentProvider != null)
            {
                CultureInfo ci = CultureInfo.CurrentCulture;
                _XmlDocument = _XmlDocumentProvider.Load("languages/" + ci.Name + ".xml");
                if (_XmlDocument == null)
                    _XmlDocument = _XmlDocumentProvider.Load("languages/" + ci.TwoLetterISOLanguageName + ".xml");
                if (_XmlDocument == null)
                    _XmlDocument = _XmlDocumentProvider.Load("languages/" + ci.ThreeLetterISOLanguageName + ".xml");

                if (_XmlDocument != null)
                {
                    _Nsmgr = new XmlNamespaceManager(_XmlDocument.NameTable);
                    _Prefix = _Nsmgr.LookupPrefix("http://icalvalid.wikidot.com/validation");
                    if (_Prefix == null)
                    {
                        _Nsmgr.AddNamespace("v", "http://icalvalid.wikidot.com/validation");
                        _Prefix = "v";
                    }
                }
            }

            return _XmlDocument != null;
        }

        static internal string GetString(string key)
        {
            if (EnsureXmlDocument())
            {
                XmlNode node = _XmlDocument.SelectSingleNode("/" + 
                    _Prefix + ":language/" +
                    _Prefix + ":string[@name='" + ToCamelCase(key) + "']", _Nsmgr);

                if (node != null)
                    return node.InnerText;
            }
            return null;
        }

        static internal string GetError(string key)
        {
            if (EnsureXmlDocument())
            {
                XmlNode node = _XmlDocument.SelectSingleNode("/" + 
                    _Prefix + ":language/" + 
                    _Prefix + ":errors/" +
                    _Prefix + ":error[@name='" + ToCamelCase(key) + "']", _Nsmgr);

                if (node != null)
                    return node.InnerText;
            }
            return null;
        }

        static internal string GetResolution(string key)
        {
            if (EnsureXmlDocument())
            {
                XmlNode node = _XmlDocument.SelectSingleNode("/" + 
                    _Prefix + ":language/" + 
                    _Prefix + ":resolutions/" +
                    _Prefix + ":resolution[@error='" + ToCamelCase(key) + "']", _Nsmgr);

                if (node != null)
                    return node.InnerText;
            }
            return null;
        }
    }
}
