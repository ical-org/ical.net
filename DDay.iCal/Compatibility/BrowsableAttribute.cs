using System;

namespace System.ComponentModel
{

#if NETCF
    [AttributeUsageAttribute(AttributeTargets.All)]
    public sealed class BrowsableAttribute : Attribute
    {
        public BrowsableAttribute(bool value)
        {
        }
    }
#endif

}