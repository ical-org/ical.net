﻿using System;
using System.IO;
using ical.net.DataTypes;
using ical.net.Interfaces.DataTypes;
using ical.net.Serialization.iCalendar.Serializers.Other;

namespace ical.net.Serialization.iCalendar.Serializers.DataTypes
{
    public class OrganizerSerializer : StringSerializer
    {
        public override Type TargetType => typeof (Organizer);

        public override string SerializeToString(object obj)
        {
            try
            {
                var o = obj as IOrganizer;
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

            IOrganizer o = null;
            try
            {
                o = CreateAndAssociate() as IOrganizer;
                if (o != null)
                {
                    var uriString = Unescape(Decode(o, value));

                    // Prepend "mailto:" if necessary
                    if (!uriString.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
                    {
                        uriString = "mailto:" + uriString;
                    }

                    o.Value = new Uri(uriString);
                }
            }
            catch {}

            return o;
        }
    }
}