using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class iCalDateTimeTest
    {
        /// <summary>
        /// Tests bug 3191956 - iCalDateTime.HasTime inconsistency
        /// See https://sourceforge.net/tracker/?func=detail&aid=3191956&group_id=187422&atid=921236
        /// </summary>
        [Test]
        public void Bug3191956()
        {
            var queue = new Queue<iCalDateTime>();
            for (int i = 0; i < 4; i++)
            {
                var dateTime = new iCalDateTime(2011, 1, 1);
                dateTime.HasTime = false;
                queue.Enqueue(dateTime);
            }

            IDateTime dt = queue.Dequeue();
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddHours(0);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddHours(24);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddHours(1);
            Assert.IsTrue(dt.HasTime);

            dt = queue.Dequeue();
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddMinutes(0);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddMinutes(1440);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddMinutes(1);
            Assert.IsTrue(dt.HasTime);

            dt = queue.Dequeue();
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddSeconds(0);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddSeconds(86400);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddSeconds(1);
            Assert.IsTrue(dt.HasTime);

            dt = queue.Dequeue();
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddMilliseconds(0);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddMilliseconds(86400000);
            Assert.IsFalse(dt.HasTime);
            dt = dt.AddMilliseconds(1);
            Assert.IsTrue(dt.HasTime);
        }
    }
}
