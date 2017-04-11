using Ical.Net.DataTypes;
using Ical.Net.Interfaces.General;
using NUnit.Framework;

namespace Ical.Net.UnitTests
{
    [TestFixture]
    public class RequestStatusTest
    {
        [Test]
        public void InitialObjectState_Constructor_Default()
        {
            var requestStatus = new RequestStatus();

            Assert.IsNull(requestStatus.StatusCode);
            Assert.IsNull(requestStatus.Description);
            Assert.IsNull(requestStatus.ExtraData);
        }

        [Test]
        public void InitialObjectState_Constructor_InvalidSerialisedValue() 
        {
            var requestStatus = new RequestStatus("ABC");

            Assert.IsNull(requestStatus.StatusCode);
            Assert.IsNull(requestStatus.Description);
            Assert.IsNull(requestStatus.ExtraData);
        }

        [Test, Ignore("Understand what 'valid' values RequestStatusSerializer expects")]
        public void InitialObjectState_Constructor_ValidSerialisedValue() { }

        [Test]
        public void ToString_ValidInputWithNullStatusCode()
        {
            var requestStatusA = new RequestStatus();
            var requestStatusB = new RequestStatus()
            {
                Description = "",
                ExtraData = ""
            };
            var requestStatusC = new RequestStatus() 
            {
                Description = "ABC Description",
                ExtraData = "ABC ExtraData"
            };

            Assert.AreEqual(";", requestStatusA.ToString());
            Assert.AreEqual(";", requestStatusB.ToString());
            Assert.AreEqual(";ABC Description;ABC ExtraData", requestStatusC.ToString());
        }

        [Test]
        public void EqualsGeneric() 
        {
            var requestStatusA = new RequestStatus();
            var requestStatusB = new RequestStatus();
            var requestStatusC = requestStatusA;
            var requestStatusD = new RequestStatus() 
            {
                Description = "ABC Description"
            };

            Assert.IsTrue(requestStatusA.Equals(requestStatusA));
            Assert.IsTrue(requestStatusA.Equals(requestStatusB));
            Assert.IsTrue(requestStatusA.Equals(requestStatusC));
            Assert.IsFalse(requestStatusA.Equals(requestStatusD));
        }

        [Test]
        public void EqualsNonGeneric() 
        {
            object requestStatusA = new RequestStatus();
            object requestStatusB = new RequestStatus();
            object requestStatusC = requestStatusA;
            object requestStatusD = new RequestStatus()
            {
                Description = "ABC Description"
            };

            Assert.IsFalse(requestStatusA.Equals(null));
            Assert.IsFalse(requestStatusA.Equals("Invalid type value"));
            Assert.IsTrue(requestStatusA.Equals(requestStatusA));
            Assert.IsTrue(requestStatusA.Equals(requestStatusB));
            Assert.IsTrue(requestStatusA.Equals(requestStatusC));
            Assert.IsFalse(requestStatusA.Equals(requestStatusD));
        }

        [Test]
        public void GetHashCode_ValidObject()
        {
            var requestStatusA = new RequestStatus();
            var requestStatusB = new RequestStatus() 
            {
                Description = "",
                ExtraData = "",
                StatusCode = new StatusCode()
            };
            var requestStatusC = new RequestStatus() 
            {
                Description = "ABC Description",
                ExtraData = "ABC ExtraData",
                StatusCode = new StatusCode()
            };

            Assert.AreEqual(0,  requestStatusA.GetHashCode());
            Assert.AreEqual(666477112, requestStatusB.GetHashCode());
            Assert.AreEqual(-150101454, requestStatusC.GetHashCode());
        }

        [Test]
        public void CopyFrom_NullInput() 
        {
            var requestStatus = new RequestStatus();
            requestStatus.CopyFrom(null);

            Assert.IsNull(requestStatus.StatusCode);
            Assert.IsNull(requestStatus.Description);
            Assert.IsNull(requestStatus.ExtraData);
        }

        private class Copyable : ICopyable
        {
            public T Copy<T>() { return default(T); }

            public void CopyFrom(ICopyable obj) { }
        }

        [Test]
        public void CopyFrom_NonIRequestStatusTypeInput() 
        {
            var requestStatus = new RequestStatus();
            requestStatus.CopyFrom(new Copyable());

            Assert.IsNull(requestStatus.StatusCode);
            Assert.IsNull(requestStatus.Description);
            Assert.IsNull(requestStatus.ExtraData);
        }

        [Test]
        public void CopyFrom_IRequestStatusTypeInput() 
        {
            var requestStatus = new RequestStatus();
            var requestStatusCopyable = new RequestStatus() 
            {
                Description = "XYZ Description",
                ExtraData = "XYZ ExtraData",
                StatusCode = new StatusCode()
            };

            requestStatus.CopyFrom(requestStatusCopyable);

            Assert.AreEqual("XYZ Description", requestStatus.Description);
            Assert.AreEqual("XYZ ExtraData", requestStatus.ExtraData);
            Assert.IsNotNull(requestStatus.StatusCode);
        }

        [Test]
        public void CopyFrom_IRequestStatusTypeInputWithNullStatusCode() 
        {
            var requestStatus = new RequestStatus()
            {
                Description = "ABC Description",
                ExtraData = "ABC ExtraData",
                StatusCode = new StatusCode()
            };
            var requestStatusCopyable = new RequestStatus() 
            {
                Description = "XYZ Description",
                ExtraData = "XYZ ExtraData",
                StatusCode = null
            };

            requestStatus.CopyFrom(requestStatusCopyable);

            Assert.AreEqual("XYZ Description", requestStatus.Description);
            Assert.AreEqual("XYZ ExtraData", requestStatus.ExtraData);
            Assert.IsNotNull(requestStatus.StatusCode);
        }
    }
}
