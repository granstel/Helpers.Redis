using System;
using System.Threading.Tasks;
using AutoFixture;
using GranSteL.Helpers.Redis.Extensions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StackExchange.Redis;

namespace GranSteL.Helpers.Redis.Tests
{
    [TestFixture]
    public class RedisCacheServiceTests
    {
        private MockRepository _mockRepository;

        private Mock<IDatabase> _dataBase;

        private RedisCacheService _target;

        private Fixture _fixture;

        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto
        };

        [SetUp]
        public void InitTest()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _dataBase = _mockRepository.Create<IDatabase>();

            _target = new RedisCacheService(_dataBase.Object);

            _fixture = new Fixture { OmitAutoProperties = true };
        }

        #region AddAsync

        [Test]
        public void AddAsync_NullKey_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _target.AddAsync(null, null));
        }

        [Test]
        public async Task AddAsync_NullData_False()
        {
            var key = _fixture.Create<string>();


            var result = await _target.AddAsync(key, null);


            Assert.False(result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task AddAsync_String_Success(bool expected)
        {
            var key = _fixture.Create<string>();
            var value = _fixture.Create<string>();
            var timeOut = _fixture.Create<TimeSpan>();

            _dataBase.Setup(b => b.StringSetAsync(key, value, timeOut, When.Always, CommandFlags.None))
                .ReturnsAsync(() => expected);


            var result = await _target.AddAsync(key, value, timeOut);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task AddAsync_Object_Success(bool expected)
        {
            var key = _fixture.Create<string>();
            var data = _fixture.Create<object>();
            var timeOut = _fixture.Create<TimeSpan>();

            var value = data.Serialize(_serializerSettings);

            _dataBase.Setup(b => b.StringSetAsync(key, value, timeOut, When.Always, CommandFlags.None))
                .ReturnsAsync(() => expected);


            var result = await _target.AddAsync(key, data, timeOut);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);

        }

        [Test]
        public void AddAsync_Throws_Success()
        {
            var key = _fixture.Create<string>();
            var data = _fixture.Create<object>();
            var timeOut = _fixture.Create<TimeSpan>();

            var value = data.Serialize(_serializerSettings);

            _dataBase.Setup(b => b.StringSetAsync(key, value, timeOut, When.Always, CommandFlags.None))
                .Throws<Exception>();


            Assert.ThrowsAsync<Exception>(() => _target.AddAsync(key, data, timeOut));


            _mockRepository.VerifyAll();
        }

        #endregion AddAsync

        #region TryGet

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TryGet_NullKey_Throws(bool throwException)
        {
            Assert.Throws<ArgumentNullException>(() => _target.TryGet(null, out object _, throwException));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TryGet_DefaultValue_False(bool throwException)
        {
            var key = _fixture.Create<string>();

            var value = default(RedisValue);

            _dataBase.Setup(b => b.StringGet(key, CommandFlags.None)).Returns(value);


            var result = _target.TryGet(key, out object data, throwException);


            _mockRepository.VerifyAll();

            Assert.False(result);
            Assert.IsNull(data);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TryGet_StringValue_Success(bool throwException)
        {
            var key = _fixture.Create<string>();

            var value = _fixture.Create<string>();

            _dataBase.Setup(b => b.StringGet(key, CommandFlags.None)).Returns(value);


            var result = _target.TryGet(key, out string data, throwException);


            _mockRepository.VerifyAll();

            Assert.True(result);
            Assert.AreEqual(value, data);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TryGet_ObjectValue_Success(bool throwException)
        {
            var key = _fixture.Create<string>();

            var expected = _fixture.Create<object>();

            var value = expected.Serialize(_serializerSettings);

            _dataBase.Setup(b => b.StringGet(key, CommandFlags.None)).Returns(value);


            var result = _target.TryGet(key, out object data, throwException);


            _mockRepository.VerifyAll();

            Assert.True(result);
            Assert.NotNull(data);
        }

        [Test]
        public void TryGet_ThrowException_Throws()
        {
            var key = _fixture.Create<string>();

            _dataBase.Setup(b => b.StringGet(key, CommandFlags.None)).Throws<Exception>();

            object data = null;


            Assert.Throws<Exception>(() => _target.TryGet(key, out data, true));


            _mockRepository.VerifyAll();

            Assert.IsNull(data);
        }

        [Test]
        public void TryGet_NotThrowException_False()
        {
            var key = _fixture.Create<string>();

            _dataBase.Setup(b => b.StringGet(key, CommandFlags.None)).Throws<Exception>();


            var result = _target.TryGet(key, out object data, false);


            _mockRepository.VerifyAll();

            Assert.False(result);
            Assert.IsNull(data);
        }

        #endregion TryGet

        #region Exists

        [Test]
        public void Exists_NullKey_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _target.Exists(null));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Exists_DifferentValues_Success(bool expected)
        {
            var key = _fixture.Create<string>();

            _dataBase.Setup(b => b.KeyExists(key, CommandFlags.None)).Returns(expected);


            var result = _target.Exists(key);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Exists_Throws_Success()
        {
            var key = _fixture.Create<string>();

            _dataBase.Setup(b => b.KeyExists(key, CommandFlags.None)).Throws<Exception>();


            Assert.Throws<Exception>(() => _target.Exists(key));


            _mockRepository.VerifyAll();
        }

        #endregion Exists

        #region DeleteAsync

        [Test]
        public void DeleteAsync_NullKey_Throws()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _target.DeleteAsync(null));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task DeleteAsync_DifferentValues_Success(bool expected)
        {
            var key = _fixture.Create<string>();

            _dataBase.Setup(b => b.KeyDeleteAsync(key, CommandFlags.None)).ReturnsAsync(expected);


            var result = await _target.DeleteAsync(key);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void DeleteAsync_Throws_Success()
        {
            var key = _fixture.Create<string>();

            _dataBase.Setup(b => b.KeyDeleteAsync(key, CommandFlags.None)).Throws<Exception>();


            Assert.ThrowsAsync<Exception>(() => _target.DeleteAsync(key));


            _mockRepository.VerifyAll();
        }

        #endregion DeleteAsync
    }
}
