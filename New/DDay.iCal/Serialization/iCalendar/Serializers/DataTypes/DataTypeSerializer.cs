using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;

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

        #region Content Validation

        virtual public void CheckRange(string name, ICollection<int> values, int min, int max)
        {
            bool allowZero = (min == 0 || max == 0) ? true : false;
            foreach (int value in values)
                CheckRange(name, value, min, max, allowZero);
        }

        virtual public void CheckRange(string name, int value, int min, int max)
        {
            CheckRange(name, value, min, max, (min == 0 || max == 0) ? true : false);
        }

        virtual public void CheckRange(string name, int value, int min, int max, bool allowZero)
        {
            if (value != int.MinValue && (value < min || value > max || (!allowZero && value == 0)))
                throw new ArgumentException(name + " value " + value + " is out of range. Valid values are between " + min + " and " + max + (allowZero ? "" : ", excluding zero (0)") + ".");
        }

        virtual public void CheckMutuallyExclusive(string name1, string name2, object obj1, object obj2)
        {
            if (obj1 == null || obj2 == null)
                return;
            else
            {
                bool has1 = false,
                    has2 = false;

                Type t1 = obj1.GetType(),
                    t2 = obj2.GetType();

                FieldInfo fi1 = t1.GetField("MinValue");
                FieldInfo fi2 = t1.GetField("MinValue");

                has1 = fi1 == null || !obj1.Equals(fi1.GetValue(null));
                has2 = fi2 == null || !obj2.Equals(fi2.GetValue(null));
                if (has1 && has2)
                    throw new ArgumentException("Both " + name1 + " and " + name2 + " cannot be supplied together; they are mutually exclusive.");
            }
        }

        #endregion

        #region Protected Methods

        protected ICalendarDataType CreateAndAssociate()
        {
            // Create an instance of the object
            ICalendarDataType dt = Activator.CreateInstance(TargetType) as ICalendarDataType;
            if (dt != null)
            {

                ICalendarObject associatedObject = SerializationContext.Peek() as ICalendarObject;
                if (associatedObject != null)
                    dt.AssociateWith(associatedObject);

                return dt;
            }
            return null;
        }

        #endregion
    }
}
