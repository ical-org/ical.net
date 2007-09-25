using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.xCal.DataTypes
{
    public class RDateSerializer : FieldSerializer
    {
        #region Private Fields

        private RDate m_RDate;

        #endregion

        #region Constructors

        public RDateSerializer(RDate rdate)
            : base(rdate)
        {
            this.m_RDate = rdate;
        }

        #endregion

        #region Overrides

        public override string SerializeToString()
        {
            List<string> values = new List<string>();                        
            foreach (Period p in m_RDate.Periods)
            {
                IXCalSerializable serializer = SerializerFactory.Create(p);
                if (serializer != null)
                    values.Add(serializer.SerializeToString());
            }

            return string.Join(",", values.ToArray());            
        }

        #endregion
    }
}
