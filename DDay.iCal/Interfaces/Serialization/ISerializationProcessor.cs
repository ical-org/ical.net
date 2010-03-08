using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ISerializationProcessor<T>
    {
        void Process(T obj);
    }
}
