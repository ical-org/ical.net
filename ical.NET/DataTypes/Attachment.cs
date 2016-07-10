using System;
using System.Text;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class to handle attachments, or URIs as attachments, within an iCalendar. 
    /// </summary>
    public class Attachment : EncodableDataType, IAttachment
    {
        public virtual Uri Uri { get; set; }
        public virtual byte[] Data { get; set; }
        public virtual Encoding ValueEncoding { get; set; }

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
            return Equals(Uri, other.Uri) && Equals(Data, other.Data) && Equals(ValueEncoding, other.ValueEncoding);
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
                hashCode = (hashCode * 397) ^ (Data?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ValueEncoding?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}