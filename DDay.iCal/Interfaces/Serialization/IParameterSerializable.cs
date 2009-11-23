using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// Indicates that the property can serialize parameters.
    /// </summary>
    public interface IParameterSerializable
    {
        List<Parameter> Parameters { get; }
        List<Parameter> DisallowedParameters { get; }
    }
}
