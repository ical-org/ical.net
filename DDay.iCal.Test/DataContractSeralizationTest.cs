using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using DDay.iCal.DataTypes;
using System.IO;
using System.Runtime.Serialization;
using System.Collections;

namespace DDay.iCal.Test
{
    [TestFixture]
    public class DataContractSeralizationTest
    {
        static public void SerializeTest(object obj, string filename)
        {
            string dir = @"Temp\Serialization";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            FileStream fs = new FileStream(Path.Combine(dir, filename), FileMode.Create, FileAccess.Write);

            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            serializer.WriteObject(fs, obj);

            object deserializedObject = null;

            fs.Close();
            fs = new FileStream(Path.Combine(dir, filename), FileMode.Open, FileAccess.Read);

            deserializedObject = serializer.ReadObject(fs);

            AssertEquality(obj, deserializedObject);
        }

        static public void AssertEquality(object obj1, object obj2)
        {
            if (obj1 is IComparable)
            {
                Assert.AreEqual(0, ((IComparable)obj1).CompareTo(obj2));
            }
            else if (
                obj1 is IEnumerable &&
                obj2 is IEnumerable)
            {
                IEnumerator currEnum = ((IEnumerable)obj1).GetEnumerator();
                IEnumerator copyEnum = ((IEnumerable)obj2).GetEnumerator();
                if (currEnum != null &&
                    copyEnum != null)
                {
                    while (currEnum.MoveNext())
                    {
                        Assert.IsTrue(copyEnum.MoveNext(), "Collection sizes do not match.");
                        AssertEquality(currEnum.Current, copyEnum.Current);
                    }
                    Assert.IsFalse(copyEnum.MoveNext(), "Collection sizes do not match.");
                }
            }
            else
            {
                Assert.AreEqual(obj1, obj2, "Original object and copy object are not equal.");
            }
        }

        [Test, Category("DataContractSerialization")]
        public void Binary1()
        {
            Binary b = new Binary();
            b.Data = new byte[] { 1, 2, 3, 4 };
            b.Name = "ATTACH";
            SerializeTest(b, "Binary1.xml");
        }

        [Test, Category("DataContractSerialization")]
        public void Cal_Address1()
        {
            Cal_Address ca = new Cal_Address();
            ca.Value = "mailto:test123@test.com";
            SerializeTest(ca, "Cal_Address1.xml");
        }
    }
}
