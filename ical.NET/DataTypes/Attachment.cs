using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// A class to handle attachments, or URIs as attachments, within an iCalendar. 
    /// </summary>
    [Serializable]
    public class Attachment : EncodableDataType, IAttachment
    {
        private Uri _mUri;
        private byte[] _mData;
        private Encoding _mEncoding;

        public Attachment()
        {
            Initialize();
        }

        public Attachment(string value) : this()
        {
            var serializer = new AttachmentSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public Attachment(byte[] value) : this()
        {
            _mData = value;
        }

        private void Initialize()
        {
            _mEncoding = System.Text.Encoding.Unicode;
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected bool Equals(Attachment other)
        {
            return Equals(_mUri, other._mUri) && Equals(_mData, other._mData) && Equals(_mEncoding, other._mEncoding);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Attachment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (_mUri != null
                    ? _mUri.GetHashCode()
                    : 0);
                hashCode = (hashCode * 397) ^ (_mData != null
                    ? _mData.GetHashCode()
                    : 0);
                hashCode = (hashCode * 397) ^ (_mEncoding != null
                    ? _mEncoding.GetHashCode()
                    : 0);
                return hashCode;
            }
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IAttachment)
            {
                var a = (IAttachment) obj;
                ValueEncoding = a.ValueEncoding;

                if (a.Data != null)
                {
                    Data = new byte[a.Data.Length];
                    a.Data.CopyTo(Data, 0);
                }
                else
                {
                    Data = null;
                }

                Uri = a.Uri;
            }
        }

        public virtual Uri Uri
        {
            get { return _mUri; }
            set { _mUri = value; }
        }

        public virtual byte[] Data
        {
            get { return _mData; }
            set { _mData = value; }
        }

        public virtual Encoding ValueEncoding
        {
            get { return _mEncoding; }
            set { _mEncoding = value; }
        }

        public virtual string Value
        {
            get
            {
                if (Data != null)
                {
                    return _mEncoding.GetString(Data);
                }
                return null;
            }
            set
            {
                Data = value == null
                    ? null
                    : _mEncoding.GetBytes(value);
            }
        }

        public virtual string FormatType
        {
            get { return Parameters.Get("FMTTYPE"); }
            set { Parameters.Set("FMTTYPE", value); }
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated at the given URI".
        /// </summary>
        public virtual void LoadDataFromUri()
        {
            LoadDataFromUri(null, null, null);
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated at the given URI.
        /// </summary>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        public virtual void LoadDataFromUri(string username, string password)
        {
            LoadDataFromUri(null, username, password);
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the contents of the given URI.
        /// </summary>
        /// <param name="uri">The Uri from which to download the <c>Data</c></param>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        public virtual void LoadDataFromUri(Uri uri, string username, string password)
        {
            using (var client = new WebClient())
            {
                if (username != null && password != null)
                {
                    client.Credentials = new NetworkCredential(username, password);
                }

                if (uri == null)
                {
                    if (Uri == null)
                    {
                        throw new ArgumentException("A URI was not provided for the LoadDataFromUri() method");
                    }
                    uri = new Uri(Uri.OriginalString);
                }

                Data = client.DownloadData(uri);
            }
        }
    }
}