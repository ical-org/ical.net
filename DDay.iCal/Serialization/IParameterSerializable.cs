using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface IParameterSerializable
    {
        List<string> Parameters { get; }
        List<string> DisallowedParameters { get; }
    }
}
