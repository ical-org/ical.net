using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IObjectFactory
    {
        object Create(Type objectType, string value);
        object Create(Type objectType, string[] values);
    }
}
