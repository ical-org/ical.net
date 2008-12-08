using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class BinarySerializer : EncodableDataTypeSerializer
    {
        #region Private Fields

        private Binary m_Binary;

        #endregion

        #region Constructors

        public BinarySerializer(Binary b)
            : base(b)
        {
            this.m_Binary = b;
        }

        #endregion

        #region Overrides

        public override List<Parameter> DisallowedParameters
        {
            get
            {
                List<Parameter> disallowed = new List<Parameter>();
                disallowed.Add(new Parameter("ENCODING"));
                return disallowed;
            }
        }

        public override List<Parameter> Parameters
        {
            get
            {
                List<Parameter> parameters = base.Parameters;
                if (m_Binary.Uri == null)
                {                 
                    // NOTE: fixed a bug here that caused the ENCODING parameter
                    // to not be properly serialized if it was not included
                    // in the original object.
                    if (!m_Binary.Parameters.ContainsKey("ENCODING"))
                    {
                        Parameter p = new Parameter("ENCODING", "BASE64");
                        m_Binary.Parameters.Add("ENCODING", p);
                    }

                    parameters.Add(m_Binary.Parameters["ENCODING"] as Parameter);
                }
                return parameters;
            }
        }

        public override string SerializeToString()
        {
            if (m_Binary.Uri != null)
            {
                ISerializable serializer = SerializerFactory.Create(m_Binary.Uri);
                return serializer.SerializeToString();
            }
            else
            {             
                // NOTE: fixed a bug pointed out by Tony Dubey that caused binary data
                // to be converted to a UTF8 string before being serialized into
                // a BASE64 char array (which caused data loss).
                return Encode(m_Binary.Data);
            }
        }        

        #endregion
    }
}
