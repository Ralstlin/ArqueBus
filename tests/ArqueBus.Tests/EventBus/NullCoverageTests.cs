using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace ArqueBus.Tests.EventBus
{
    public class NullCoverageTests
    {

        [Fact]
        public void Given_EventBus_When_SubscribeWithNull_Then_Throw()
        {
            // Arrange
            var bus = new EventBus<string, object>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => bus.Subscribe(string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => bus.Subscribe(null, (data) => { } ));

            Assert.Throws<ArgumentNullException>(() => bus.SubscribeOnce(string.Empty, null));
            Assert.Throws<ArgumentNullException>(() => bus.SubscribeOnce(null, (data) => { }));

            Assert.Throws<ArgumentNullException>(() => bus.Unsubscribe((SubscriptionIdentity<string>)null));
            Assert.Throws<ArgumentNullException>(() => bus.Unsubscribe((string)null));

            Assert.ThrowsAsync<ArgumentNullException>(() => bus.PublishAsync(null, "test"));
            Assert.ThrowsAsync<ArgumentNullException>(() => bus.PublishAsync(null, (object)"test"));

            Assert.Throws<ArgumentNullException>(() => bus.CreateListener(null));

            Assert.ThrowsAsync<ArgumentNullException>(() => bus.ListenAsync<string>(null, new DataflowBlockOptions() { CancellationToken = new CancellationToken() }));
            Assert.ThrowsAsync<ArgumentNullException>(() => bus.ListenAsync<string>(string.Empty, new DataflowBlockOptions()));
            Assert.ThrowsAsync<ArgumentNullException>(() => bus.ListenAsync<string>(string.Empty, null));

            Assert.ThrowsAsync<ArgumentNullException>(() => bus.ListenFixedTimesAsync<string>(null, 1, new DataflowBlockOptions() { CancellationToken = new CancellationToken() }));
            Assert.ThrowsAsync<ArgumentNullException>(() => bus.ListenFixedTimesAsync<string>(string.Empty, 1, new DataflowBlockOptions()));
            Assert.ThrowsAsync<ArgumentNullException>(() => bus.ListenFixedTimesAsync<string>(string.Empty, 1, null));
            Assert.ThrowsAsync<ArgumentException>(() => bus.ListenFixedTimesAsync<string>(string.Empty, 0, new DataflowBlockOptions() { CancellationToken = new CancellationToken() }));
        }
    }
}
