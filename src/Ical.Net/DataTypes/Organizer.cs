using System;
using System.Diagnostics;
using System.IO;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class that represents the organizer of an event/todo/journal.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public class Organizer : EncodableDataType
    {
        public Uri SentBy
        {
            get => new Uri(Parameters.Get("SENT-BY"));
            set
            {
                if (value != null)
                {
                    Parameters.Set("SENT-BY", value.OriginalString);
                }
                else
                {
                    Parameters.Set("SENT-BY", (string) null);
                }
            }
        }

        public string CommonName
        {
            get => Parameters.Get("CN");
            set => Parameters.Set("CN", value);
        }

        public Uri DirectoryEntry
        {
            get => new Uri(Parameters.Get("DIR"));
            set
            {
                if (value != null)
                {
                    Parameters.Set("DIR", value.OriginalString);
                }
                else
                {
                    Parameters.Set("DIR", (string) null);
                }
            }
        }

        public Uri Value { get; set; }

        public Organizer() {}

        public Organizer(string value) : this()
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var serializer = new OrganizerSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        protected bool Equals(Organizer other) => Equals(Value, other.Value);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Organizer) obj);
        }

        public override int GetHashCode() => Value?.GetHashCode() ?? 0;

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            if (obj is Organizer o)
            {
                Value = o.Value;
            }
        }
    }
}