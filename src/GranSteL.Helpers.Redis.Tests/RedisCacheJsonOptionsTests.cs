using System.Text.Json;
using Moq;
using NUnit.Framework;
using StackExchange.Redis;

namespace GranSteL.Helpers.Redis.Tests
{
    [TestFixture]
    public class RedisCacheJsonOptionsTests
    {
        [Test]
        public void LegacyMetadataCannotSelectADerivedClrType()
        {
            var db = new Mock<IDatabase>(MockBehavior.Strict);
            var typeName = typeof(DerivedItem).AssemblyQualifiedName;
            var json = $"{{\"$type\":{JsonSerializer.Serialize(typeName)},\"Name\":\"sample\",\"Rank\":1}}";
            db.Setup(x => x.StringGet("cache:item", CommandFlags.None)).Returns(json);

            var cache = new RedisCacheService(db.Object, "cache:");
            Assert.That(cache.TryGet("item", out Item item), Is.True);
            Assert.That(item, Is.TypeOf<Item>());
            Assert.That(item.Name, Is.EqualTo("sample"));
        }

        [Test]
        public void UsesSuppliedOptionsForReadingAndWriting()
        {
            var db = new Mock<IDatabase>(MockBehavior.Strict);
            const string json = "{\"name\":\"sample\"}";
            db.Setup(x => x.StringSet("item", json, null, false, When.Always, CommandFlags.None)).Returns(true);
            db.Setup(x => x.StringGet("item", CommandFlags.None)).Returns(json);
            var cache = new RedisCacheService(db.Object, defaultSerializerOptions: new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            Assert.That(cache.Add("item", new Item { Name = "sample" }), Is.True);
            Assert.That(cache.Get<Item>("item").Name, Is.EqualTo("sample"));
        }

        public class Item
        {
            public string Name { get; set; }
        }

        public class DerivedItem : Item
        {
            public int Rank { get; set; }
        }
    }
}
