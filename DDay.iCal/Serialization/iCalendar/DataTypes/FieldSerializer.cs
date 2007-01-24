using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Objects;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class FieldSerializer : DataTypeSerializer 
    {        
        #region Constructors

        public FieldSerializer(DDay.iCal.DataTypes.iCalDataType dataType) : base(dataType) {}

        #endregion

        #region Overrides

        public override string SerializeToString()
        {
            return DataType.ToString();
        }

        public override void Serialize(Stream stream, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(SerializeToString());
            if (data.Length > 0)
                stream.Write(data, 0, data.Length);
        }

        #endregion
    }
}
