using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    public class DateUtilTests
    {
        [Test]
        public void FooTest()
        {
            var otherZone = "大阪、札幌、東京";
            var foo = DateUtil.GetEnglishNameForTimeZone(otherZone);
        }
    }
}
