using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public class XmlValidationRule :
        ValidationRule
    {
        public XmlValidationRule(XmlNode node, XmlNamespaceManager nsmgr)
        {
            if (node.Attributes["name"] != null)
                Name = node.Attributes["name"].Value;
        }

        public override Type ValidatorType
        {
            get
            {
                Type type = Type.GetType("DDay.iCal.Validator.RFC2445." + Name + "Validator, DDay.iCal.Validator", false, true);
                return type;                
            }
            protected set
            {
            }
        }
    }
}
