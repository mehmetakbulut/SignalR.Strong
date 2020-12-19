[![Build status](https://ci.appveyor.com/api/projects/status/w2grsvnercr66p95/branch/master?svg=true)](https://ci.appveyor.com/project/mehmetakbulut/signalr-strong/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/mehmetakbulut/SignalR.Strong/badge.svg?branch=master)](https://coveralls.io/github/mehmetakbulut/SignalR.Strong?branch=master)

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
- Support for client-to-server and server-to-client streams using `Channel.Reader<T>`
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

|   Type |                            Method |       Mean |    Error |    StdDev |    Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------- |---------------------------------- |-----------:|---------:|----------:|----------:|-------:|------:|------:|----------:|
|    Rpc |                 GetVoid_SendAsync |   3.826 us | 0.016 us |  0.084 us |   3.82 us | 0.0153 |     - |     - |   1.31 KB |
|    Rpc |               GetVoid_InvokeAsync |  97.114 us | 0.249 us |  1.279 us |  97.14 us |      - |     - |     - |   3.52 KB |
|    Rpc |                    GetVoid_Strong |  97.684 us | 0.258 us |  1.337 us |  97.77 us |      - |     - |     - |   3.74 KB |
|    Rpc |          GetValueType_InvokeAsync |  99.393 us | 0.224 us |  1.159 us |  99.40 us |      - |     - |     - |   3.88 KB |
|    Rpc |               GetValueType_Strong | 101.034 us | 0.242 us |  1.252 us | 101.05 us |      - |     - |     - |   4.16 KB |
|    Rpc |          SetValueType_InvokeAsync | 105.773 us | 0.297 us |  1.541 us | 105.79 us |      - |     - |     - |   4.15 KB |
|    Rpc |               SetValueType_Strong | 106.012 us | 0.302 us |  1.556 us | 106.13 us |      - |     - |     - |   4.41 KB |
| Stream |    GetReader_StreamAsChannelAsync |   33.61 us | 0.147 us |  0.730 us |  33.43 us | 0.1831 |     - |     - |  15.61 KB |
| Stream |                  GetReader_Strong |   40.11 us | 0.062 us |  0.316 us |  40.08 us | 0.1221 |     - |     - |  16.79 KB |
| Stream |    SetReader_StreamAsChannelAsync |   22.24 us | 0.108 us |  0.562 us |  22.31 us | 0.0610 |     - |     - |   7.44 KB |
| Stream |                  SetReader_Strong |   24.87 us | 0.130 us |  0.676 us |  24.90 us | 0.0610 |     - |     - |   7.75 KB |
```

### Usage

###### Calls from client to server

```c#
public interface IMyHub
{
    Task<int> DoSomethingOnServer(List<double> arg);
}

var conn = new SignalR.Client.HubConnection()
    .WithUrl("http://localhost:53353/MyHub")
    .Build();

var client = SignalR.Strong.StrongClient();
await client
    .RegisterHub<IMyHub>(conn)
    .ConnectToHubsAsync();
client.Build();

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

public class MySpoke : SignalR.Strong.Spoke<IMySpoke>, IMySpoke
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

var client = SignalR.Strong.StrongClient();
await client
    .RegisterHub<IMyHub>(conn)
    .RegisterSpoke<MySpoke, IMyHub>()
    .ConnectToHubsAsync();
client.Build();

/* Some time after server calls `DoSomethingOnClient` */

var mySpoke = client.GetSpoke<MySpoke>();
Console.WriteLine(mySpoke.HasServerCalled);
```

###### Streams

```c#
public interface IMyHub
{
    Task<Channel.Reader<int>> ServerToClientStream(CancellationToken token);
    Task ClientToServerStream(Channel.Reader<int> reader);
}

var conn = new SignalR.Client.HubConnection()
    .WithUrl("http://localhost:53353/MyHub")
    .Build();

var client = SignalR.Strong.StrongClient();
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

Reflection is heavily used though there is no `Reflection.Emit` so should be fine on platforms that don't support `Reflection.Emit`.
Benchmarks show that overhead from reflection pales in comparison to network latency so it is not a problem. Performance can be further improved by caching interception behavior.  

### Limitations

- Streams using `IAsyncEnumerable<T>` are currently unsupported. Try streams using `Channel.Reader<T>` instead.

- Passing multiple `CancellationToken` and/or `Channel.Reader<T>` is undefined behavior.


### Footnote

Provided "as is" under MIT License without warranty.

Copyright (c) Mehmet Akbulut
