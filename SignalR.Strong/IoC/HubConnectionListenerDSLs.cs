using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong.IoC
{
    internal static class HubConnectionListenerDSLs
    {
        public static void ListenWith<TSpoke>(
            this HubConnection connection,
            IServiceProvider services,
            MethodInfo method,
            Action<IDisposable> registerDisconnector
        )
        {
            var listenerBuilderType = typeof(ListenerBuilderImpl<,>)
                .MakeGenericType(typeof(TSpoke), method.ReturnType);
            var listenerBuilder = (IListenerBuilder)Activator
                .CreateInstance(listenerBuilderType);

            var parameterTypes = method
                .GetParameters()
                .Select(a => a.ParameterType)
                .ToArray();
            
            var disconnector = listenerBuilder
                .Build(services, connection, method, parameterTypes);
            registerDisconnector(disconnector);
        }
    }
}