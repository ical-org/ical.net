﻿using System.Collections.Generic;

namespace ical.net.Serialization.iCalendar.Processors
{
    public class CompositeProcessor<T> : List<CompositeProcessor<T>>
    {
        public CompositeProcessor() {}

        public CompositeProcessor(IEnumerable<CompositeProcessor<T>> processors)
        {
            AddRange(processors);
        }

        public virtual void PreSerialization(T obj)
        {
            foreach (var p in this)
            {
                p.PreSerialization(obj);
            }
        }

        public virtual void PostSerialization(T obj)
        {
            foreach (var p in this)
            {
                p.PostSerialization(obj);
            }
        }

        public virtual void PreDeserialization(T obj)
        {
            foreach (var p in this)
            {
                p.PreDeserialization(obj);
            }
        }

        public virtual void PostDeserialization(T obj)
        {
            foreach (var p in this)
            {
                p.PostDeserialization(obj);
            }
        }
    }
}