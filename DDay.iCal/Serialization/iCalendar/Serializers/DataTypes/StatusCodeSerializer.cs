using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace DDay.iCal.Serialization.iCalendar
{
    public class StatusCodeSerializer :
        StringSerializer
    {
        public override Type TargetType
        {
            get { return typeof(StatusCode); }
        }

        public override string SerializeToString(object obj)
        {
            IStatusCode sc = obj as IStatusCode;
            if (sc != null)
            {
                string[] vals = new string[sc.Parts.Length];
                for (int i = 0; i < sc.Parts.Length; i++)
                    vals[i] = sc.Parts[i].ToString();
                return Encode(sc, Escape(string.Join(".", vals)));
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            string value = tr.ReadToEnd();

            IStatusCode sc = CreateAndAssociate() as IStatusCode;
            if (sc != null)
            {
                // Decode the value as needed
                value = Decode(sc, value);
                                
                Match match = Regex.Match(value, @"\d(\.\d+)*");
                if (match.Success)
                {
                    int[] iparts;
                    string[] parts = match.Value.Split('.');
                    iparts = new int[parts.Length];
                    for (int i = 0; i < parts.Length; i++)
                    {
                        int num;
                        if (!Int32.TryParse(parts[i], out num))
                            return false;
                        iparts[i] = num;
                    }

                    sc.Parts = iparts;
                    return sc;
                }
            }
            return null;
        }
    }
}
