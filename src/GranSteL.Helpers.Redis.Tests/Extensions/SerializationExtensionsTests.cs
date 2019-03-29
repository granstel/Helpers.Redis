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
            _fixture = new Fixture { OmitAutoProperties = true };
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
        public void Deserialize_Throws_Default()
        {
            var serialized = _fixture.Create<string>();


            var result = serialized.Deserialize<object>();


            Assert.AreEqual(null, result);
        }

        [Test]
        public void Deserialize_String_NotDeserialize()
        {
            var expected = _fixture.Create<string>();


            var result = expected.Deserialize<string>();


            Assert.AreEqual(expected, result);
        }
    }
}
