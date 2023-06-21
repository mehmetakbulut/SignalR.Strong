using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable InvertIf
// ReSharper disable HeapView.ObjectAllocation
// ReSharper disable HeapView.ObjectAllocation.Evident
// ReSharper disable HeapView.ClosureAllocation

namespace SignalR.Strong.IoC
{
    internal sealed class ListenerBuilderImpl<TSpoke, TResult>
        : IListenerBuilder
    {
        private static readonly Type ResultType = typeof(TResult);

        public IDisposable Build(
            IServiceProvider services,
            HubConnection connection,
            MethodInfo method,
            Type[] parameterTypes
        )
        {
            if (ResultType == typeof(Task))
            {
                return connection.On(method.Name, parameterTypes, async args =>
                {
                    using (var scope = services.CreateScope())
                    {
                        var spoke = scope
                            .ServiceProvider
                            .GetRequiredService<TSpoke>();

                        if (spoke is ISpoke sp)
                            sp.Connection = connection;

                        var result = (Task)method.Invoke(spoke, args);
                        await (result ?? throw NotSupported());
                    }
                });
            }

            if (ResultType.IsGenericType)
            {
                var genericDef = ResultType
                    .GetGenericTypeDefinition();
                if (genericDef != typeof(Task<>))
                    throw NotSupported();

                var genericArguments = ResultType
                    .GetGenericArguments();

                var builderType = typeof(ListenerBuilderGenericImpl<,>)
                    .MakeGenericType(typeof(TSpoke), genericArguments[0]);

                var builder = (IListenerBuilder)Activator
                    .CreateInstance(builderType);

                return builder
                    .Build(services, connection, method, parameterTypes);
            }

            throw NotSupported();
        }

        private static NotSupportedException NotSupported()
        {
            return new NotSupportedException(
                $"Only async Task or Task<> are supported as a result: {ResultType.FullName}"
            );
        }
    }
}