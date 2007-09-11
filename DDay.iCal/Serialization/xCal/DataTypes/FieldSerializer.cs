using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Serialization.xCal.DataTypes
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

        public override void Serialize(System.Xml.XmlTextWriter xtw)
        {
            xtw.WriteString(DataType.ToString());            
        }

        #endregion
    }
}
