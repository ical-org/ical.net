using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Utility;

namespace Ical.Net.Serialization.DataTypes
{
    public class StatusCodeSerializer : StringSerializer
    {
        public StatusCodeSerializer() { }

        public StatusCodeSerializer(SerializationContext ctx) : base(ctx) { }

        public override Type TargetType => typeof (StatusCode);

        public override string SerializeToString(object obj)
        {
            var sc = obj as StatusCode;
            if (sc == null)
            {
                return null;
            }

            var vals = new string[sc.Parts.Length];
            for (var i = 0; i < sc.Parts.Length; i++)
            {
                vals[i] = sc.Parts[i].ToString();
            }
            return Encode(sc, Escape(string.Join(".", vals)));
        }

        public override object Deserialize(TextReader tr)
        {
            var value = tr.ReadToEnd();

            var sc = CreateAndAssociate() as StatusCode;
            if (sc == null)
            {
                return null;
            }

            // Decode the value as needed
            value = Decode(sc, value);

            var match = CompiledRegularExpressions.StatusCode.Match(value);
            if (!match.Success)
            {
                return null;
            }

            var parts = match.Value.Split('.');
            var iparts = new int[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                int num;
                if (!int.TryParse(parts[i], out num))
                {
                    return false;
                }
                iparts[i] = num;
            }

            return new StatusCode(iparts);
        }
    }
}