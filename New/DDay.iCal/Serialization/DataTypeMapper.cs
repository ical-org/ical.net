using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class DataTypeMapper :
        IDataTypeMapper
    {
        #region IDataTypeMapper Members

        virtual public Type Map(object obj)
        {
            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null &&
                p.Name != null)
            {
                switch (p.Name.ToUpper())
                {
                    case "STATUS":
                        if (p.Parent is IEvent)
                            return typeof(EventStatus);
                        // FIXME: add other statuses here
                        break;
                    case "DTSTART":
                    case "DTEND":
                    case "CREATED":
                        return typeof(iCalDateTime);
                    //case "DURATION":
                    //    return typeof(IDuration);
                    //case "GEO":
                    //    return typeof(IGeographicLocation);
                    //case "LOCATION":
                    //    return typeof(IText);
                    //case "RESOURCES":
                    //    return typeof(ITextCollection[]);
                }
            }
            return null;
        }

        #endregion
    }
}
