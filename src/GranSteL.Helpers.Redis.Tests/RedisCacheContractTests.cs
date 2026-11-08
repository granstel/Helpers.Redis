using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using StackExchange.Redis;

namespace GranSteL.Helpers.Redis.Tests
{
    [TestFixture]
    public class RedisCacheContractTests
    {
        private ConnectionMultiplexer _connection;
        private IDatabase _database;
        private IRedisCacheService _cache;
        private string _prefix;

        [OneTimeSetUp]
        public async Task Connect()
        {
            var port = Environment.GetEnvironmentVariable("HELPERS_REDIS_TEST_PORT");
            if (string.IsNullOrEmpty(port))
                Assert.Ignore("Set HELPERS_REDIS_TEST_PORT to the port of a disposable local Redis instance.");

            _connection = await ConnectionMultiplexer.ConnectAsync($"127.0.0.1:{int.Parse(port)}");
            _database = _connection.GetDatabase();
        }

        [OneTimeTearDown]
        public void Disconnect() => _connection?.Dispose();

        [SetUp]
        public void SetUp()
        {
            _prefix = $"helpers-redis-test:{Guid.NewGuid():N}:";
            _cache = new RedisCacheService(_database, _prefix);
        }

        [Test]
        public async Task WritesRawStringsIncludingEmptyMarkers()
        {
            var raw = "\"{\\\"Greeting\\\":\\\"Привет\\\"}\"";
            Assert.That(_cache.Add("raw", raw, TimeSpan.FromMinutes(1)), Is.True);
            Assert.That((string)await _database.StringGetAsync(_prefix + "raw"), Is.EqualTo(raw));
            Assert.That(_cache.Get<string>("raw"), Is.EqualTo(raw));
            Assert.That(await _cache.GetAsync<string>("raw"), Is.EqualTo(raw));
            Assert.That(_cache.TryGet<object>("raw", out var untyped), Is.True);
            Assert.That(untyped, Is.EqualTo(raw));

            Assert.That(await _cache.AddAsync("marker", "", TimeSpan.FromMinutes(1)), Is.True);
            Assert.That((string)await _database.StringGetAsync(_prefix + "marker"), Is.Empty);
            Assert.That(_cache.Exists("marker"), Is.True);
            Assert.That(await _cache.ExistsAsync("marker"), Is.True);
            Assert.That(_cache.Get<string>("marker"), Is.Null);
            Assert.That(await _cache.GetAsync<string>("marker"), Is.Null);
            Assert.That(_cache.TryGet<string>("marker", out var marker, throwException: true), Is.False);
            Assert.That(marker, Is.Null);
        }

        [Test]
        public async Task WritesPascalCasePropertiesAndPublicFieldsWithoutNullMembers()
        {
            var data = new CachedItem { Name = "sample", Revision = 7 };
            Assert.That(await _cache.AddAsync("data", data, TimeSpan.FromMinutes(1)), Is.True);
            using var json = JsonDocument.Parse((string)await _database.StringGetAsync(_prefix + "data"));
            Assert.That(json.RootElement.EnumerateObject().Select(property => property.Name),
                Is.EquivalentTo(new[] { "Name", "Revision" }));
            Assert.That(json.RootElement.GetProperty("Name").GetString(), Is.EqualTo("sample"));
            Assert.That(json.RootElement.GetProperty("Revision").GetInt32(), Is.EqualTo(7));
        }

        [TestCase("{\"name\":\"sample\",\"Revision\":\"7\",\"Status\":\"Ready\"}")]
        [TestCase("{\"$type\":\"GranSteL.Helpers.Redis.Tests.RedisCacheContractTests+CachedItem, GranSteL.Helpers.Redis.Tests\",\"Name\":\"sample\",\"Revision\":7,\"Status\":1}")]
        public async Task ReadsLegacyJsonAsTheRequestedConcreteType(string legacyJson)
        {
            await SeedAsync("legacy", legacyJson);

            AssertItem(_cache.Get<CachedItem>("legacy"));
            AssertItem(await _cache.GetAsync<CachedItem>("legacy"));
            Assert.That(_cache.TryGet<CachedItem>("legacy", out var item), Is.True);
            AssertItem(item);
        }

        [Test]
        public async Task TryAddOverwritesAndUsesTheProvidedTtl()
        {
            var ttl = TimeSpan.FromMinutes(1);
            Assert.That(_cache.TryAdd("item", "first", ttl), Is.True);
            Assert.That(await _cache.TryAddAsync("item", "second", ttl), Is.True);
            Assert.That(_cache.Get<string>("item"), Is.EqualTo("second"));
            Assert.That(await _database.KeyTimeToLiveAsync(_prefix + "item"),
                Is.InRange(TimeSpan.FromSeconds(30), ttl));
            Assert.That(await _cache.DeleteAsync("item"), Is.True);
            Assert.That(await _cache.DeleteAsync("item"), Is.False);
            Assert.That(await _cache.ExistsAsync("item"), Is.False);
            Assert.That(_cache.TryGet<int>("item", out var value), Is.False);
            Assert.That(value, Is.Zero);
        }

        [Test]
        public async Task TryOperationsHandleInvalidValuesButStillRejectEmptyKeys()
        {
            Assert.That(_cache.TryAdd("missing", null), Is.False);
            Assert.That(await _cache.TryAddAsync("missing", null), Is.False);
            Assert.That(await _cache.ExistsAsync("missing"), Is.False);
            Assert.Throws<NullKeyException>(() => _cache.TryAdd("", "x"));
            await Assert.ThrowsAsync<NullKeyException>(() => _cache.TryAddAsync("", "x"));
            Assert.Throws<NullKeyException>(() => _cache.TryGet<int>("", out _));

            await SeedAsync("broken", "{");
            Assert.That(_cache.TryGet<CachedItem>("broken", out var value), Is.False);
            Assert.That(value, Is.Null);
            Assert.Catch(() => _cache.TryGet<CachedItem>("broken", out _, throwException: true));
        }

        private Task<bool> SeedAsync(string key, string json) =>
            _database.StringSetAsync(_prefix + key, json, TimeSpan.FromMinutes(1));

        private static void AssertItem(CachedItem item)
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Name, Is.EqualTo("sample"));
            Assert.That(item.Revision, Is.EqualTo(7));
            Assert.That(item.Status, Is.EqualTo(ItemStatus.Ready));
        }

        public class CachedItem
        {
            public string Name { get; set; }
            public string Optional { get; set; }
            public ItemStatus? Status { get; set; }
            public int Revision;
        }

        public enum ItemStatus
        {
            Ready = 1
        }
    }
}
