using System;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Serialization.iCalendar.Serializers.DataTypes
{
    public abstract class DataTypeSerializer : SerializerBase
    {
        protected DataTypeSerializer() {}

        protected DataTypeSerializer(SerializationContext ctx) : base(ctx) {}

        protected virtual ICalendarDataType CreateAndAssociate()
        {
            // Create an instance of the object
            var dt = Activator.CreateInstance(TargetType) as ICalendarDataType;
            if (dt != null)
            {
                var associatedObject = SerializationContext.Peek() as ICalendarObject;
                if (associatedObject != null)
                {
                    dt.AssociatedObject = associatedObject;
                }

                return dt;
            }
            return null;
        }
    }
}