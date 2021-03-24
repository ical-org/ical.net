using System;
using System.IO;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization.DataTypes
{
    public class OrganizerSerializer : StringSerializer
    {
        public OrganizerSerializer() { }

        public OrganizerSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (Organizer);

        public override string SerializeToString(object obj)
        {
            try
            {
                var o = obj as Organizer;
                return o?.Value == null
                    ? null
                    : Encode(o, Escape(o.Value.OriginalString));
            }
            catch
            {
                return null;
            }
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            Organizer o = null;
            try
            {
                o = CreateAndAssociate() as Organizer;
                if (o != null)
                {
                    var uriString = Unescape(Decode(o, value));

                    uriString = DecodeUrlString(uriString); // in some iCal files the organizer e-mail address contains URL encoded special characters
                    uriString = HandleDualooCase(uriString);

                    // Prepend "mailto:" if necessary
                    if (!uriString.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) && AlmostPerfectEmailMatch.Match(uriString.ToLower()).Success)
                    {
                        uriString = "mailto:" + uriString;
                    }

                    o.Value = new Uri(uriString);
                }
            }
            catch {}

            return o;
        }

        private static string DecodeUrlString(string url)
        {
            string newUrl;
            while ((newUrl = Uri.UnescapeDataString(url)) != url)
                url = newUrl;
            return newUrl;
        }

        /// <summary>
        /// iCals created by the software Dualoo formats the organizer like this:
        /// ORGANIZER;CN=Judith Alpiger:mailto:Dualoo%20%3Cno-reply@dualoo.com%3E
        /// URL decoded it's: Dualoo <no-reply@dualoo.com>
        /// This method gets the e-mail address btween the <> and returns it.
        /// </summary>
        /// <param name="uriString"></param>
        /// <returns></returns>
        private string HandleDualooCase(string uriString)
        {
            if (uriString.StartsWith("mailto:") && uriString.Contains("<") && uriString.EndsWith(">"))
            {
                uriString = uriString.Substring(7, uriString.Length - 7);
                uriString = uriString.Substring(uriString.IndexOf("<") + 1);
                uriString = uriString.Replace(">", "");
            }

            return uriString;
        }
    }
}