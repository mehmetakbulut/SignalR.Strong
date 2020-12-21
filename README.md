[![Build status](https://ci.appveyor.com/api/projects/status/w2grsvnercr66p95/branch/master?svg=true)](https://ci.appveyor.com/project/mehmetakbulut/signalr-strong/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/mehmetakbulut/SignalR.Strong/badge.svg?branch=master)](https://coveralls.io/github/mehmetakbulut/SignalR.Strong?branch=master)
[![Nuget](https://img.shields.io/nuget/v/SignalR.Strong)](https://www.nuget.org/packages/SignalR.Strong/)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/SignalR.Strong)](https://www.nuget.org/packages/SignalR.Strong/)

_**Work in progress, API may change**_

# SignalR.Strong

[SignalR Core](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-3.1) hubs can define strongly-typed hub methods and also perform strongly-typed server-to-client RPC however clients can neither define strongly-typed client methods nor perform strongly-typed client-to-server RPC.
SignalR.Strong is a .NET Standard 2.0 library which addresses this gap by introducing a higher level client so end-to-end static type-checking and refactoring is made possible.

###### Without SignalR.Strong

```c#
var resp = await conn.InvokeAsync<int>("DoSomethingOnServer", arg1, arg2, arg3);
```

###### With SignalR.Strong

```c#
var resp = await hub.DoSomethingOnServer(arg1, arg2, arg3);
```

### Features

- Strongly-typed calls from client to server
- Strongly-typed handlers for server to client calls
- Support for client-to-server and server-to-client streams using `ChannelReader<T>`
- No magic strings
- Small overhead per call and no additional overhead during streaming

#### Compatibility

- Built for .NET Standard 2.0
- Tested on .NET Core 3.1 and .NET 5
- Can be consumed from Unity3D by using [MSB4U](https://github.com/microsoft/MSBuildForUnity) or manually pulling in .NET Standard 2.0 DLLs. 

#### Performance

Benchmark suite is available at `SignalR.Strong.Tests.Benchmark`.

```
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.19041
AMD Ryzen 7 1700, 1 CPU, 16 logical and 8 physical cores
.NET Core SDK=5.0.100
  [Host]  : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT
  LongRun : .NET Core 3.1.9 (CoreCLR 4.700.20.47201, CoreFX 4.700.20.47203), X64 RyuJIT

Job=LongRun  IterationCount=100  LaunchCount=3  WarmupCount=15  

|   Type |                            Method |       Mean |     Error |    StdDev |     Median |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|------- |---------------------------------- |-----------:|----------:|----------:|-----------:|-------:|-------:|------:|----------:|
|    Rpc |                 GetVoid_SendAsync |   3.905 us | 0.0265 us | 0.1347 us |   3.894 us | 0.0153 |      - |     - |   1.28 KB |
|    Rpc |               GetVoid_InvokeAsync |  99.943 us | 0.2659 us | 1.3526 us |  99.992 us |      - |      - |     - |   3.52 KB |
|    Rpc |                    GetVoid_Strong | 103.363 us | 0.8879 us | 4.4757 us | 102.024 us |      - |      - |     - |   3.74 KB |
|    Rpc |          GetValueType_InvokeAsync | 107.348 us | 1.1837 us | 5.9989 us | 106.958 us |      - |      - |     - |   3.88 KB |
|    Rpc |               GetValueType_Strong | 107.014 us | 0.8530 us | 4.2602 us | 106.253 us |      - |      - |     - |   4.16 KB |
|    Rpc |          SetValueType_InvokeAsync | 106.813 us | 0.6868 us | 3.4495 us | 105.802 us |      - |      - |     - |   4.15 KB |
|    Rpc |               SetValueType_Strong | 112.837 us | 1.3977 us | 7.1845 us | 111.189 us |      - |      - |     - |   4.41 KB |
| Stream | GetRxChannel_StreamAsChannelAsync |  35.061 us | 0.4748 us | 2.4536 us |  34.585 us | 0.1831 |      - |     - |  15.66 KB |
| Stream |               GetRxChannel_Strong |  42.759 us | 0.3332 us | 1.7245 us |  42.419 us | 0.2441 |      - |     - |  16.81 KB |
| Stream |    SetReader_StreamAsChannelAsync |  23.800 us | 0.2336 us | 1.1861 us |  23.542 us | 0.1221 | 0.0610 |     - |   7.45 KB |
| Stream |                  SetReader_Strong |  25.580 us | 0.1117 us | 0.5599 us |  25.559 us | 0.0610 |      - |     - |   7.73 KB |
```

### Usage

#### Setup

1. Create a client: `var client = new SignalR.Strong.StrongClient()`
2. Register hubs: `client.RegisterHub<IMyHub>(hubConnection)`
3. Register spokes which are handlers for server-to-client calls:
```c#
client.RegisterSpoke<MySpoke, IMyHub>();             // Simplest form, type will be MySpoke
client.RegisterSpoke<IMySpoke, MySpoke, IMyHub>();   // Constrain handler interface, type will be IMySpoke
client.RegisterSpoke<IMySpoke, IMyHub>(new MyHub()); // Pass instance manually, type will be IMySpoke
```
4. Connect hubs that haven't been yet: `await client.ConnectToHubsAsync()`
5. Build spokes: `client.BuildSpokes()` (only needed if you have registered spokes)

#### Interaction

- `THub StrongClient.GetHub<THub>()` returns a hub proxy that you can use for performing strongly-typed calls as well as streaming.

- `HubConnection StrongClient.GetHubConnection<THub>()` returns underlying hub connection for performing low-level operations such as registering for events. (e.g. `Reconnecting`)

- `TSpoke StrongClient.GetSpoke<TSpoke>()` can be used to inspect and edit a spoke though might need to cast it to a concrete type if `TSpoke` is an interface defining only the callback surface.

- Overloads of above methods exists for calling non-generically with a `System.Type` instance if needed.

#### Examples

###### Calls from client to server

```c#
public interface IMyHub
{
    Task<int> DoSomethingOnServer(List<double> arg);
}

var conn = new SignalR.Client.HubConnection()
    .WithUrl("http://localhost:53353/MyHub")
    .Build();

var client = new SignalR.Strong.StrongClient();
await client
    .RegisterHub<IMyHub>(conn)
    .ConnectToHubsAsync();

var myHub = client.GetHub<IMyHub>();
var response = await myHub.DoSomethingOnServer(new List<double>() { 0.4, 0.2 });
```

###### Handlers for server to client calls

```c#
public interface IMyHub
{
}

public interface IMySpoke
{
    void DoSomethingOnClient();
}

public class MySpoke : IMySpoke
{
    public bool HasServerCalled = { get; private set; }
    
    public void DoSomethingOnClient()
    {
        this.HasServerCalled = true;
    }
}

var conn = new SignalR.Client.HubConnection()
    .WithUrl("http://localhost:53353/MyHub")
    .Build();

var client = new SignalR.Strong.StrongClient();
await client
    .RegisterHub<IMyHub>(conn)
    .RegisterSpoke<IMySpoke, IMyHub>(new MySpoke())
    .ConnectToHubsAsync();
client.BuildSpokes();

/* Some time after server calls `DoSomethingOnClient` */

var mySpoke = client.GetSpoke<IMySpoke>();
Console.WriteLine(mySpoke.HasServerCalled);
```

###### Streams

```c#
public interface IMyHub
{
    Task<ChannelReader<int>> ServerToClientStream(CancellationToken token);
    Task ClientToServerStream(ChannelReader<int> reader);
}

var conn = new SignalR.Client.HubConnection()
    .WithUrl("http://localhost:53353/MyHub")
    .Build();

var client = new SignalR.Strong.StrongClient();
await client
    .RegisterHub<IMyHub>(conn)
    .ConnectToHubsAsync();
client.Build();

var myHub = client.GetHub<IMyHub>();

// Server to Client
var cts = new CancellationTokenSource();
var stream = await myHub.ServerToClientStream(cts.Token);

// Client to Server
var channel = Channel.CreateUnbounded<int>();
var reader = channel.Reader;
var writer = channel.Writer;
await myHub.ClientToServerStream(reader);
```

### Implementation

`StrongClient` relies on interfaces for hubs and spokes to be defined.
This can be accomplished by a common library so both the server and client use these interfaces in their implementations.
(e.g. `MyHub : Hub<IMySpoke>, IMyHub` on server and `MySpoke : Spoke<IMySpoke>, IMySpoke` on client)

Proxy objects provided by [Castle DynamicProxy](https://www.castleproject.org/projects/dynamicproxy/) are leveraged to provide the API surface of the target hub in a strongly-typed manner.
This also allows interception of method invocations so the underlying `SignalR.Client.HubConnection` can have its `SendAsync(..)`, `InvokeAsync(..)` and `StreamAsChannelAsync(..)` methods invoked as appropriate with proper transformation.

Reflection is heavily used though benchmarks show that overhead from reflection pales in comparison to network latency.
Performance can be further improved by caching interception behavior.

### Limitations

- Due to use of `Reflection.Emit` in Castle DynamicProxy, AOT platforms aren't supported.

- Streams using `IAsyncEnumerable<T>` are currently unsupported. Try streams using `ChannelReader<T>` instead.

- Passing multiple `CancellationToken` and/or `ChannelReader<T>` is undefined behavior.


### Footnote

Provided "as is" under MIT License without warranty.

Copyright (c) Mehmet Akbulut
