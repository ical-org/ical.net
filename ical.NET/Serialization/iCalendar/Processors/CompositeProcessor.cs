using System.Collections.Generic;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization.iCalendar.Processors
{
    public class CompositeProcessor<T> :
        List<ISerializationProcessor<T>>,
        ISerializationProcessor<T>
    {
        #region Constructors

        public CompositeProcessor()
        {
        }

        public CompositeProcessor(IEnumerable<ISerializationProcessor<T>> processors)
        {
            AddRange(processors);
        }

        #endregion

        #region ISerializationProcessor<T> Members

        virtual public void PreSerialization(T obj)
        {
            foreach (var p in this)
                p.PreSerialization(obj);
        }

        virtual public void PostSerialization(T obj)
        {
            foreach (var p in this)
                p.PostSerialization(obj);
        }

        virtual public void PreDeserialization(T obj)
        {
            foreach (var p in this)
                p.PreDeserialization(obj);
        }

        virtual public void PostDeserialization(T obj)
        {
            foreach (var p in this)
                p.PostDeserialization(obj);
        }

        #endregion
    }
}
