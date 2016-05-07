using System;

namespace Ical.Net.Factory
{
    public class UidFactory
    {
        virtual public string Build()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
