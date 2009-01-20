using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IKeyedObject<T>
    {
        T Key { get; }
    }
}
