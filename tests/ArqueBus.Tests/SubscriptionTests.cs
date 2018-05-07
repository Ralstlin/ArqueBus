using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ArqueBus.Tests
{
    public class SubscriptionTests
    {
        [Fact]
        public void Given_Subscription_When_Constructed_Then_ValidateIdentity()
        {
            var identity = new SubscriptionIdentity<string>(string.Empty);
            var underTest = new Subscription<string, object>((data) => { }, identity);

            underTest.SubscriptionIdentity.Identity.Should().Be(identity.Identity);
        }

        [Fact]
        public async Task Given_Subscription_When_Run_Then_ThrowIfNull()
        {
            var identity = new SubscriptionIdentity<string>(string.Empty);
            var underTest = new Subscription<string, object>((data) => { }, identity);

            await Assert.ThrowsAsync<ArgumentNullException>(() => underTest.Run(null, null));
        }
    }
}
