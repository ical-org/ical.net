using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public class EncodingProvider :
        IEncodingProvider
    {
        public delegate string EncoderDelegate(byte[] data);
        public delegate byte[] DecoderDelegate(string value);

        #region Private Fields

        ISerializationContext m_SerializationContext;

        #endregion

        #region Constructors

        public EncodingProvider(ISerializationContext ctx)
        {
            m_SerializationContext = ctx;
        }

        #endregion

        #region Protected Methods

        #region Decoding

        protected byte[] Decode7Bit(string value)
        {
            try
            {
                UTF7Encoding utf7 = new UTF7Encoding();                
                return utf7.GetBytes(value);
            }
            catch
            {
                return null;
            }
        }

        protected byte[] Decode8Bit(string value)
        {
            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                return utf8.GetBytes(value);                
            }
            catch
            {
                return null;
            }
        }

        protected byte[] DecodeBase64(string value)
        {
            try
            {
                return Convert.FromBase64String(value);
            }
            catch
            {
                return null;
            }
        }

        virtual protected DecoderDelegate GetDecoderFor(string encoding)
        {
            if (encoding != null)
            {
                switch (encoding.ToUpper())
                {
                    case "7BIT": return Decode7Bit;
                    case "8BIT": return Decode8Bit;
                    case "BASE64": return DecodeBase64;
                    default:
                        return null;
                }
            }
            return null;
        } 

        #endregion

        #region Encoding

        protected string Encode7Bit(byte[] data)
        {
            try
            {
                UTF7Encoding utf7 = new UTF7Encoding();
                return utf7.GetString(data);
            }
            catch
            {
                return null;
            }
        }

        protected string Encode8Bit(byte[] data)
        {
            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                return utf8.GetString(data);
            }
            catch
            {
                return null;
            }
        }

        protected string EncodeBase64(byte[] data)
        {
            try
            {
                return Convert.ToBase64String(data);
            }
            catch
            {
                return null;
            }
        }

        virtual protected EncoderDelegate GetEncoderFor(string encoding)
        {
            if (encoding != null)
            {
                switch (encoding.ToUpper())
                {
                    case "7BIT": return Encode7Bit;
                    case "8BIT": return Encode8Bit;
                    case "BASE64": return EncodeBase64;
                    default:
                        return null;
                }
            }
            return null;
        } 

        #endregion

        #endregion

        #region IEncodingProvider Members

        public string Encode(string encoding, byte[] data)
        {
            if (encoding != null &&
                data != null)
            {
                EncoderDelegate encoder = GetEncoderFor(encoding);
                if (encoder != null)
                    return encoder(data);
            }
            return null;
        }

        public string DecodeString(string encoding, string value)
        {
            if (encoding != null &&
                value != null)
            {
                byte[] data = DecodeData(encoding, value);
                if (data != null)
                {
                    // Decode the string into the current encoding
                    IEncodingStack encodingStack = m_SerializationContext.GetService(typeof(IEncodingStack)) as IEncodingStack;
                    return encodingStack.Current.GetString(data);
                }
            }
            return null;
        }

        public byte[] DecodeData(string encoding, string value)
        {
            if (encoding != null &&
                value != null)
            {
                DecoderDelegate decoder = GetDecoderFor(encoding);
                if (decoder != null)
                    return decoder(value);
            }
            return null;
        }

        #endregion
    }
}
