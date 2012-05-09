using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace DDay.Collections.Test
{
    [TestFixture]
    public class GroupedCollectionTests
    {
        IGroupedList<long, Person> _People;
        IGroupedCollection<long, Doctor> _Doctors;
        Person _JonSchmidt;
        Person _BobRoss;
        Person _ForrestGump;
        Person _MichaelJackson;
        Person _DoogieHowser;

        [SetUp]
        public void Setup()
        {
            _JonSchmidt = new Person() { Group = 1, Name = "Jon Schmidt" };
            _BobRoss = new Person() { Group = 2, Name = "Bob Ross" };
            _ForrestGump = new Doctor() { Group = 3, Name = "Forrest Gump", ProviderNumber = "123456" };
            _MichaelJackson = new Person() { Group = 4, Name = "Michael Jackson" };
            _DoogieHowser = new Doctor() { Group = 5, Name = "Doogie Howser", ProviderNumber = "234567" };

            _People = new GroupedList<long, Person>();

            _People.Add(_ForrestGump);
            _People.Add(_JonSchmidt);
            _People.Add(_BobRoss);
            _People.Add(_DoogieHowser);
            _People.Add(_MichaelJackson);

            _Doctors = new GroupedCollectionProxy<long, Person, Doctor>(_People);
        }

        /// <summary>
        /// Ensures that the Add() correctly adds items when
        /// called from the original list.
        /// </summary>
        [Test]
        public void Add1()
        {
            var newDoctor = new Doctor() { Group = 5, Name = "New Doctor", ProviderNumber = "23456" };
            Assert.AreEqual(5, _People.Count);
            Assert.AreEqual(2, _Doctors.Count);
            _People.Add(newDoctor);
            Assert.AreEqual(6, _People.Count);
            Assert.AreEqual(3, _Doctors.Count);
        }

        /// <summary>
        /// Tests the basic operation of the AllOf() method.
        /// </summary>
        [Test]
        public void AllOf1()
        {
            Assert.AreEqual(1, _People.AllOf(1).Count());
            Assert.AreEqual(1, _People.AllOf(2).Count());
            Assert.AreEqual(1, _People.AllOf(3).Count());
            Assert.AreEqual(1, _People.AllOf(4).Count());
            Assert.AreEqual(1, _People.AllOf(5).Count());
        }

        /// <summary>
        /// Tests the AllOf() method after one of the 
        /// object's keys has changed.
        /// </summary>
        [Test]
        public void AllOf2()
        {
            Assert.AreEqual(1, _People.AllOf(4).Count());
            Assert.AreEqual(1, _People.AllOf(5).Count());
            _MichaelJackson.Group = 5;
            Assert.AreEqual(2, _People.AllOf(5).Count());
            Assert.AreEqual(0, _People.AllOf(4).Count());
        }

        /// <summary>
        /// Tests the basic function of the Count property.
        /// </summary>
        [Test]
        public void Count1()
        {
            Assert.AreEqual(5, _People.Count);
        }

        /// <summary>
        /// Ensures the Count property works as expected with proxied lists.
        /// </summary>
        [Test]
        public void CountProxy1()
        {
            Assert.AreEqual(2, _Doctors.Count);
        }

        /// <summary>
        /// Ensure that the KeyedList enumerator properly enumerates the items in the list.
        /// </summary>
        [Test]
        public void Enumeration1()
        {
            var people = new Person[] { _ForrestGump, _JonSchmidt, _BobRoss, _DoogieHowser, _MichaelJackson };
            int i = 0;
            foreach (var person in _People)
            {
                Assert.AreSame(people[i++], person);
            }
        }

        /// <summary>
        /// Ensures that the indexer properly retrieves the items at the specified index.
        /// </summary>
        [Test]
        public void Indexer1()
        {
            Assert.AreSame(_ForrestGump, _People[0]);
            Assert.AreSame(_JonSchmidt, _People[1]);
            Assert.AreSame(_BobRoss, _People[2]);
            Assert.AreSame(_DoogieHowser, _People[3]);
            Assert.AreSame(_MichaelJackson, _People[4]);
        }

        /// <summary>
        /// Ensures that proxies properly order their items.
        /// </summary>
        [Test]
        public void ProxyOrder1()
        {
            Assert.AreSame(_ForrestGump, _Doctors.First());
            Assert.AreSame(_DoogieHowser, _Doctors.Skip(1).First());
        }

        /// <summary>
        /// Ensures that proxies properly order their items.
        /// </summary>
        [Test]
        public void ProxyOrder2()
        {
            var newDoctor = new Doctor() { Group = 5, Name = "New Doctor", ProviderNumber = "23456" };
            _Doctors.Add(newDoctor);
            Person[] list = { _ForrestGump, _DoogieHowser, newDoctor };

            Assert.IsTrue(_Doctors.SequenceEqual(list.OfType<Doctor>()));
        }

        /// <summary>
        /// Ensures that proxies properly order their items.
        /// </summary>
        [Test]
        public void ProxyOrder3()
        {
            var newDoctor = new Doctor() { Group = 5, Name = "New Doctor", ProviderNumber = "23456" };
            _People.Insert(0, newDoctor);
            Person[] list = { newDoctor, _ForrestGump, _DoogieHowser };

            Assert.IsTrue(_Doctors.SequenceEqual(list.OfType<Doctor>()));
        }

        /// <summary>
        /// Ensures that proxies properly order their items.
        /// </summary>
        [Test]
        public void ProxyOrder4()
        {
            var newDoctor = new Doctor() { Group = 5, Name = "New Doctor", ProviderNumber = "23456" };
            _People.Insert(3, newDoctor);
            Person[] list = { _ForrestGump, newDoctor, _DoogieHowser };

            Assert.IsTrue(_Doctors.SequenceEqual(list.OfType<Doctor>()));
        }

        /// <summary>
        /// Ensures the IndexOf() method works as expected.
        /// </summary>
        [Test]
        public void IndexOf1()
        {
            Assert.AreEqual(0, _People.IndexOf(_ForrestGump));
            Assert.AreEqual(1, _People.IndexOf(_JonSchmidt));
            Assert.AreEqual(2, _People.IndexOf(_BobRoss));
            Assert.AreEqual(3, _People.IndexOf(_DoogieHowser));
            Assert.AreEqual(4, _People.IndexOf(_MichaelJackson));
        }
        
        [Test]
        public void Insert1()
        {   
            Assert.AreEqual(5, _People.Count);
            Assert.AreEqual(2, _Doctors.Count);

            var newDoctor = new Doctor() { Group = 5, Name = "New Doctor", ProviderNumber = "23456" };
            _People.Insert(0, newDoctor);

            Assert.AreEqual(6, _People.Count);
            Assert.AreEqual(3, _Doctors.Count);
            Assert.AreEqual(newDoctor, _Doctors.First());

            var middleDoctor = new Doctor() { Group = 5, Name = "Middle Doctor", ProviderNumber = "23456" };
            _People.Insert(2, middleDoctor);

            Assert.AreEqual(7, _People.Count);
            Assert.AreEqual(4, _Doctors.Count);
            Assert.AreEqual(middleDoctor, _Doctors.Skip(2).First());            
        }

        /// <summary>
        /// Ensures items are properly removed 
        /// when calling Remove() from the original list.
        /// </summary>
        [Test]
        public void Remove1()
        {
            Assert.AreEqual(5, _People.Count);
            Assert.AreEqual(2, _Doctors.Count);
            _People.Remove(_DoogieHowser);
            Assert.AreEqual(4, _People.Count);
            Assert.AreEqual(1, _Doctors.Count);
        }

        /// <summary>
        /// Ensures items are properly removed 
        /// when calling Remove() from the proxied list.
        /// </summary>
        [Test]
        public void RemoveProxy1()
        {
            Assert.AreEqual(2, _Doctors.Count);
            Assert.AreEqual(5, _People.Count);
            _People.Remove(_DoogieHowser);
            Assert.AreEqual(1, _Doctors.Count);
            Assert.AreEqual(4, _People.Count);
        }

        /// <summary>
        /// Ensure items are presented in ascending order (by group)
        /// </summary>
        [Test]
        public void SortKeys1()
        {
            _People.SortKeys();
            
            long group = -1;
            foreach (var person in _People)
            {
                Assert.LessOrEqual(group, person.Group);
                group = person.Group;
            }
        }
    }
}
