using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization.iCalendar
{
    public abstract class EncodableDataTypeSerializer :
        DataTypeSerializer
    {
        #region Constructors

        public EncodableDataTypeSerializer()
        {
        }

        public EncodableDataTypeSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion

        #region Protected Methods

        protected string Encode(IEncodableDataType dt, string value)
        {
            if (value != null)
            {
                if (dt == null || dt.Encoding == null)
                    return value;

                // Return the value in the current encoding
                IEncodingStack encodingStack = GetService<IEncodingStack>();
                return Encode(dt, encodingStack.Current.GetBytes(value));
            }
            return null;
        }

        protected string Encode(IEncodableDataType dt, byte[] data)
        {
            if (data != null)
            {
                if (dt == null || dt.Encoding == null)
                {
                    // Default to the current encoding
                    IEncodingStack encodingStack = GetService<IEncodingStack>();
                    return encodingStack.Current.GetString(data);
                }

                IEncodingProvider encodingProvider = GetService<IEncodingProvider>();
                if (encodingProvider != null)
                    return encodingProvider.Encode(dt.Encoding, data);
            }
            return null;
        }

        protected string Decode(IEncodableDataType dt, string value)
        {
            byte[] data = DecodeData(dt, value);
            if (data != null)
            {
                // Default to the current encoding
                IEncodingStack encodingStack = GetService<IEncodingStack>();
                return encodingStack.Current.GetString(data);
            }
            return null;
        }

        protected byte[] DecodeData(IEncodableDataType dt, string value)
        {
            if (value != null)
            {
                if (dt == null || dt.Encoding == null)
                {
                    // Default to the current encoding
                    IEncodingStack encodingStack = GetService<IEncodingStack>();
                    return encodingStack.Current.GetBytes(value);
                }

                IEncodingProvider encodingProvider = GetService<IEncodingProvider>();
                if (encodingProvider != null)
                    return encodingProvider.DecodeData(dt.Encoding, value);
            }
            return null;
        }

        #endregion
    }
}
