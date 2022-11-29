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
    public class RedisCacheServiceTestsKeyPrefix
    {
        private MockRepository _mockRepository;

        private Mock<IDatabase> _dataBase;

        private RedisCacheService _target;

        private Fixture _fixture;

        private string _keyPrefix;

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

            _fixture = new Fixture();

            _keyPrefix = _fixture.Create<string>();

            _target = new RedisCacheService(_dataBase.Object, _keyPrefix, _serializerSettings);
        }

        #region Add

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task AddAsync_Object_Success(bool expected)
        {
            var key = _fixture.Create<string>();
            var data = _fixture.Create<object>();
            var timeOut = _fixture.Create<TimeSpan>();

            var value = data.Serialize(_serializerSettings);

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.StringSetAsync(expectedKey, value, timeOut, 
                    It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(() => expected);


            var result = await _target.AddAsync(key, data, timeOut);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);

        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task TryAddAsync_Object_Success(bool expected)
        {
            var key = _fixture.Create<string>();
            var data = _fixture.Create<object>();
            var timeOut = _fixture.Create<TimeSpan>();

            var value = data.Serialize(_serializerSettings);

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.StringSetAsync(expectedKey, value, timeOut, 
                    It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .ReturnsAsync(() => expected);


            var result = await _target.TryAddAsync(key, data, timeOut);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Add_Object_Success(bool expected)
        {
            var key = _fixture.Create<string>();
            var data = _fixture.Create<object>();
            var timeOut = _fixture.Create<TimeSpan>();

            var value = data.Serialize(_serializerSettings);

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.StringSet(expectedKey, value, timeOut, 
                    It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns(() => expected);


            var result = _target.Add(key, data, timeOut);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TryAdd_Object_Success(bool expected)
        {
            var key = _fixture.Create<string>();
            var data = _fixture.Create<object>();
            var timeOut = _fixture.Create<TimeSpan>();

            var value = data.Serialize(_serializerSettings);

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.StringSet(expectedKey, value, timeOut, 
                    It.IsAny<bool>(), It.IsAny<When>(), It.IsAny<CommandFlags>()))
                .Returns(() => expected);


            var result = _target.TryAdd(key, data, timeOut);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        #endregion Add

        #region Get

        [Test]
        public async Task GetAsync_ObjectValue_Success()
        {
            var key = _fixture.Create<string>();

            var expected = _fixture.Create<TestType>();

            var value = expected.Serialize(_serializerSettings);

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.StringGetAsync(expectedKey, CommandFlags.None)).ReturnsAsync(value);


            var data = await _target.GetAsync<TestType>(key);


            _mockRepository.VerifyAll();

            Assert.NotNull(data);
            Assert.AreEqual(expected.Property, data.Property);
            Assert.AreEqual(expected.Field, data.Field);
        }

        [Test]
        public void Get_ObjectValue_Success()
        {
            var key = _fixture.Create<string>();

            var expected = _fixture.Create<TestType>();

            var value = expected.Serialize(_serializerSettings);

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.StringGet(expectedKey, CommandFlags.None)).Returns(value);


            var data = _target.Get<TestType>(key);


            _mockRepository.VerifyAll();

            Assert.NotNull(data);
            Assert.AreEqual(expected.Property, data.Property);
            Assert.AreEqual(expected.Field, data.Field);
        }

        #endregion

        #region TryGet

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TryGet_ObjectValue_Success(bool throwException)
        {
            var key = _fixture.Create<string>();

            var expected = _fixture.Create<TestType>();

            var value = expected.Serialize(_serializerSettings);

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.StringGet(expectedKey, CommandFlags.None)).Returns(value);


            var result = _target.TryGet(key, out TestType data, throwException);


            _mockRepository.VerifyAll();

            Assert.True(result);
            Assert.NotNull(data);
            Assert.AreEqual(expected.Property, data.Property);
            Assert.AreEqual(expected.Field, data.Field);
        }

        #endregion TryGet

        #region Exists

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task ExistsAsync_DifferentValues_Success(bool expected)
        {
            var key = _fixture.Create<string>();

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.KeyExistsAsync(expectedKey, CommandFlags.None)).ReturnsAsync(expected);


            var result = await _target.ExistsAsync(key);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Exists_DifferentValues_Success(bool expected)
        {
            var key = _fixture.Create<string>();

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.KeyExists(expectedKey, CommandFlags.None)).Returns(expected);


            var result = _target.Exists(key);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        #endregion Exists

        #region DeleteAsync

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task DeleteAsync_DifferentValues_Success(bool expected)
        {
            var key = _fixture.Create<string>();

            var expectedKey = $"{_keyPrefix}{key}";

            _dataBase.Setup(b => b.KeyDeleteAsync(expectedKey, CommandFlags.None)).ReturnsAsync(expected);


            var result = await _target.DeleteAsync(key);


            _mockRepository.VerifyAll();

            Assert.AreEqual(expected, result);
        }

        #endregion DeleteAsync
    }
}
