using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.General;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Ical.Net.UnitTests
{
    public class RequestStatusTest
    {
        private static IEnumerable<ITestCaseData> ConstructorTestCases() 
        {
            var emptyRequestStatus = new RequestStatus();
            yield return new TestCaseData(emptyRequestStatus, null, null, null)
                .SetName("Default request status constructor has null properties");

            var invalidSerializedValue = new RequestStatus("ABC");
            yield return new TestCaseData(invalidSerializedValue, null, null, null)
                .SetName("Constructor with invalid serialised text has null properties");
        }

        [Test, TestCaseSource(nameof(ConstructorTestCases))]
        public void ConstructorTests(RequestStatus incoming, string expectedDescription, string expectedStatusCode, string expectedExtraData) 
        {
            Assert.AreEqual(incoming.StatusCode, expectedStatusCode);
            Assert.AreEqual(incoming.Description, expectedDescription);
            Assert.AreEqual(incoming.ExtraData, expectedExtraData);
        }

        [Test, Ignore("Understand what 'valid' values RequestStatusSerializer expects")]
        public void InitialObjectState_Constructor_ValidSerialisedValue() { }

        private static IEnumerable<ITestCaseData> ToStringTestCases() 
        {
            var emptyRequestStatus = new RequestStatus();
            yield return new TestCaseData(emptyRequestStatus, ";")
                .SetName("ToString() for empty request status should return semicolon");

            var emptyStringPropertiesRequestStatus = new RequestStatus()
            {
                Description = "",
                ExtraData = ""
            };
            yield return new TestCaseData(emptyStringPropertiesRequestStatus, ";")
                .SetName("ToString() for request status with empty string properties should return semicolon");

            var nonEmptyStringPropertiesRequestStatus = new RequestStatus() 
            {
                Description = "ABC Description",
                ExtraData = "ABC ExtraData"
            };
            yield return new TestCaseData(nonEmptyStringPropertiesRequestStatus, ";ABC Description;ABC ExtraData")
                .SetName("ToString() for request status should return semicolon separated values");
        }

        [Test, TestCaseSource(nameof(ToStringTestCases))]
        public void ToStringTests(RequestStatus incoming, string expected) 
        {
            Assert.AreEqual(expected, incoming.ToString());
        }

        private static IEnumerable<ITestCaseData> EqualsTestCases() 
        {
            var requestStatus = new RequestStatus();
            yield return new TestCaseData(requestStatus, requestStatus, true)
                .SetName("Equals() same request status should return true");

            yield return new TestCaseData(requestStatus, new RequestStatus(), true)
                .SetName("Equals() for request statuses with equal properties should return true");

            var secondReferenceVariable = requestStatus;
            yield return new TestCaseData(requestStatus, secondReferenceVariable, true)
                .SetName("Equals() for two variables referencing the same object should return true");

            var requestStatusWithPropertyValues = new RequestStatus() 
            {
                Description = "ABC Description"
            };
            yield return new TestCaseData(requestStatus, requestStatusWithPropertyValues, false)
                .SetName("Equals() for request statuses with inequal properties should return false");
        }

        [Test, TestCaseSource(nameof(EqualsTestCases))]
        public void EqualsGenericTests(RequestStatus incoming, RequestStatus other, bool expected) 
        {
            Assert.AreEqual(expected, incoming.Equals(other));
        }

        [Test, TestCaseSource(nameof(EqualsTestCases))]
        public void EqualsTests(RequestStatus incoming, object other, bool expected) 
        {
            Assert.AreEqual(expected, incoming.Equals((object) other));
        }

        private static IEnumerable<ITestCaseData> GetHashCodeTestCases() 
        {
            var emptyRequestStatus = new RequestStatus();
            yield return new TestCaseData(emptyRequestStatus, 0)
                .SetName("GetHashCode() for empty request status");

            var emptyPropertiesRequestStatus = new RequestStatus() 
            {
                Description = "",
                ExtraData = "",
                StatusCode = new StatusCode()
            };
            yield return new TestCaseData(emptyPropertiesRequestStatus, 666477112)
                .SetName("GetHashCode() for request status with empty properties");

            var normalRequestStatus = new RequestStatus() 
            {
                Description = "ABC Description",
                ExtraData = "ABC ExtraData",
                StatusCode = new StatusCode()
            };
            yield return new TestCaseData(normalRequestStatus, -150101454)
                .SetName("GetHashCode() for initialised request status");
        }

        [Test, TestCaseSource(nameof(GetHashCodeTestCases))]
        public void GetHashCodeTests(RequestStatus incoming, int expected) 
        {
            Assert.AreEqual(expected, incoming.GetHashCode());
        }      

        private class Copyable : ICopyable
        {
            public T Copy<T>() { return default(T); }

            public void CopyFrom(ICopyable obj) { }
        }

        private static IEnumerable<ITestCaseData> CopyFrom_InvalidTestCases() 
        {
            yield return new TestCaseData(new RequestStatus(), null).SetName("Request status CopyFrom() null");
            yield return new TestCaseData(new RequestStatus(), new Copyable()).SetName("Request status CopyFrom() non-IRequestStatus type");
        }

        [Test, TestCaseSource(nameof(CopyFrom_InvalidTestCases))]
        public void CopyFromTests(RequestStatus incoming, ICopyable copyable) 
        {
            incoming.CopyFrom(copyable);

            Assert.IsNull(incoming.StatusCode);
            Assert.IsNull(incoming.Description);
            Assert.IsNull(incoming.ExtraData);
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

        [Test, TestCaseSource(nameof(CopyFromTestCases))]
        public void CopyFromTests(RequestStatus incoming, ICopyable copyable, string expectedDescription, string expectedExtraData) 
        {
            incoming.CopyFrom(copyable);

            Assert.AreEqual(expectedDescription, incoming.Description);
            Assert.AreEqual(expectedExtraData, incoming.ExtraData);
            Assert.IsNotNull(incoming.StatusCode);
        }
    }
}
