using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// Indicates that the property can serialize parameters.
    /// </summary>
    public interface IParameterSerializable
    {
        List<string> Parameters { get; }
        List<string> DisallowedParameters { get; }
    }
}
