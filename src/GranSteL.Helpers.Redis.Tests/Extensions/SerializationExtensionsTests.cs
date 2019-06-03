using AutoFixture;
using GranSteL.Helpers.Redis.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GranSteL.Helpers.Redis.Tests.Extensions
{
    [TestFixture]
    public class SerializationExtensionsTests
    {
        private Fixture _fixture;

        [SetUp]
        public void InitTest()
        {
            _fixture = new Fixture();
        }

        [Test]
        public void Serialize_String_NotSerialize()
        {
            var expected = _fixture.Create<string>();


            var result = expected.Serialize();


            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Serialize_Object_Serialize()
        {
            var obj = _fixture.Create<object>();
            var expected = JsonConvert.SerializeObject(obj);


            var result = obj.Serialize();


            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Deserialize_Throws_Success()
        {
            var serialized = _fixture.Create<string>();


            Assert.Throws<JsonReaderException>(() => serialized.Deserialize<TestType>());
        }

        [Test]
        public void Deserialize_String_NotDeserialize()
        {
            var expected = _fixture.Create<string>();


            var result = expected.Deserialize<string>();


            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Deserialize_Object_Deserialize()
        {
            var expected = _fixture.Create<TestType>();


            var result = expected.Deserialize<TestType>();


            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Deserialize_AnotherType_Null()
        {
            var expected = _fixture.Build<TestType2>().OmitAutoProperties().Create();


            var result = expected.Deserialize<TestType>();


            Assert.IsNull(result);
        }
    }
}
