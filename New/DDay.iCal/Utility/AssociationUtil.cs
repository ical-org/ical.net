using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class AssociationUtil
    {
        #region Static Public Methods

        static public void AssociateItem(object item, ICalendarObject objectToAssociate)
        {
            if (item is ICalendarDataType)
                ((ICalendarDataType)item).AssociatedObject = objectToAssociate;
            else if (item is ICalendarObject)
                ((ICalendarObject)item).Parent = objectToAssociate;
        }

        static public void DeassociateItem(object item)
        {
            if (item is ICalendarDataType)
                ((ICalendarDataType)item).AssociatedObject = null;
            else if (item is ICalendarObject)
                ((ICalendarObject)item).Parent = null;
        }

        #endregion
    }
}
