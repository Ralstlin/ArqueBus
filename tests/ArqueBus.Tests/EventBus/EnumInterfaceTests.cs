using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using FluentAssertions;
using Xunit;

namespace ArqueBus.Tests.EventBus
{
    public class EnumInterfaceTests
    {
        private enum ActionTest
        {
            Test1,
            Test2
        }
        private interface IBusModel
        {
            int IntValue { get; set; }
        }
        private class TestModel : IBusModel
        {
            public int IntValue { get; set; }
            public string StringValue { get; set; }
        }

        [Fact]
        public void Given_EnumInterfaceEventBus_When_SinglePublish_Then_RunSubscription()
        {
            // Arrange
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };

            var underTest = new EventBus<ActionTest, IBusModel>();
            var counter = 0;
            underTest.Subscribe(ActionTest.Test1, (data) => { counter++; });

            // Act
            underTest.Publish(ActionTest.Test1, dataToSendFirst);

            counter.Should().Be(1);
        }

        [Fact]
        public void Given_EnumInterfaceEventBus_When_PublishSomethingNotUsingInterface_Then_Throw()
        {
            // Arrange
            var underTest = new EventBus<ActionTest, IBusModel>();

            // Assert & Act
            Assert.ThrowsAsync<InvalidCastException>(() => underTest.PublishAsync<string>(ActionTest.Test1, string.Empty));
        }

        [Fact]
        public void Given_EnumInterfaceEventBus_When_MultiplePublish_Then_RunSubscription()
        {
            // Arrange
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };

            var underTest = new EventBus<ActionTest, IBusModel>();
            var counter = 0;
            underTest.Subscribe(ActionTest.Test1, (data) => { counter++; });

            // Act
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);

            counter.Should().Be(5);
        }

        [Fact]
        public void Given_EnumInterfaceEventBus_With_ValidSubscribeOnce_When_Publish_Then_GetDataOnlyOnce()
        {
            // Arrange
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };
            var counter = 0;
            var underTest = new EventBus<ActionTest, IBusModel>();

            underTest.SubscribeOnce(ActionTest.Test1, (data) =>
            {
                counter++;
            });

            // Act
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);
            underTest.Publish(ActionTest.Test1, dataToSendFirst);

            // Assert
            counter.Should().Be(1);
        }


        [Fact]
        public async Task Given_EnumInterfaceEventBus_With_ListenFixedTimesAsync_When_PublishEnough_Then_GetItemsPublished()
        {
            // Arrange            
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };
            var dataToSendLater = new TestModel { IntValue = 34, StringValue = "34" };
            var underTest = new EventBus<ActionTest, IBusModel>();
            var cancellationTokenSource = new CancellationTokenSource();
            var options = new DataflowBlockOptions {CancellationToken = cancellationTokenSource.Token};

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
        public async Task Given_EnumInterfaceEventBus_With_ListenFixedTimesAsync_When_PublishAndCancel_Then_GetItemsPublished()
        {
            // Arrange            
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };
            var underTest = new EventBus<ActionTest, IBusModel>();
            var cancellationTokenSource = new CancellationTokenSource();
            var options = new DataflowBlockOptions { CancellationToken = cancellationTokenSource.Token };

            // Act
            var listenedTask = underTest.ListenFixedTimesAsync<TestModel>(ActionTest.Test1, 2, options);

            underTest.Publish(ActionTest.Test1, dataToSendFirst);

            cancellationTokenSource.CancelAfter(500);

            var result = await listenedTask;

            // Assert
            result.Count.Should().Be(1);
            result.Single().IntValue.Should().Be(33);
            result.Single().StringValue.Should().Be("33");
        }

        [Fact]
        public async Task Given_EnumInterfaceEventBus_With_ListenAsync_When_Publish_Then_GetItemsPublished()
        {
            // Arrange            
            var dataToSendFirst = new TestModel { IntValue = 33, StringValue = "33" };
            var dataToSendLater = new TestModel { IntValue = 34, StringValue = "34" };
            var underTest = new EventBus<ActionTest, IBusModel>();
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

        [Fact]
        public async Task Given_EnumInterfaceEventBus_With_ListenFixedTimesAsync_When_NoPublish_Then_GetEmptyList()
        {
            // Arrange            
            var underTest = new EventBus<ActionTest, IBusModel>();
            var cancellationTokenSource = new CancellationTokenSource();
            var options = new DataflowBlockOptions { CancellationToken = cancellationTokenSource.Token };

            // Act
            var listenedTask = underTest.ListenFixedTimesAsync<TestModel>(ActionTest.Test1, 1, options);

            cancellationTokenSource.CancelAfter(1000);
            var result = await listenedTask;

            // Assert
            result.Count.Should().Be(0);
        }
    }
}
