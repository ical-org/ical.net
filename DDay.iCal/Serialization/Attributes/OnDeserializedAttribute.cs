using System;
using System.Collections.Generic;
using System.Text;

#if !DATACONTRACT
namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class OnDeserializedAttribute : Attribute
    {
    }    
}
#endif
