using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface ITestProvider
    {
        ITest[] Tests { get; }
    }
}
