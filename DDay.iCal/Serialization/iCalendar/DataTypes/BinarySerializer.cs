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
                    if (!m_Binary.Parameters.ContainsKey("ENCODING"))
                    {
                        Parameter p = new Parameter(m_Binary);
                        p.Name = "ENCODING";
                        p.Values.Add("BASE64");
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
                UTF8Encoding encoding = new UTF8Encoding();
                return Encode(encoding.GetString(m_Binary.Data));                
            }
        }        

        #endregion
    }
}
