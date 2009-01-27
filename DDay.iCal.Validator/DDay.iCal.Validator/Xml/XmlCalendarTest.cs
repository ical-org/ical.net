using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace DDay.iCal.Validator.Xml
{
    public class XmlCalendarTest :
        Test
    {
        public XmlCalendarTest(XmlNode node, XmlNamespaceManager nsmgr)
        {
            if (string.Equals("pass", node.LocalName))
                Type = TestType.Pass;
            else
            {
                Type = TestType.Fail;
                if (node.Attributes["error"] != null)
                    ExpectedError = node.Attributes["error"].Value;
            }

            iCalendarText = node.InnerText;
        }
    }
}
