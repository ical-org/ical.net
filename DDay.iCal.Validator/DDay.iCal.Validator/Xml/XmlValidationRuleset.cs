using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public class XmlValidationRuleset :
        ValidationRuleset
    {
        public XmlValidationRuleset(IXmlDocumentProvider docProvider, XmlNode node, XmlNamespaceManager nsmgr)
        {
            if (node.Attributes["name"] != null)
                this.Name = node.Attributes["name"].Value;
            if (node.Attributes["nameString"] != null)
                this.NameString = node.Attributes["nameString"].Value;
            if (node.Attributes["descriptionString"] != null)
                this.DescriptionString = node.Attributes["descriptionString"].Value;

            List<IValidationRule> rules = new List<IValidationRule>();
            string prefix = nsmgr.LookupPrefix("http://icalvalid.wikidot.com/validation");

            if (node.Attributes["basedOn"] != null)
            {
                // Inherit rules from the ruleset this one is based on.
                string name = node.Attributes["basedOn"].Value;
                foreach (XmlNode rule in node.SelectNodes("parent::" + prefix + ":rulesets/" + prefix + ":ruleset[@name='" + name + "']/" + prefix + ":rule", nsmgr))
                    rules.Add(new XmlValidationRule(docProvider, rule, nsmgr));
            }
            
            foreach (XmlNode rule in node.SelectNodes(prefix + ":rule", nsmgr))
                rules.Add(new XmlValidationRule(docProvider, rule, nsmgr));

            Rules = rules.ToArray();
        }
    }
}
