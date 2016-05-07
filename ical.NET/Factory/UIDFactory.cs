using System;

namespace Ical.Net.Factory
{
    public class UIDFactory
    {
        virtual public string Build()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
