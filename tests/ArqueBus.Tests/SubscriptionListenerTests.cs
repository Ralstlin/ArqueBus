using FluentAssertions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ArqueBus.Tests
{
    public class SubscriptionListenerTests
    {
        public class TestModel { }

        [Fact]
        public void Given_SubscriptionListener_When_Constructed_Then_ThrowIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SubscriptionListener<string, object>("", null));
        }

        [Fact]
        public async Task Given_SubscriptionListener_When_Run_Then_ThrowIfNull()
        {
            var bus = new EventBus<string, object>();

            var underTest = new SubscriptionListener<string, object>("", bus);
            await Assert.ThrowsAsync<ArgumentNullException>(() => underTest.Run(null, string.Empty));
        }

        [Fact]
        public async Task Given_SubscriptionListener_When_Listen_Then_ThrowIfNull()
        {
            var bus = new EventBus<string, object>();

            var underTest = new SubscriptionListener<string, object>("test", bus);
            await underTest.Run("test", string.Empty);

            await Assert.ThrowsAsync<InvalidCastException>(() => underTest.ListenAsync<TestModel>());
        }

        [Fact]
        public async Task Given_SubscriptionListener_When_ListenWithToken_Then_ThrowIfNull()
        {
            var bus = new EventBus<string, object>();
            var cancellationTokenSource = new CancellationTokenSource();

            var underTest = new SubscriptionListener<string, object>("test", bus);
            await underTest.Run("test", string.Empty);

            await Assert.ThrowsAsync<InvalidCastException>(() => underTest.ListenAsync<TestModel>(cancellationTokenSource.Token));
        }


        [Fact]
        public async Task Given_SubscriptionListener_When_ListenWithoutToken_Then_Retrieve()
        {
            var bus = new EventBus<string, object>();
            var underTest = new SubscriptionListener<string, object>("test", bus);
            await underTest.Run("test", string.Empty);

            var result = await underTest.ListenAsync<string>();

            result.Should().Be(string.Empty);
        }
    }
}
