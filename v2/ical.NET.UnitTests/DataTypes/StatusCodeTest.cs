using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.General;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.UnitTests.DataTypes
{
    public class StatusCodeTest
    {
        [Test, TestCaseSource(nameof(ConstructorTestCases))]
        public void ConstructorTests(StatusCode incoming, int[] expectedParts)
        {
            Assert.IsTrue(incoming.Parts.SequenceEqual(expectedParts));
        }

        private static IEnumerable<ITestCaseData> ConstructorTestCases()
        {
            var empty = new StatusCode();
            yield return new TestCaseData(empty, new int[0])
                .SetName("Default status code constructor has has Parts property");

            var nullIntArray = new StatusCode((int[])null);
            yield return new TestCaseData(nullIntArray, new int[0])
                .SetName("Constructor invoked with null array has empty Parts property");

            var zeroLengthArray = new StatusCode(new int[0]);
            yield return new TestCaseData(zeroLengthArray, new int[0])
                .SetName("Constructor invoked with zero length array has empty Parts property");

            var validArray = new StatusCode(new int[3] { 1, 2, 3 });
            yield return new TestCaseData(validArray, new int[3] { 1, 2, 3 })
                .SetName("Constructor invoked with a valid array");

            var nullString = new StatusCode((string)null);
            yield return new TestCaseData(nullString, new int[0])
                .SetName("Constructor invoked with null string has empty Parts property");

            var invalidSerializedValue = new StatusCode("ABC");
            yield return new TestCaseData(invalidSerializedValue, new int[0])
                .SetName("Constructor invoked with invalid serialised text has empty Parts property");

            var validSerializedValue = new StatusCode("3.4.5");
            yield return new TestCaseData(validSerializedValue, new int[3] { 3, 4, 5 })
                .SetName("Constructor invoked with a valid serialised string");
        }

        [Test, TestCaseSource(nameof(EqualsTestCases))]
        public void EqualsGenericTests(StatusCode incoming, StatusCode other, bool expected)
        {
            Assert.AreEqual(expected, incoming.Equals(other));
        }

        [Test, TestCaseSource(nameof(EqualsTestCases))]
        public void EqualsTests(StatusCode incoming, object other, bool expected)
        {
            Assert.AreEqual(expected, incoming.Equals((object)other));
        }

        private static IEnumerable<ITestCaseData> EqualsTestCases()
        {
            var statusCode = new StatusCode();
            yield return new TestCaseData(statusCode, null, false)
                .SetName("Equals() compares with null");

            yield return new TestCaseData(statusCode, statusCode, true)
                .SetName("Equals() compares with itself");

            yield return new TestCaseData(statusCode, new StatusCode(), true)
                .SetName("Equals() compares status codes with equal properties");

            var secondReferenceVariable = statusCode;
            yield return new TestCaseData(statusCode, secondReferenceVariable, true)
                .SetName("Equals() compares two variables referencing the same object");

            yield return new TestCaseData(statusCode, new StatusCode(new int[1] { 1 }), false)
                .SetName("Equals() compares status codes with different property values");
        }

        [Test, TestCaseSource(nameof(GetHashCodeTestCases))]
        public void GetHashCodeTests(StatusCode incoming, StatusCode other, bool expected)
        {
            Assert.AreEqual(expected, incoming.GetHashCode() == other.GetHashCode());
        }

        private static IEnumerable<ITestCaseData> GetHashCodeTestCases()
        {
            var empty = new StatusCode();
            var emptyOther = new StatusCode();
            yield return new TestCaseData(empty, emptyOther, true).SetName("GetHashCode() for empty StatusCode");

            var typical = new StatusCode(new int[] { 101, 202, 303 });
            var typicalOther = new StatusCode(new int[] { 101, 202, 303 });
            yield return new TestCaseData(typical, typicalOther, true).SetName("GetHashCode() for typical StatusCode");
        }

        [Test, TestCaseSource(nameof(GetIndividualPartTestCases))]
        public void GetIndividualPartTests(StatusCode incoming, int[] expected)
        {
            Assert.AreEqual(expected[0], incoming.Primary);
            Assert.AreEqual(expected[1], incoming.Secondary);
            Assert.AreEqual(expected[2], incoming.Tertiary);
        }

        private static IEnumerable<ITestCaseData> GetIndividualPartTestCases()
        {
            var empty = new StatusCode();
            yield return new TestCaseData(empty, new int[3] { 0, 0, 0 })
                .SetName("Individual parts for empty status code");

            var primaryOnly = new StatusCode(new int[1] { 10 });
            yield return new TestCaseData(primaryOnly, new int[3] { 10, 0, 0 })
                .SetName("Individual parts when primary only part defined in status code");

            var typical = new StatusCode(new int[3] { 10, 20, 30 });
            yield return new TestCaseData(typical, new int[3] { 10, 20, 30 })
                .SetName("Individual parts for typical status code");
        }

        [Test, TestCaseSource(nameof(CopyFromTestCases))]
        public void CopyFromTests(StatusCode incoming, ICopyable copyable, int[] expected)
        {
            incoming.CopyFrom(copyable);
            Assert.AreEqual(expected, incoming.Parts);
  
        }

        private static IEnumerable<ITestCaseData> CopyFromTestCases()
        {
            yield return new TestCaseData(new StatusCode(), null, new int[0])
                .SetName("StatusCode copy from null");

            yield return new TestCaseData(new StatusCode(), new Copyable(), new int [0])
                .SetName("StatusCode copy from non-IStatusCode type");

            yield return new TestCaseData(new StatusCode(), new StatusCode(new int[] { 10, 20, 30 }), new int[] { 10, 20, 30 })
                .SetName("StatusCode copy from IStatusCode type");
        }

        private class Copyable : ICopyable
        {
            public T Copy<T>() { return default(T); }

            public void CopyFrom(ICopyable obj) { }
        }

        [Test, TestCaseSource(nameof(ToStringTestCases))]
        public void ToStringTests(StatusCode incoming, string expected)
        {
            Assert.AreEqual(expected, incoming.ToString());
        }

        private static IEnumerable<ITestCaseData> ToStringTestCases()
        {
            var empty = new StatusCode();
            yield return new TestCaseData(empty, string.Empty)
                .SetName("ToString() for empty StatusCode");

            var typical = new StatusCode(new int[] { 10, 20, 30 });
            yield return new TestCaseData(typical, "10.20.30")
                .SetName("ToString() for typical StatusCode");
        }
    }
}
