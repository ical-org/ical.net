using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization.iCalendar
{
    public class CompositeProcessor<T> :
        List<ISerializationProcessor<T>>,
        ISerializationProcessor<T>
    {
        #region ISerializationProcessor<T> Members

        virtual public void PreSerialization(T obj)
        {
            foreach (ISerializationProcessor<T> p in this)
                p.PreSerialization(obj);
        }

        virtual public void PostSerialization(T obj)
        {
            foreach (ISerializationProcessor<T> p in this)
                p.PostSerialization(obj);
        }

        virtual public void PreDeserialization(T obj)
        {
            foreach (ISerializationProcessor<T> p in this)
                p.PreDeserialization(obj);
        }

        virtual public void PostDeserialization(T obj)
        {
            foreach (ISerializationProcessor<T> p in this)
                p.PostDeserialization(obj);
        }

        #endregion
    }
}
