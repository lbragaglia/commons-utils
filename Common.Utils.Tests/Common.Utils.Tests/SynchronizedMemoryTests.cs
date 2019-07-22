using System;
using System.Threading.Tasks;
using Commons.Utils.Cache;
using NUnit.Framework;

namespace Common.Utils.Tests
{
    [TestFixture]
    public class SynchronizedMemoryTests
    {
        [Test]
        public async Task CacheDifferentValuesInTwoMessageScopes()
        {
            var scope = new MessageScope();
            var cache = new SynchronizedMemoryCache<object>(scope);

            scope.StartNew("A");
            var value1A =
                await Task.Factory.StartNew(() => cache.GetOrCreate("key1", () => Task.FromResult((object)"valueA")));
            var value1B = await Task.Factory.StartNew(() =>
            {
                scope.StartNew("B");
                return cache.GetOrCreate("key1", () => Task.FromResult((object)"valueB"));
            });

            Assert.That(value1A.Result, Is.EqualTo("valueA"));
            Assert.That(value1B.Result, Is.EqualTo("valueB"));

            var value1A2 = await Task.Factory.StartNew(() =>
                cache.GetOrCreate("key1", FailingCreationTask));
            var value1B2 = await Task.Factory.StartNew(() =>
            {
                scope.StartNew("B");
                return cache.GetOrCreate("key1", FailingCreationTask);
            });

            Assert.That(value1A2.Result, Is.EqualTo("valueA"));
            Assert.That(value1B2.Result, Is.EqualTo("valueB"));
        }

        [Test]
        public async Task WaitingToGetACachedValue()
        {
            var cache = new SynchronizedMemoryCache<object>(10000, 2000);

            await Task.Factory.StartNew(async () => await cache.GetOrCreate("key2", async () =>
            {
                await Task.Delay(1000);
                return "value2";
            }));
            var value2 = await cache.GetOrCreate("key2", FailingCreationTask);

            Assert.That(value2, Is.EqualTo("value2"));
        }

        [Test]
        public void ExceedingMaxWaitTimeToGetACachedValue()
        {
            var cache = new SynchronizedMemoryCache<object>(2000, 200);

            Task.Factory.StartNew(async () => await cache.GetOrCreate("key3", async () =>
            {
                await Task.Delay(1000);
                return "value3";
            }));

            Assert.Throws<AggregateException>(() => cache.GetOrCreate("key3", FailingCreationTask).Wait());
        }

        [Test]
        public async Task CachedItemExpiration()
        {
            var cache = new SynchronizedMemoryCache<object>(100);

            await cache.GetOrCreate("key4", () => Task.FromResult((object)"value4"));
            var value1A = await cache.GetOrCreate("key4", () => Task.FromResult((object)"value5"));

            Assert.That(value1A, Is.EqualTo("value4"));

            await Task.Delay(200);
            value1A = await cache.GetOrCreate("key4", () => Task.FromResult((object)"value6"));

            Assert.That(value1A, Is.EqualTo("value6"));
        }

        private static Task<object> FailingCreationTask() =>
            Task.FromException<object>(new Exception("[TEST] cache item creation failure"));
    }
}
