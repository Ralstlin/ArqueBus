using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Xunit;

namespace ArqueBus.Tests.EventBus
{
    public class EnumObjectTests
    {
        private enum ActionTest
        {
            Test1,
            Test2
        }

        private class TestModel 
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
        }

        [Fact]
        public void Given_EnumObjectEventBus_When_SinglePublish_Then_RunSubscription()
        {
            // Arrange
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };

            var underTest = new EventBus<ActionTest, object>();
            var counter = 0;
            underTest.Subscribe(ActionTest.Test1, (data) => { counter++; });

            // Act
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            

            counter.Should().Be(1);
        }

        [Fact]
        public void Given_EnumObjectEventBus_When_UnsubscribeIdentity_Then_NotThrow()
        {
            // Arrange
            var underTest = new EventBus<ActionTest, object>();
            var identity = underTest.Subscribe(ActionTest.Test1, (data) => { });
            
            // Pre assertion
            Assert.True(underTest.IsSubscribed(identity));

            // Act
            underTest.Unsubscribe(identity);

            // Assert
            Assert.False(underTest.IsSubscribed(identity));
        }

        [Fact]
        public void Given_EnumObjectEventBus_When_UnsubscribeAction_Then_NotThrow()
        {
            // Arrange
            var underTest = new EventBus<ActionTest, object>();
            underTest.Subscribe(ActionTest.Test1, (data) => { });
            underTest.Subscribe(ActionTest.Test1, (data) => { });

            // Pre assertion
            Assert.True(underTest.HasSubscriptions(ActionTest.Test1));

            // Act
            underTest.Unsubscribe(ActionTest.Test1);

            // Assert
            Assert.False(underTest.HasSubscriptions(ActionTest.Test1));

        }

        [Fact]
        public void Given_EnumObjectEventBus_When_MultiPublish_Then_RunSubscription()
        {
            // Arrange
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };

            var underTest = new EventBus<ActionTest, object>();
            var counter = 0;
            underTest.Subscribe(ActionTest.Test1, (data) => { counter++; });

            // Act
            underTest.Publish(ActionTest.Test1, null);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            
            counter.Should().Be(5);
        }

        [Fact]
        public void Given_EnumObjectEventBus_When_PublishWithoutSubscription_Then_DontThrow()
        {
            // Arrange
            var underTest = new EventBus<ActionTest, object>();

            // Act
            underTest.Publish(ActionTest.Test1, null);
        }

        [Fact]
        public void Given_EnumObjectEventBus_With_ValidSubscribeOnce_When_Publish_Then_GetDataOnlyOnce()
        {
            // Arrange
            var dataToSendFirst = new TestModel {IntValue = 33, StringValue = "33"};
            var counter = 0;
            var underTest = new EventBus<ActionTest, object>();

            underTest.SubscribeOnce(ActionTest.Test1, (data) => { counter++; });

            // Act
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);

            // Assert
            counter.Should().Be(1);
        }

        [Fact]
        public async Task Given_EnumObjectEventBus_With_ListenAsyncFixedTimes_When_Publish_Then_GetItemsPublished()
        {
            // Arrange            
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };
            var dataToSendLater = new TestModel { IntValue = 34, StringValue = "34" };
            var underTest = new EventBus<ActionTest, object>();
            var cancellationTokenSource = new CancellationTokenSource();
            var options = new DataflowBlockOptions { CancellationToken = cancellationTokenSource.Token };

            // Act
            var listenedTask = underTest.ListenFixedTimesAsync<TestModel>(ActionTest.Test1, 1, options);

            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendLater);
            
            var result = await listenedTask;

            // Assert
            result.Count.Should().Be(1);
            result.Single().IntValue.Should().Be(33);
            result.Single().StringValue.Should().Be("33");
        }

        [Fact]
        public async Task Given_EnumObjectEventBus_With_ListenAsync_When_Publish_Then_GetItemsPublished()
        {
            // Arrange            
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };
            var dataToSendLater = new TestModel { IntValue = 34, StringValue = "34" };
            var underTest = new EventBus<ActionTest, object>();
            var cancellationTokenSource = new CancellationTokenSource();
            var options = new DataflowBlockOptions { CancellationToken = cancellationTokenSource.Token };

            // Act
            var listenedTask = underTest.ListenAsync<TestModel>(ActionTest.Test1, options);

            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendLater);

            cancellationTokenSource.CancelAfter(1000);
            var result = await listenedTask;

            // Assert
            result.Count.Should().Be(2);
            result.Skip(0).First().IntValue.Should().Be(33);
            result.Skip(0).First().StringValue.Should().Be("33");
            result.Skip(1).First().IntValue.Should().Be(34);
            result.Skip(1).First().StringValue.Should().Be("34");
        }
    }
}
