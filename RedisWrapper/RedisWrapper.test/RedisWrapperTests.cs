using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisWrapper.Tests
{
    [TestFixture,Category("RedisCacheWrapper")]
    public class RedisCacheWrapperTests
    {
        
        [Test]
        public void SetEntryTest()
        {
            TestObject obj = GetObject();

            RedisCacheWrapper.SetEntry<TestObject>(obj.TestObjectId, obj);
        }

        [Test]
        public void GetTypedValueTest()
        {
            TestObject cachedObj = RedisCacheWrapper.GetTypedValue<TestObject>(GetKey());

            TestObject cachedObj1 = RedisCacheWrapper.GetTypedValue<TestObject>(GetKey());

            Assert.AreNotEqual(cachedObj, null);
        }

        [Test]
        public void SetEntryExpiredTest()
        {
            TestObject obj = GetObject();

            RedisCacheWrapper.SetEntry<TestObject>(obj.TestObjectId, obj,5);

            Thread.Sleep(5999);

            TestObject cachedObj = RedisCacheWrapper.GetTypedValue<TestObject>(GetKey());

            Assert.AreEqual(null, cachedObj);
        }

        [Test]
        public void DeleteEntry()
        {
            TestObject obj = GetObject();

            RedisCacheWrapper.SetEntry<TestObject>(obj.TestObjectId, obj);

            string key = GetKey();

            bool result = RedisCacheWrapper.Delete<TestObject>(key);

            Assert.AreEqual(true, result);
        }

        private string GetKey()
        {
            return "key1";
        }

        private TestObject GetObject()
        {
            TestObject obj = new TestObject
            {
                TestObjectId = GetKey(),
                Name = "TestName",
                Gender = "Male",
                Child = new TestChildObject
                {
                    TestObjectId = "TestId",
                    Name = "ChildName",
                    Gender = "Female"
                }
            };

            return obj;
        }

        internal class TestObject
        {
            public string TestObjectId { get; set; }

            public string Name { get; set; }

            public string Gender { get; set; }

            public TestChildObject Child { get; set; }
        }

        internal class TestChildObject
        {
            public string TestObjectId { get; set; }

            public string Name { get; set; }

            public string Gender { get; set; }
        }
    }
}
