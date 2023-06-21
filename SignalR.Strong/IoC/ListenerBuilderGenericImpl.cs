using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable HeapView.PossibleBoxingAllocation
// ReSharper disable HeapView.ClosureAllocation
// ReSharper disable UnusedTypeParameter

namespace SignalR.Strong.IoC
{
    internal sealed class ListenerBuilderGenericImpl<TSpoke, TResult>
        : IListenerBuilder
    {
        public IDisposable Build(
            IServiceProvider services,
            HubConnection connection,
            MethodInfo method,
            Type[] parameterTypes
        )
        {
            return connection.On(method.Name, parameterTypes, async args =>
            {
                using (var scope = services.CreateAsyncScope())
                {
                    var spoke = scope.ServiceProvider.GetRequiredService<TSpoke>();
                    if (spoke is ISpoke sp)
                        sp.Connection = connection;
                    
                    var result = method.Invoke(spoke, args);
                    var task = (Task<TResult>)result;
                    
                    return await task;
                }
            });
        }
    }
}