using System;

namespace DDay.iCal.Serialization.iCalendar
{
    public abstract class DataTypeSerializer :
        SerializerBase
    {
        #region Constructors

        public DataTypeSerializer()
        {
        }

        public DataTypeSerializer(ISerializationContext ctx) : base(ctx)
        {
        }

        #endregion        

        #region Protected Methods

        virtual protected ICalendarDataType CreateAndAssociate()
        {
            // Create an instance of the object
            ICalendarDataType dt = Activator.CreateInstance(TargetType) as ICalendarDataType;
            if (dt != null)
            {
                ICalendarObject associatedObject = SerializationContext.Peek() as ICalendarObject;
                if (associatedObject != null)
                    dt.AssociatedObject = associatedObject;
                
                return dt;
            }
            return null;
        }

        #endregion
    }
}
