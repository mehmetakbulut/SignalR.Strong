[![Build status](https://ci.appveyor.com/api/projects/status/w2grsvnercr66p95/branch/master?svg=true)](https://ci.appveyor.com/project/mehmetakbulut/signalr-strong/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/mehmetakbulut/SignalR.Strong/badge.svg?branch=master)](https://coveralls.io/github/mehmetakbulut/SignalR.Strong?branch=master)
[![Nuget](https://img.shields.io/nuget/v/SignalR.Strong)](https://www.nuget.org/packages/SignalR.Strong/)
[![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/SignalR.Strong)](https://www.nuget.org/packages/SignalR.Strong/)

_**Work in progress, API may change**_

# SignalR.Strong

[SignalR Core](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-3.1) hubs can define strongly-typed hub methods and also perform strongly-typed server-to-client RPC however clients can neither define strongly-typed client methods nor perform strongly-typed client-to-server RPC.
SignalR.Strong is a .NET Standard 2.0 library which addresses this gap by introducing higher level extensions so end-to-end static type-checking and refactoring is made possible.

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
- Support for client-to-server and server-to-client streams
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

|   Type |                          Method |       Mean |     Error |    StdDev |     Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------- |-------------------------------- |-----------:|----------:|----------:|-----------:|-------:|------:|------:|----------:|
|    Rpc |               GetVoid_SendAsync |   3.794 us | 0.0159 us | 0.0830 us |   3.793 us | 0.0153 |     - |     - |   1.38 KB |
|    Rpc |             GetVoid_InvokeAsync |  96.429 us | 0.2275 us | 1.1651 us |  96.444 us |      - |     - |     - |   3.79 KB |
|    Rpc |                  GetVoid_Strong |  99.873 us | 0.7411 us | 3.8094 us |  98.564 us |      - |     - |     - |   4.02 KB |
|    Rpc |           GetVoid_ExprSendAsync |   6.384 us | 0.0356 us | 0.1848 us |   6.360 us | 0.0153 |     - |     - |   2.14 KB |
|    Rpc |         GetVoid_ExprInvokeAsync | 105.746 us | 0.3789 us | 1.9407 us | 105.653 us |      - |     - |     - |   4.44 KB |
|    Rpc |        GetValueType_InvokeAsync |  98.114 us | 0.1878 us | 0.9752 us |  98.119 us |      - |     - |     - |   4.15 KB |
|    Rpc |             GetValueType_Strong | 102.316 us | 0.4395 us | 2.2476 us | 102.764 us |      - |     - |     - |   4.44 KB |
|    Rpc |               GetValueType_Expr | 108.173 us | 0.4034 us | 2.0698 us | 108.173 us |      - |     - |     - |    4.8 KB |
|    Rpc |        SetValueType_InvokeAsync | 107.273 us | 0.2637 us | 1.3673 us | 107.253 us |      - |     - |     - |   4.42 KB |
|    Rpc |             SetValueType_Strong | 109.812 us | 0.6058 us | 3.0756 us | 109.170 us |      - |     - |     - |   4.69 KB |
|    Rpc |               SetValueType_Expr | 210.252 us | 0.7363 us | 3.7382 us | 209.827 us |      - |     - |     - |   8.68 KB |
| Stream | GetChannel_StreamAsChannelAsync |  34.795 us | 0.3795 us | 1.9164 us |  33.971 us | 0.1831 |     - |     - |  15.83 KB |
| Stream |               GetChannel_Strong |  41.688 us | 0.3619 us | 1.8210 us |  41.156 us | 0.2441 |     - |     - |  17.06 KB |
| Stream |                 GetChannel_Expr | 292.978 us | 1.6017 us | 8.2331 us | 289.330 us |      - |     - |     - |  22.02 KB |
| Stream |            SetChannel_SendAsync |  23.304 us | 0.1247 us | 0.6501 us |  23.333 us | 0.0610 |     - |     - |   7.74 KB |
| Stream |               SetChannel_Strong |  25.724 us | 0.1312 us | 0.6769 us |  25.680 us | 0.0610 |     - |     - |   8.01 KB |
| Stream |                 SetChannel_Expr | 218.007 us | 0.2408 us | 1.2354 us | 217.885 us |      - |     - |     - |  13.33 KB |
```
`*_SendAsync`, `*_InvokeAsync` and `*_StreamAsChannelAsync` use standard SignalR `HubConnection` methods.

`*_Strong` use methods exposed by `AsDynamicHub<THub>()` while `*_Expr` use methods exposed by `AsExpressiveHub<THub>()`.

### Usage

#### Setup

1. Create `HubConnection` as usual.
2. Get strongly typed proxy with `conn.AsDynamicHub<THub>()`.
3. Register spokes which are handlers for server-to-client calls:
```c#
conn.RegisterSpoke<MySpoke>();                  // Simplest form, type will be MySpoke
conn.RegisterSpoke<IMySpoke, MySpoke>();        // Constrain handler interface, type will be IMySpoke
conn.RegisterSpoke<IMySpoke>(new MySpoke());    // Pass instance manually, type will be IMySpoke
```

#### Interaction

- `THub HubConnection.AsDynamicHub<THub>()` returns a dynamic proxy that you can use for performing strongly-typed calls as well as streaming. Not supported on AOT platforms.

- `ExpressiveHub<THub> HubConnection.AsExpressiveHub<THub>()` returns an expressive proxy that allows you to specify underlying SignalR operation (e.g. `SendAsync` vs `InvokeAsync`).
  This requires you to feed an expression (e.g. `conn.AsExpressiveHub<IMyHub>().InvokeAsync(hub => hub.DoSomethingOnServer(arg1, arg2, arg3))`). Should work on AOT but untested with IL2CPP. 
  
- `SpokeRegistration HubConnection.RegisterSpoke<TSpoke>()` registers a server-to-client callback handler and returns registration object which can be used to access the spoke instance. Remember to call `SpokeRegistration.Dispose()` if handler should no longer handle callbacks.

- Overloads of above methods exist for calling non-generically with a `System.Type` instance if needed.

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
    
await conn.StartAsync();

var myHub = conn.AsDynamicHub<IMyHub>();
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

await conn.StartAsync();

var registration = conn.RegisterSpoke<IMySpoke>(new MySpoke())

/* Some time after server calls `DoSomethingOnClient` */

var mySpoke = (MySpoke) registration.Spoke;
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

await conn.StartAsync();

var myHub = conn.AsDynamicHub<IMyHub>();

// Server to Client
var cts = new CancellationTokenSource();
var stream = await myHub.ServerToClientStream(cts.Token);

// Client to Server
var channel = Channel.CreateUnbounded<int>();
var reader = channel.Reader;
var writer = channel.Writer;
await myHub.ClientToServerStream(reader);
```

###### Expressive hubs

```c#
public interface IMyHub
{
    Task DoThisOnServer(List<double> arg);
    Task<int> DoThatOnServer(List<double> arg);
    Task<ChannelReader<int>> ServerToClientStream(CancellationToken token);
    Task ClientToServerStream(ChannelReader<int> reader);
}

var conn = new SignalR.Client.HubConnection()
    .WithUrl("http://localhost:53353/MyHub")
    .Build();

await conn.StartAsync();

var ehub = conn.AsExpressiveHub<IMyHub>();

await ehub.SendAsync(hub => hub.DoThisOnServer(arg));
await ehub.InvokeAsync(hub => hub.DoThisOnServer(arg));

var ret1 = await ehub.InvokeAsync(hub => hub.DoThatOnServer(arg));

var cts = new CancellationTokenSource();
var ret2 = await ehub.StreamAsChannelAsync(hub => hub.ServerToClientStream(token));

var channel = Channel.CreateUnbounded<int>();
var reader = channel.Reader;
var writer = channel.Writer;
await ehub.SendAsync(hub => hub.ClientToServerStream(reader));
```

### Implementation

`SignalR.Strong` relies on interfaces for hubs and spokes to be defined.
This can be accomplished by a common library so both the server and client use these interfaces in their implementations.
(e.g. `MyHub : Hub<IMySpoke>, IMyHub` on server and `MySpoke : IMySpoke` on client)

There are two implementations of hub calls: dynamic proxy (`conn.AsDynamicHub<T>()`) and expressive (`conn.AsExpressiveHub<T>()`).
Dynamic proxy calls are the recommended approach since they offer better performance and simplicity.
Expressive calls are offered as an alternative for many AOT platforms as well as the ability to run specific `HubConnection` methods while maintaining some type safety.

Dynamic proxies provided by [Castle DynamicProxy](https://www.castleproject.org/projects/dynamicproxy/) are leveraged to provide the API surface of the target hub in a strongly-typed manner.
This also allows interception of method invocations so the underlying `SignalR.Client.HubConnection` can have its `SendAsync(..)`, `InvokeAsync(..)` and `StreamAsChannelAsync(..)` methods invoked as appropriate with proper transformation.
Reflection is heavily used though benchmarks show that overhead from reflection pales in comparison to network latency.
Performance can be further improved by caching interception behavior. Since DynamicProxy uses `Reflection.Emit`, proxy hubs won't work on most AOT platforms.

Expressive hubs have a subset of the `HubConnection` API surface but take in `System.Linq.Expression` instead.
This works by grabbing the method call in the expression, computing the values of this method's own arguments (compiled on JIT and interpreted on AOT) as well as grabbing the name and argument types of the method.
After which these intermediary products are fed into `HubConnection` method call.
This is rather expensive and should be avoided on JIT platforms without good reason.
On AOT platforms, this might be the only viable option though it is hard to guarantee it would run on all AOT targets.

### Limitations

- Due to use of `Reflection.Emit` in Castle DynamicProxy, `conn.AsDynamicHub<THub>()` isn't supported on AOT platforms. Try `conn.AsExpressiveHub<THub>()` instead though that may still not work for all cases such as IL2CPP.

- Streams using `IAsyncEnumerable<T>` are currently unsupported via `conn.AsDynamicHub<THub>()`. Try streams using `ChannelReader<T>` or `conn.AsExpressiveHub<THub>()` instead.

- Passing of multiple `CancellationToken`, `ChannelReader<T>` and `IAsyncEnumerable<T>` are undefined behavior.


### Footnote

Provided "as is" under MIT License without warranty.

Copyright (c) Mehmet Akbulut
