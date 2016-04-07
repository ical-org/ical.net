using System;
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
            var sc = obj as IStatusCode;
            if (sc != null)
            {
                var vals = new string[sc.Parts.Length];
                for (var i = 0; i < sc.Parts.Length; i++)
                    vals[i] = sc.Parts[i].ToString();
                return Encode(sc, Escape(string.Join(".", vals)));
            }
            return null;
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            var sc = CreateAndAssociate() as IStatusCode;
            if (sc != null)
            {
                // Decode the value as needed
                value = Decode(sc, value);
                                
                var match = Regex.Match(value, @"\d(\.\d+)*");
                if (match.Success)
                {
                    int[] iparts;
                    var parts = match.Value.Split('.');
                    iparts = new int[parts.Length];
                    for (var i = 0; i < parts.Length; i++)
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
