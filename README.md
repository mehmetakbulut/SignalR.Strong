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

|   Type |                            Method |       Mean |     Error |    StdDev |     Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------- |---------------------------------- |-----------:|----------:|----------:|-----------:|-------:|------:|------:|----------:|
|    Rpc |                 GetVoid_SendAsync |   3.967 us | 0.0264 us | 0.1361 us |   3.971 us | 0.0153 |     - |     - |    1.3 KB |
|    Rpc |               GetVoid_InvokeAsync | 101.389 us | 0.4854 us | 2.5082 us | 100.923 us |      - |     - |     - |   3.52 KB |
|    Rpc |                    GetVoid_Strong | 101.674 us | 0.5258 us | 2.6935 us | 101.162 us |      - |     - |     - |   3.74 KB |
|    Rpc |             GetVoid_ExprSendAsync |   6.757 us | 0.0552 us | 0.2867 us |   6.716 us | 0.0153 |     - |     - |   2.05 KB |
|    Rpc |           GetVoid_ExprInvokeAsync | 108.218 us | 0.7766 us | 3.9778 us | 107.266 us |      - |     - |     - |   4.16 KB |
|    Rpc |          GetValueType_InvokeAsync | 105.519 us | 0.7621 us | 3.9382 us | 105.219 us |      - |     - |     - |   3.88 KB |
|    Rpc |               GetValueType_Strong | 105.812 us | 0.6124 us | 3.1372 us | 105.148 us |      - |     - |     - |   4.16 KB |
|    Rpc |                 GetValueType_Expr | 110.480 us | 0.6239 us | 3.1278 us | 110.140 us |      - |     - |     - |   4.52 KB |
|    Rpc |          SetValueType_InvokeAsync | 110.714 us | 0.8177 us | 4.2400 us | 109.678 us |      - |     - |     - |   4.15 KB |
|    Rpc |               SetValueType_Strong | 113.168 us | 0.9252 us | 4.7724 us | 112.164 us |      - |     - |     - |   4.41 KB |
|    Rpc |                 SetValueType_Expr | 214.109 us | 1.3943 us | 7.0915 us | 212.251 us |      - |     - |     - |   8.41 KB |
| Stream |   GetChannel_StreamAsChannelAsync |  36.887 us | 0.4177 us | 2.1734 us |  36.567 us | 0.1221 |     - |     - |  15.66 KB |
| Stream |                 GetChannel_Strong |  42.712 us | 0.5021 us | 2.5076 us |  41.729 us | 0.2441 |     - |     - |  16.81 KB |
| Stream |                   GetChannel_Expr | 294.537 us | 0.9812 us | 4.9729 us | 292.374 us |      - |     - |     - |  21.71 KB |
| Stream |              SetChannel_SendAsync |  24.552 us | 0.1750 us | 0.8980 us |  24.398 us | 0.0610 |     - |     - |   7.29 KB |
| Stream |                 SetChannel_Strong |  26.178 us | 0.1168 us | 0.5939 us |  26.193 us | 0.0610 |     - |     - |   7.72 KB |
| Stream |                   SetChannel_Expr | 220.633 us | 0.3706 us | 1.8984 us | 220.332 us |      - |     - |     - |  12.82 KB |
```
`*_SendAsync`, `*_InvokeAsync` and `*_StreamAsChannelAsync` use standard SignalR `HubConnection` methods.

`*_Strong` use methods exposed by `IStrongClient.GetHub<THub>()` while `*_Expr` use methods exposed by `IStrongClient.GetExpressiveHub<THub>()`.

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

- `THub IStrongClient.GetHub<THub>()` returns a hub proxy that you can use for performing strongly-typed calls as well as streaming. Not supported on AOT platforms.

- `ExpressiveHub<THub> IStrongClient.GetExpressiveHub<THub>()` returns an expressive hub that allows you to specify underlying SignalR operation (e.g. `SendAsync` vs `InvokeAsync`).
  This requires you to feed an expression (e.g. `client.GetExpressiveHub<IMyHub>().InvokeAsync(hub => hub.DoSomethingOnServer(arg1, arg2, arg3))`). Should work on AOT but untested with IL2CPP. 

- `HubConnection IStrongClient.GetHubConnection<THub>()` returns underlying hub connection for performing low-level operations such as registering for events. (e.g. `Reconnecting`)

- `TSpoke IStrongClient.GetSpoke<TSpoke>()` can be used to inspect and edit a spoke though might need to cast it to a concrete type if `TSpoke` is an interface defining only the callback surface.

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

var client = new SignalR.Strong.StrongClient();
await client
    .RegisterHub<IMyHub>(conn)
    .ConnectToHubsAsync();
client.Build();

var ehub = client.GetExpressiveHub<IMyHub>();

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

`StrongClient` relies on interfaces for hubs and spokes to be defined.
This can be accomplished by a common library so both the server and client use these interfaces in their implementations.
(e.g. `MyHub : Hub<IMySpoke>, IMyHub` on server and `MySpoke : Spoke<IMySpoke>, IMySpoke` on client)

There are two implementations of hub calls: proxy (`IStrongClient.GetHub<T>()`) and expressive (`IStrongClient.GetExpressiveHub<T>()`).
Proxy calls are the recommended approach since they offer better performance and simplicity.
Expressive calls are offered as an alternative for many AOT platforms as well as the ability to run specific `HubConnection` methods while maintaining some type safety.

Proxy hubs provided by [Castle DynamicProxy](https://www.castleproject.org/projects/dynamicproxy/) are leveraged to provide the API surface of the target hub in a strongly-typed manner.
This also allows interception of method invocations so the underlying `SignalR.Client.HubConnection` can have its `SendAsync(..)`, `InvokeAsync(..)` and `StreamAsChannelAsync(..)` methods invoked as appropriate with proper transformation.
Reflection is heavily used though benchmarks show that overhead from reflection pales in comparison to network latency.
Performance can be further improved by caching interception behavior. Since DynamicProxy uses `Reflection.Emit`, proxy hubs won't work on most AOT platforms.

Expressive hubs have a subset of the `HubConnection` API surface but take in `System.Linq.Expression` instead.
This works by grabbing the method call in the expression, computing the values of this method's own arguments (compiled on JIT and interpreted on AOT) as well as grabbing the name and argument types of the method.
After which these intermediary products are fed into `HubConnection` method call.
This is rather expensive and should be avoided on JIT platforms without good reason.
On AOT platforms, this might be the only viable option though it is hard to guarantee it would run on all AOT targets.

### Limitations

- Due to use of `Reflection.Emit` in Castle DynamicProxy, `IStrongClient.GetHub<THub>()` isn't supported on AOT platforms. Try `IStrongClient.GetExpressiveHub<THub>()` instead though that may still not work for all cases such as IL2CPP.

- Streams using `IAsyncEnumerable<T>` are currently unsupported via `IStrongClient.GetHub<THub>()`. Try streams using `ChannelReader<T>` or `IStrongClient.GetExpressiveHub<THub>()` instead.

- Passing of multiple `CancellationToken`, `ChannelReader<T>` and `IAsyncEnumerable<T>` are undefined behavior.


### Footnote

Provided "as is" under MIT License without warranty.

Copyright (c) Mehmet Akbulut
