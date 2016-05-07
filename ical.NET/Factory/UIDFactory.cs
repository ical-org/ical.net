using System;

namespace Ical.Net.Factory
{
    public class UidFactory
    {
        public virtual string Build()
        {
            return Guid.NewGuid().ToString();
        }
    }
}