using System;

namespace DDay.iCal
{
    public class UIDFactory
    {
        virtual public string Build()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
