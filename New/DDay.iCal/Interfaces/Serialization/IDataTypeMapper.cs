using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IDataTypeMapper
    {
        Type Map(object obj);
    }
}
