using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.XPath;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong
{
    internal class HubInterceptor : IInterceptor
    {
        private readonly HubConnection _conn;

        public HubInterceptor(HubConnection connection)
        {
            _conn = connection;
        }

        public void InterceptSynchronous(IInvocation invocation)
        { 
            invocation.ReturnValue = _conn.SendCoreAsync(invocation.Method.Name, invocation.Arguments);
        }
        
        public void InterceptAsynchronous(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous(invocation);
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.ReturnValue = InternalInterceptAsynchronous<TResult>(invocation);
        }

        private async Task InternalInterceptAsynchronous(IInvocation invocation)
        {
            if (hasChannelReaderArgument(invocation.Method))
            {
                var (args, token) = HubInteractionHelpers.ParseIntoArgsAndToken(invocation.Arguments);
                await _conn.SendCoreAsync(invocation.Method.Name, args, token);
            }
            else
            {
                await _conn.InvokeCoreAsync(invocation.Method.Name, invocation.Arguments);
            }
        }

        private bool hasChannelReaderArgument(MethodInfo method)
        {
            foreach (var parameter in method.GetParameters())
            {
                if (typeof(ChannelReader<>).IsAssignableFrom(parameter.ParameterType.GetGenericTypeDefinition()))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation)
        {
            Task<TResult> ret = null;
            if (typeof(TResult).IsConstructedGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(ChannelReader<>))
            {
                var (args, token) = HubInteractionHelpers.ParseIntoArgsAndToken(invocation.Arguments);
                var method = typeof(HubConnectionExtensions).GetMethod("StreamAsChannelCoreAsync")
                    .MakeGenericMethod(typeof(TResult).GenericTypeArguments[0]);
                ret = (Task<TResult>) method.Invoke(null, new object[] {_conn, invocation.Method.Name, args, token});
            }
            else
            {
                ret = _conn.InvokeCoreAsync<TResult>(invocation.Method.Name, invocation.Arguments);
            }
            return await ret;
        }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method;
            var (args, token) = HubInteractionHelpers.ParseIntoArgsAndToken(invocation.Arguments);
            if (method.ReturnType.IsConstructedGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var a = method.GetGenericArguments()[0];
                if (a.IsConstructedGenericType && a.GetGenericTypeDefinition() == typeof(ChannelReader<>))
                {
                    var b = a.GetGenericArguments()[0];
                    var hubMethod = typeof(HubConnectionExtensions).GetMethod("StreamAsChannelCoreAsync")
                        .MakeGenericMethod(b);
                    invocation.ReturnValue = method.Invoke(null, new object[] {_conn, method.Name, args, token});
                    return;
                }
                else
                {
                    invocation.ReturnValue = _conn.InvokeCore
                }
            }

            if (method.ReturnType.IsConstructedGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
            {
                var b = method.ReturnType.GetGenericArguments()[0];
                var hubMethod = typeof(HubConnectionExtensions).GetMethod("StreamAsyncCore")
                    .MakeGenericMethod(b);
                invocation.ReturnValue = method.Invoke(null, new object[] {_conn, method.Name, args, token});
                return;
            }
            
            if (method.ReturnType != typeof(Task) && method.ReturnType.GetGenericTypeDefinition() != typeof(Task<>))
            {
                var b = method.ReturnType.GetGenericArguments()[0];
                invocation.ReturnValue = _conn.SendCoreAsync(method.Name, args, token);
                return;
            }

            if (hasChannelReaderArgument(method))
            {
                invocation.ReturnValue = _conn.SendCoreAsync(method.Name, args, token);
                return;
            }
            
            if (method.ReturnType.IsGenericType && method.)
        }
    }
}
