using System;
using System.Runtime.InteropServices;

namespace System.Diagnostics
{

#if NETCF
    [AttributeUsageAttribute(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Delegate, AllowMultiple = true)]
    [ComVisibleAttribute(true)]
    public sealed class DebuggerDisplayAttribute : Attribute
    {
        public DebuggerDisplayAttribute(string value)
        {
        }
    }
#endif

}