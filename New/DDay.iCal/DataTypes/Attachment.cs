using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// A class to handle attachments, or URIs as attachments, within an iCalendar. 
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "Attachment", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#else
    [Serializable]
#endif
    public class Attachment : 
        EncodableDataType,
        IAttachment
    {
        #region Private Fields

        private Uri m_Uri;
        private byte[] m_Data;

        #endregion

        #region Constructors

        public Attachment() { }
        public Attachment(string value)
            : this()
        {
            AttachmentSerializer serializer = new AttachmentSerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is IAttachment)
            {
                IAttachment a = (IAttachment)obj;

                if (Data == null && a.Data == null)
                    return Uri.Equals(a.Uri);
                else if (Data == null || a.Data == null)
                    // One item is null, but the other isn't                    
                    return false;
                else if (Data.Length != a.Data.Length)
                    return false;
                for (int i = 0; i < Data.Length; i++)
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
                IAttachment a = (IAttachment)obj;
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

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public Uri Uri
        {
            get { return m_Uri; }
            set { m_Uri = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public byte[] Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        virtual public string Value
        {
            get
            {
                if (Data != null)
                    return System.Text.Encoding.Unicode.GetString(Data);
                return null;
            }
            set
            {
                if (value != null)
                    Data = System.Text.Encoding.Unicode.GetBytes(value);
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
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        virtual public void LoadDataFromUri()
        {
            LoadDataFromUri(null, null, null);
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        virtual public void LoadDataFromUri(string username, string password)
        {
            LoadDataFromUri(null, username, password);
        }

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        /// <param name="uri">The Uri from which to download the <c>Data</c></param>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        virtual public void LoadDataFromUri(Uri uri, string username, string password)
        {
            WebClient client = new WebClient();
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

        #endregion
    }
}
