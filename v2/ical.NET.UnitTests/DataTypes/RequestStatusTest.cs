using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.General;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.UnitTests
{
    public class RequestStatusTest
    {
        [Test]
        public void NullStringConstructorThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new StatusCode((string)null));
        }

        [Test, TestCaseSource(nameof(ConstructorTestCases))]
        public void ConstructorTests(RequestStatus incoming, string expectedDescription, string expectedStatusCode, string expectedExtraData)
        {
            Assert.AreEqual(incoming.StatusCode, expectedStatusCode);
            Assert.AreEqual(incoming.Description, expectedDescription);
            Assert.AreEqual(incoming.ExtraData, expectedExtraData);
        }

        private static IEnumerable<ITestCaseData> ConstructorTestCases()
        {
            var empty = new RequestStatus();
            yield return new TestCaseData(empty, null, null, null)
                .SetName("Default request status constructor has null properties");

            var invalidSerializedValue = new RequestStatus("ABC");
            yield return new TestCaseData(invalidSerializedValue, null, null, null)
                .SetName("Constructor invoked with invalid serialised text has null properties");
        }

        [Test, Ignore("Understand what 'valid' values RequestStatusSerializer expects")]
        public void InitialObjectState_Constructor_ValidSerialisedValue() { }

        [Test, TestCaseSource(nameof(ToStringTestCases))]
        public void ToStringTests(RequestStatus incoming, string expected)
        {
            Assert.AreEqual(expected, incoming.ToString());
        }

        private static IEnumerable<ITestCaseData> ToStringTestCases()
        {
            var empty = new RequestStatus();
            yield return new TestCaseData(empty, null)
                .SetName("ToString() for empty request status")
                .Ignore("Unit test is correct. ToString() should return null");

            var emptyStringProperties = new RequestStatus()
            {
                Description = "",
                ExtraData = ""
            };
            yield return new TestCaseData(emptyStringProperties, ";")
                .SetName("ToString() for request status with empty string properties");

            var nonEmptyStringProperties = new RequestStatus()
            {
                Description = "ABC Description",
                ExtraData = "ABC ExtraData"
            };
            yield return new TestCaseData(nonEmptyStringProperties, ";ABC Description;ABC ExtraData")
                .SetName("ToString() for request status returns semicolon separated values");
        }

        [Test, TestCaseSource(nameof(EqualsTestCases))]
        public void EqualsGenericTests(RequestStatus incoming, RequestStatus other, bool expected)
        {
            Assert.AreEqual(expected, incoming.Equals(other));
        }

        [Test, TestCaseSource(nameof(EqualsTestCases))]
        public void EqualsTests(RequestStatus incoming, object other, bool expected)
        {
            Assert.AreEqual(expected, incoming.Equals(other));
        }

        private static IEnumerable<ITestCaseData> EqualsTestCases()
        {
            var requestStatus = new RequestStatus();
            yield return new TestCaseData(requestStatus, null, false)
                .SetName("Equals() compares with null");

            yield return new TestCaseData(requestStatus, requestStatus, true)
                .SetName("Equals() compares with itself");

            yield return new TestCaseData(requestStatus, new RequestStatus(), true)
                .SetName("Equals() compares request statuses with equal properties");

            var secondReferenceVariable = requestStatus;
            yield return new TestCaseData(requestStatus, secondReferenceVariable, true)
                .SetName("Equals() compares two variables referencing the same object");

            var requestStatusWithPropertyValues = new RequestStatus()
            {
                Description = "ABC Description"
            };
            yield return new TestCaseData(requestStatus, requestStatusWithPropertyValues, false)
                .SetName("Equals() compares request statuses with different property values");
        }

        [Test, TestCaseSource(nameof(GetHashCodeTestCases))]
        public void GetHashCodeTests(RequestStatus incoming, RequestStatus other, bool expected)
        {
            Assert.AreEqual(expected, incoming.GetHashCode() == other.GetHashCode());
        }

        private static IEnumerable<ITestCaseData> GetHashCodeTestCases()
        {
            var empty = new RequestStatus();
            var emptyOther = new RequestStatus();
            yield return new TestCaseData(empty, emptyOther, true)
                .SetName("GetHashCode() for empty request status");

            var emptyProperties = new RequestStatus()
            {
                Description = "",
                ExtraData = "",
                StatusCode = new StatusCode()
            };
            var emptyPropertiesOther = new RequestStatus()
            {
                Description = "",
                ExtraData = "",
                StatusCode = new StatusCode()
            };
            yield return new TestCaseData(emptyProperties, emptyPropertiesOther, true)
                .SetName("GetHashCode() for request status with empty properties");

            var normal = new RequestStatus()
            {
                Description = "ABC Description"
            };
            var normalOther = new RequestStatus()
            {
                Description = "ABC Description"
            };
            yield return new TestCaseData(normal, normalOther, true)
                .SetName("GetHashCode() for request status");

            yield return new TestCaseData(normal, emptyPropertiesOther, false)
                .SetName("GetHashCode() compare empty request status and request status with empty properties");
        }

        [Test, TestCaseSource(nameof(CopyFrom_InvalidTestCases))]
        public void CopyFromTests(RequestStatus incoming, ICopyable copyable)
        {
            incoming.CopyFrom(copyable);

            Assert.IsNull(incoming.StatusCode);
            Assert.IsNull(incoming.Description);
            Assert.IsNull(incoming.ExtraData);
        }

        private static IEnumerable<ITestCaseData> CopyFrom_InvalidTestCases()
        {
            yield return new TestCaseData(new RequestStatus(), null).SetName("Request status CopyFrom() null");
            yield return new TestCaseData(new RequestStatus(), new Copyable()).SetName("Request status CopyFrom() non-IRequestStatus type");
        }

        private class Copyable : ICopyable
        {
            public T Copy<T>() { return default(T); }

            public void CopyFrom(ICopyable obj) { }
        }

        [Test, TestCaseSource(nameof(CopyFromTestCases))]
        public void CopyFromTests(RequestStatus incoming, ICopyable copyable, string expectedDescription, string expectedExtraData)
        {
            incoming.CopyFrom(copyable);

            Assert.AreEqual(expectedDescription, incoming.Description);
            Assert.AreEqual(expectedExtraData, incoming.ExtraData);
            Assert.IsNotNull(incoming.StatusCode);
        }

        private static IEnumerable<ITestCaseData> CopyFromTestCases()
        {
            var emptyRequestStatus = new RequestStatus();
            var copySource = new RequestStatus()
            {
                Description = "XYZ Description",
                ExtraData = "XYZ ExtraData",
                StatusCode = new StatusCode()
            };
            yield return new TestCaseData(emptyRequestStatus, copySource, copySource.Description, copySource.ExtraData)
                .SetName("Empty request status copy from IRequestStatus type");

            var normalRequestStatus = new RequestStatus()
            {
                Description = "ABC Description",
                ExtraData = "ABC ExtraData",
                StatusCode = new StatusCode()
            };
            yield return new TestCaseData(normalRequestStatus, copySource, copySource.Description, copySource.ExtraData)
                .SetName("Request status copy from IRequestStatus type");

            var copySourceWithNullStatusCode = new RequestStatus()
            {
                Description = "XYZ Description",
                ExtraData = "XYZ ExtraData",
                StatusCode = null
            };
            yield return new TestCaseData(
                    normalRequestStatus, copySourceWithNullStatusCode,
                    copySourceWithNullStatusCode.Description, copySourceWithNullStatusCode.ExtraData)
                .SetName("Request status copy from IRequestStatus type with null StatusCode");
        }
    }
}
