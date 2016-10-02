using System;
using System.Linq;
using System.Text;
using ical.net.Interfaces.General;
using ical.net.Serialization.iCalendar.Serializers.DataTypes;
using ical.net.Utility;

namespace ical.net.DataTypes
{
    /// <summary>
    /// Attachments represent the ATTACH element that can be associated with Alarms, Journals, Todos, and Events. There are two kinds of attachments:
    /// 1) A string representing a URI which is typically human-readable, OR
    /// 2) A base64-encoded string that can represent anything
    /// </summary>
    public class Attachment : EncodableDataType//, IAttachment
    {
        public virtual Uri Uri { get; set; }
        public virtual byte[] Data { get; set; }

        private Encoding _valueEncoding = System.Text.Encoding.UTF8;
        public virtual Encoding ValueEncoding
        {
            get
            {
                return _valueEncoding;
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                _valueEncoding = value;
            }
        }

        public virtual string FormatType
        {
            get { return Parameters.Get("FMTTYPE"); }
            set { Parameters.Set("FMTTYPE", value); }
        }

        public Attachment() {}

        public Attachment(byte[] value) : this()
        {
            Data = value;
        }

        public Attachment(string value) : this()
        {
            var serializer = new AttachmentSerializer();
            var a = serializer.Deserialize(value);
            if (a == null)
            {
                throw new ArgumentException($"{value} is not a valid ATTACH component");
            }

            ValueEncoding = a.ValueEncoding;

            Data = a.Data;
            Uri = a.Uri;
        }

        public override string ToString()
        {
            return Data == null
                ? string.Empty
                : ValueEncoding.GetString(Data);
        }

        //ToDo: See if this can be deleted
        public override void CopyFrom(ICopyable obj) { }

        protected bool Equals(Attachment other)
        {
            var firstPart = Equals(Uri, other.Uri) && ValueEncoding.Equals(other.ValueEncoding);
            return Data == null
                ? firstPart
                : firstPart && Data.SequenceEqual(other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Attachment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Uri?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (CollectionHelpers.GetHashCode(Data));
                hashCode = (hashCode * 397) ^ (ValueEncoding?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}