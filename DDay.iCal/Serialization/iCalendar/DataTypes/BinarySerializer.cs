using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class BinarySerializer : FieldSerializer
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

        public override List<string> DisallowedParameters
        {
            get
            {
                List<string> disallowed = new List<string>();
                disallowed.Add("ENCODING");
                return disallowed;
            }
        }

        public override List<string> Parameters
        {
            get
            {
                List<string> Parameters = base.Parameters;
                if (m_Binary.Uri == null)
                    Parameters.Add("ENCODING=BASE64");
                return Parameters;
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
                return Convert.ToBase64String(m_Binary.Data);                
            }
        }        

        #endregion
    }
}
