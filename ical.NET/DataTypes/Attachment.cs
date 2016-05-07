using System;
using System.IO;
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
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Attachment : 
        EncodableDataType,
        IAttachment
    {
        #region Private Fields

        private Uri _mUri;
        private byte[] _mData;
        private Encoding _mEncoding;

        #endregion

        #region Constructors

        public Attachment() 
        {
            Initialize();
        }
        public Attachment(string value)
            : this()
        {
            var serializer = new AttachmentSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public Attachment(byte[] value) : this()
        {
            _mData = value;
        }

        void Initialize()
        {
            _mEncoding = System.Text.Encoding.Unicode;
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        public override bool Equals(object obj)
        {
            if (obj is IAttachment)
            {
                var a = (IAttachment)obj;

                if (Data == null && a.Data == null)
                    return Uri.Equals(a.Uri);
                else if (Data == null || a.Data == null)
                    // One item is null, but the other isn't                    
                    return false;
                else if (Data.Length != a.Data.Length)
                    return false;
                for (var i = 0; i < Data.Length; i++)
                    if (Data[i] != a.Data[i])
                        return false;
                return true;                
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Uri != null)
                return Uri.GetHashCode();
            else if (Data != null)
                return Data.GetHashCode();
            return base.GetHashCode();
        }
      
        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IAttachment)
            {
                var a = (IAttachment)obj;
                ValueEncoding = a.ValueEncoding;

                if (a.Data != null)
                {
                    Data = new byte[a.Data.Length];
                    a.Data.CopyTo(Data, 0);
                }
                else Data = null;

                Uri = a.Uri;
            }
        }

        #endregion        

        #region IAttachment Members

        virtual public Uri Uri
        {
            get { return _mUri; }
            set { _mUri = value; }
        }

        virtual public byte[] Data
        {
            get { return _mData; }
            set { _mData = value; }
        }

        virtual public Encoding ValueEncoding
        {
            get { return _mEncoding; }
            set { _mEncoding = value; }
        }

        virtual public string Value
        {
            get
            {
                if (Data != null)
                    return _mEncoding.GetString(Data);
                return null;
            }
            set
            {
                if (value != null)
                    Data = _mEncoding.GetBytes(value);
                else                    
                    Data = null;
            }
        }

        virtual public string FormatType
        {
            get { return Parameters.Get("FMTTYPE"); }
            set { Parameters.Set("FMTTYPE", value); }
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated at the given URI".
        /// </summary>
        virtual public void LoadDataFromUri()
        {
            LoadDataFromUri(null, null, null);
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated at the given URI.
        /// </summary>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        virtual public void LoadDataFromUri(string username, string password)
        {
            LoadDataFromUri(null, username, password);
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the contents of the given URI.
        /// </summary>
        /// <param name="uri">The Uri from which to download the <c>Data</c></param>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        virtual public void LoadDataFromUri(Uri uri, string username, string password)
        {
            using (var client = new WebClient())
            {
                if (username != null &&
                    password != null)
                    client.Credentials = new System.Net.NetworkCredential(username, password);

                if (uri == null)
                {
                    if (Uri == null)
                        throw new ArgumentException("A URI was not provided for the LoadDataFromUri() method");
                    uri = new Uri(Uri.OriginalString);
                }

                Data = client.DownloadData(uri);
            }
        }

        #endregion
    }
}
