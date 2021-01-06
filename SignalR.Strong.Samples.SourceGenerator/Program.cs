using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong;
using SignalR.Strong.Samples.Common.Hubs;

namespace SignalR.Strong.Samples.SourceGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var conn = new HubConnectionBuilder().WithUrl("http://localhost/").Build();
            var mock = conn.AsGeneratedHub<IMockHub>();
            await mock.GetVoid();
        }
    }
}