using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public class XmlValidationRuleset :
        ValidationRuleset
    {
        public XmlValidationRuleset(XmlNode node, XmlNamespaceManager nsmgr)
        {
            if (node.Attributes["name"] != null)
                this.Name = node.Attributes["name"].Value;
            if (node.Attributes["description"] != null)
                this.Name = node.Attributes["description"].Value;
        
            List<IValidationRule> rules = new List<IValidationRule>();

            string prefix = nsmgr.LookupPrefix("http://icalvalid.wikidot.com/validation");
            foreach (XmlNode rule in node.SelectNodes(prefix + ":rule", nsmgr))
                rules.Add(new XmlValidationRule(rule, nsmgr));

            Rules = rules.ToArray();
        }
    }
}
