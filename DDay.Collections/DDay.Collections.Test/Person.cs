using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace DDay.Collections.Test
{
    public class Person :
        IGroupedObject<long>,
        IComparer
    {
        public event EventHandler<ObjectEventArgs<long, long>> GroupChanged;

        protected void OnKeyChanged(long oldId, long newId)
        {
            if (GroupChanged != null)
                GroupChanged(this, new ObjectEventArgs<long, long>(oldId, newId));
        }

        long _Key;
        virtual public long Group
        {
            get { return _Key; }
            set
            {
                if (!object.Equals(_Key, value))
                {
                    long oldID = _Key;
                    _Key = value;
                    OnKeyChanged(oldID, _Key);
                }
            }
        }

        virtual public string Name { get; set; }

        virtual public int Compare(object x, object y)
        {
            if (x is Person && y is Person)
            {
                return string.Compare(((Person)x).Name, ((Person)y).Name);
            }
            return 0;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
