# Helpers.Redis
[![NuGet](https://buildstats.info/nuget/GranSteL.Helpers.Redis)](https://www.nuget.org/packages/GranSteL.Helpers.Redis)

`RedisCacheService` wraps StackExchange.Redis and caches raw strings and System.Text.Json values. The library targets .NET Standard 2.0.

Install
-------
It's available via [nuget package](https://www.nuget.org/packages/GranSteL.Helpers.Redis/)  
PM> `Install-Package GranSteL.Helpers.Redis`

### Migrating from 1.x

- Rebuild consumers against 2.0. Calls like `new RedisCacheService(database, "cache:")` still work. Replace custom `JsonSerializerSettings` with `JsonSerializerOptions`; supplied options replace the defaults for both reading and writing.
- Strings are still stored verbatim. An empty string can mark an existing key, but `Get<T>` and `GetAsync<T>` return `default` and `TryGet<T>` returns `false` for that value. Key prefixes, TTL, and overwrite behavior are unchanged.
- The default writer keeps CLR member names (typically PascalCase), includes public fields, and omits null members. The default reader accepts case differences, quoted numbers, and enum names. JSON property order and escaping may change.
- Read old JSON using the concrete type, including concrete types for nested values. Old `$type` metadata cannot select CLR types; new values carry no CLR type names by default. If you relied on automatic polymorphism or old `$values` collection wrappers, use an explicit System.Text.Json converter for known types or invalidate those entries.
- Review Newtonsoft.Json attributes, converters, and permissive conversions used by your models. In particular, explicit JSON nulls can now replace initialized members, values inside an `object` member may deserialize as `JsonElement`, and JSON errors now come from System.Text.Json. The System.Text.Json 10 dependency can report support warnings on older .NET runtimes.

### Tests

`dotnet test src/GranSteL.Helpers.sln` runs the unit tests with the .NET 10 SDK. Set `HELPERS_REDIS_TEST_PORT` to the port of a disposable Redis instance on loopback to also run the integration tests.
