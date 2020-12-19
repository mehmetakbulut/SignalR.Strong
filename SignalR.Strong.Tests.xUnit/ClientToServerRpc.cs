using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Collections;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong.Tests.Common;
using Xunit;

namespace SignalR.Strong.Tests.xUnit
{
    public class BasicGetAndSet : IClassFixture<ServerFixture>
    {
        private ServerFixture fixture;

        public BasicGetAndSet(ServerFixture fixture)
        {
            this.fixture = fixture;
        }
        
        [Fact]
        public async Task Get()
        {
            var client = await fixture.GetClient();
            var hub = client.GetHub<IMockHub>();
            
            await hub.GetVoid();

            Assert.Equal(123, await hub.GetValueType());

            Assert.Equal("abc", await hub.GetReferenceType());

            var col = await hub.GetCollection();
            col.Should().BeEquivalentTo(new List<int>() {1, 2, 3});
        }

        [Fact]
        public async Task Set()
        {
            var client = await fixture.GetClient();
            var hub = client.GetHub<IMockHub>();
            
            Assert.Equal(123, await hub.SetValueType(123));

            Assert.Equal("abc", await hub.SetReferenceType("abc"));

            var col = await hub.SetCollection(new List<int>() {1, 2, 3});
            col.Should().BeEquivalentTo(new List<int>() {1, 2, 3});
        }
    }
}