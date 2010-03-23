using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class AssociationUtil
    {
        #region Static Private Fields

        static Dictionary<ICompositeList, EventHandler<ObjectEventArgs<object>>> m_ItemAddedDelegates = new Dictionary<ICompositeList, EventHandler<ObjectEventArgs<object>>>();
        static Dictionary<ICompositeList, EventHandler<ObjectEventArgs<object>>> m_ItemRemovedDelegates = new Dictionary<ICompositeList, EventHandler<ObjectEventArgs<object>>>();

        #endregion

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

        static public void AssociateList(ICompositeList list, ICalendarObject objectToAssociate)
        {
            if (list != null)
            {
                // Associate each item in the list with the parent
                foreach (object obj in list)
                    AssociateItem(obj, objectToAssociate);

                m_ItemAddedDelegates[list] = delegate(object sender, ObjectEventArgs<object> e)
                {
                    AssociateItem(e.Object, objectToAssociate);
                };

                m_ItemRemovedDelegates[list] = delegate(object sender, ObjectEventArgs<object> e)
                {
                    DeassociateItem(e.Object);
                };

                list.ItemAdded += m_ItemAddedDelegates[list];
                list.ItemRemoved += m_ItemRemovedDelegates[list];
            }
        }

        static public void DeassociateList(ICompositeList list)
        {
            if (list != null)
            {
                // Deassociate each item in the list
                foreach (object obj in list)
                    DeassociateItem(obj);

                if (m_ItemAddedDelegates.ContainsKey(list))
                {
                    list.ItemAdded -= m_ItemAddedDelegates[list];
                    m_ItemAddedDelegates.Remove(list);
                }
                if (m_ItemRemovedDelegates.ContainsKey(list))
                {
                    list.ItemRemoved -= m_ItemRemovedDelegates[list];
                    m_ItemRemovedDelegates.Remove(list);
                }
            }
        }

        #endregion
    }
}
