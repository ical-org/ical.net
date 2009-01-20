using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public class XmlValidationRulesetLoader :
        IValidationRulesetProvider
    {
        #region Private Fields

        IXmlDocumentProvider _DocumentProvider; 

        #endregion

        #region Constructors

        public XmlValidationRulesetLoader(IXmlDocumentProvider docProvider)
        {
            _DocumentProvider = docProvider;
        } 

        #endregion

        #region IValidationRulesetProvider Members

        public IValidationRuleset[] Load()
        {
            XmlDocument xmlDoc = _DocumentProvider.Load("rulesets.xml");
            if (xmlDoc != null)
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("v", "http://icalvalid.wikidot.com/validation");

                // Build a list of rulesets
                List<IValidationRuleset> rulesets = new List<IValidationRuleset>();
                foreach (XmlNode node in xmlDoc.SelectNodes("/v:rulesets/v:ruleset", nsmgr))
                    rulesets.Add(new XmlValidationRuleset(_DocumentProvider, node, nsmgr));
                
                return rulesets.ToArray();
            }
            return null;
        }

        #endregion
    }
}
