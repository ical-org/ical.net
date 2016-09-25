using System;
using ical.net.Interfaces.DataTypes;
using ical.net.Interfaces.General;
using ical.net.Interfaces.Serialization;

namespace ical.net.Serialization.iCalendar.Serializers.DataTypes
{
    public abstract class DataTypeSerializer : SerializerBase
    {
        protected DataTypeSerializer() {}

        protected DataTypeSerializer(ISerializationContext ctx) : base(ctx) {}

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