using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using System.Runtime.Serialization;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// A class to handle binary attachments, or URIs as binary attachments, within an iCalendar. 
    /// </summary>
    [Encodable("BASE64")]
#if SILVERLIGHT
    [DataContract(Name = "Binary", Namespace="http://www.ddaysoftware.com/dday.ical/datatypes/2009/07/")]
#else
    [Serializable]
#endif
    public class Binary : EncodableDataType
    {
        #region Private Fields

        private URI m_Uri;

        #endregion

        #region Public Properties

        public URI Uri
        {
            get { return m_Uri; }
            set { m_Uri = value; }
        }

        public Text FormatType
        {
            get
            {                
                if (Parameters.ContainsKey("FMTYPE"))
                {
                    Parameter p = (Parameter)Parameters["FMTYPE"];
                    Text fmtype = new Text(p.Values[0].ToString());
                    return fmtype;
                }
                return null;
            }
        }

        #endregion

        #region Constructors

        public Binary() { }
        public Binary(string value)
            : this()
        {
            CopyFrom(Parse(value));
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is Binary)
            {
                Binary b = (Binary)obj;

                if (Data == null && b.Data == null)
                    return Uri.Equals(b.Uri);
                else if (Data == null || b.Data == null)
                    // One item is null, but the other isn't                    
                    return false;
                else if (Data.Length != b.Data.Length)
                    return false;
                for (int i = 0; i < Data.Length; i++)
                    if (Data[i] != b.Data[i])
                        return false;
                return true;                
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Uri != null)
                return Uri.GetHashCode();
            return base.GetHashCode();
        }

        public override bool TryParse(string value, ref object obj)
        {
            if (Encoding != null)
                return base.TryParse(value, ref obj);
            else
            {
                Binary b = (Binary)obj;
                b.m_Uri = new URI();
                object o = b.m_Uri;
                return b.m_Uri.TryParse(value, ref o);                
            }
        }
        
        public override void CopyFrom(object obj)
        {
            base.CopyFrom(obj);
            if (obj is Binary)
            {
                Binary b = (Binary)obj;
                if (b.Data != null)
                {
                    Data = new byte[b.Data.Length];
                    b.Data.CopyTo(Data, 0);
                }
                else Data = null;
                Value = b.Value;
                Uri = b.Uri;
            }
            base.CopyFrom(obj);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        public void LoadDataFromUri()
        {
            LoadDataFromUri(null, null, null);
        }
        
        /// <summary>
        /// Loads (fills) the <c>Data</c> property with the file designated
        /// at the given <see cref="URI"/>.
        /// </summary>
        /// <param name="username">The username to supply for credentials</param>
        /// <param name="password">The pasword to supply for credentials</param>
        public void LoadDataFromUri(string username, string password)
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
        public void LoadDataFromUri(Uri uri, string username, string password)
        {            
            WebClient client = new WebClient();
            if (username != null &&
                password != null)
                client.Credentials = new System.Net.NetworkCredential(username, password);

            if (uri == null)
            {
                if (Uri == null)
                    throw new ArgumentException("A URI was not provided for the Binary::LoadDataFromUri() method");
                uri = new Uri(Uri.Value);
            }                       
            
            Data = client.DownloadData(uri);
        }

        #endregion
    }
}
