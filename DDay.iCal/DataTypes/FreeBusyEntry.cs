using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal
{
    public class FreeBusyEntry :
        Period,
        IFreeBusyEntry
    {
        #region Private Fields

        FreeBusyType _Type;

        #endregion

        #region Constructors

        public FreeBusyEntry() : base() {}
        public FreeBusyEntry(IPeriod period, FreeBusyType type) : base()
        {
            CopyFrom(period);
            Type = type;
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            IFreeBusyEntry fb = obj as IFreeBusyEntry;
            if (fb != null)
            {
                Type = fb.Type;
            }
        }

        #endregion

        #region IFreeBusyEntry Members

        virtual public FreeBusyType Type
        {
            get { return _Type; }
            set { _Type = value; }
        }

        #endregion
    }
}
