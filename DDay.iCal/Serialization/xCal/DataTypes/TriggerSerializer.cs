using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.xCal.DataTypes
{
    public class TriggerSerializer : FieldSerializer
    {
        #region Private Fields

        private Trigger m_Trigger;

        #endregion

        #region Constructors

        public TriggerSerializer(Trigger t)
            : base(t)
        {
            this.m_Trigger = t;
        }

        #endregion

        #region ISerializable Members

        public override List<Parameter> Parameters
        {
            get
            {
                List<Parameter> Parameters = base.Parameters;
                if (m_Trigger.ValueType() == typeof(Date_Time))
                    Parameters.Add(new Parameter("VALUE", "DATE-TIME"));
                if (m_Trigger.Related == Trigger.TriggerRelation.END)
                    Parameters.Add(new Parameter("RELATED", "END"));
                return Parameters;
            }
        }

        public override List<Parameter> DisallowedParameters
        {
            get
            {
                List<Parameter> disallowed = base.DisallowedParameters;
                disallowed.Add(new Parameter("VALUE"));
                disallowed.Add(new Parameter("RELATED"));
                return disallowed;
            }
        }

        public override string SerializeToString()
        {
            string value = string.Empty;
            ISerializable serializer = null;
            if (m_Trigger.ValueType() == typeof(Date_Time))            
                serializer = new Date_TimeSerializer(m_Trigger.DateTime);                
            else
                serializer = new FieldSerializer(m_Trigger.Duration);

            if (serializer != null)
                value += serializer.SerializeToString();

            return value;
        }        

        #endregion
    }
}
