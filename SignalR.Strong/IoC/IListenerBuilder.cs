using System;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong.IoC
{
    internal interface IListenerBuilder
    {
        IDisposable Build(IServiceProvider services, HubConnection connection, MethodInfo method, Type[] parameterTypes);
    }
}