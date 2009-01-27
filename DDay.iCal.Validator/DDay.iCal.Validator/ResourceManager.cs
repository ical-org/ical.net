using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DDay.iCal.Validator.Xml;

namespace DDay.iCal.Validator
{
    static public class ResourceManager
    {
        #region Static Private Fields

        static private IXmlDocumentProvider _XmlDocumentProvider;
        static private XmlDocument _XmlDocument;
        static private XmlNamespaceManager _Nsmgr;
        static private string _Prefix;
        static private string[] _LanguageIdentifiers = null;
        static private string _CurrentLanguageIdentifier = null;        
        static private bool _IsInitialized = false; 

        #endregion

        #region Static Public Properties

        public static string CurrentLanguageIdentifier
        {
            get
            {
                if (_CurrentLanguageIdentifier == null)
                    EnsureXmlDocument();
                return _CurrentLanguageIdentifier;
            }
        }

        #endregion

        #region Static Constructor

        static ResourceManager()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            _LanguageIdentifiers = new string[3]
            {
                ci.Name,
                ci.TwoLetterISOLanguageName,
                ci.ThreeLetterISOLanguageName
            };
        } 

        #endregion

        #region Static Private Methods

        static private void ParseLanguageIdentifiers(string language)
        {
            List<string> ids = new List<string>();
            ids.Add(language);
            string[] parts = language.Split('-');
            if (parts.Length > 1)
                ids.Add(parts[0]);

            _LanguageIdentifiers = ids.ToArray();
        }

        static private string PrepareForStringFormatting(string s)
        {
            StringBuilder sb = new StringBuilder();

            Match m = Regex.Match(s, @"(%(\d)+)");
            if (m.Success)
            {
                if (m.Index > 0)
                    sb.Append(s.Substring(0, m.Index));

                int num;
                if (Int32.TryParse(m.Groups[2].Value, out num))
                {
                    sb.Append("{");
                    sb.Append(num - 1);
                    sb.Append("}");
                }

                if (m.Index + m.Length < s.Length - 1)
                    sb.Append(s.Substring(m.Index + m.Length));

                return PrepareForStringFormatting(sb.ToString());
            }
            else return s;
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
            if (_XmlDocument == null)
            {
                _XmlDocument = null;
                _Nsmgr = null;
                _Prefix = null;

                foreach (string id in _LanguageIdentifiers)
                {
                    _XmlDocument = _XmlDocumentProvider.Load("languages/" + id + ".xml");
                    if (_XmlDocument != null)
                    {
                        _CurrentLanguageIdentifier = id;
                        break;
                    }
                }

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

        #endregion

        #region Static Public Methods

        static public bool Initialize(IXmlDocumentProvider docProvider, bool forceEnglishOnNotFound)
        {
            _XmlDocumentProvider = docProvider;
            _IsInitialized = EnsureXmlDocument();

            if (forceEnglishOnNotFound && !_IsInitialized)
            {
                _LanguageIdentifiers = new string[] { "en-US", "en" };
                _IsInitialized = EnsureXmlDocument();
            }
            return _IsInitialized;
        }

        static public bool Initialize(IXmlDocumentProvider docProvider, string language)
        {
            ParseLanguageIdentifiers(language);
            return Initialize(docProvider, false);
        }

        static public string GetString(string key)
        {
            if (EnsureXmlDocument())
            {
                XmlNode node = _XmlDocument.SelectSingleNode("/" +
                    _Prefix + ":language/" +
                    _Prefix + ":string[@name='" + ToCamelCase(key) + "']", _Nsmgr);

                if (node != null)
                    return PrepareForStringFormatting(node.InnerText);
            }
            return null;
        }

        static public string GetError(string key)
        {
            if (EnsureXmlDocument())
            {
                XmlNode node = _XmlDocument.SelectSingleNode("/" +
                    _Prefix + ":language/" +
                    _Prefix + ":errors/" +
                    _Prefix + ":error[@name='" + ToCamelCase(key) + "']", _Nsmgr);

                if (node != null)
                    return PrepareForStringFormatting(node.InnerText);
            }
            return null;
        }

        static public string GetResolution(string key)
        {
            if (EnsureXmlDocument())
            {
                XmlNode node = _XmlDocument.SelectSingleNode("/" +
                    _Prefix + ":language/" +
                    _Prefix + ":resolutions/" +
                    _Prefix + ":resolution[@error='" + ToCamelCase(key) + "']", _Nsmgr);

                if (node != null)
                    return PrepareForStringFormatting(node.InnerText);
            }
            return null;
        } 

        #endregion
    }
}
