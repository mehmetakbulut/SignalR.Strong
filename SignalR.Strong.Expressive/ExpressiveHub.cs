using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong.Expressive
{
    public struct ExpressiveHub<THub>
    {
        private readonly HubConnection conn;

        public ExpressiveHub(HubConnection connection)
        {
            this.conn = connection;
        }
        
        public Task SendAsync(Expression<Action<THub>> expression)
        {
            var (name, arguments, token) = AnalyzeHubCall(expression);
            return conn.SendCoreAsync(name, arguments, token);
        }
        
        public Task InvokeAsync(Expression<Action<THub>> expression)
        {
            var (name, arguments, token) = AnalyzeHubCall(expression);
            return conn.InvokeCoreAsync(name, arguments, token);
        }
        
        public Task<TResult> InvokeAsync<TResult>(Expression<Func<THub, Task<TResult>>> expression)
        {
            var (name, arguments, token) = AnalyzeHubCall(expression);
            return conn.InvokeCoreAsync<TResult>(name, arguments, token);
        }
        
        public Task<ChannelReader<TResult>> StreamAsChannelAsync<TResult>(
            Expression<Func<THub,
            Task<ChannelReader<TResult>>>> expression)
        {
            var (name, arguments, token) = AnalyzeHubCall(expression);
            return conn.StreamAsChannelCoreAsync<TResult>(name, arguments, token);
        }
        
        public IAsyncEnumerable<TResult> StreamAsync<TResult>(
            Expression<Func<THub, IAsyncEnumerable<TResult>>> expression)
        {
            var (name, arguments, token) = AnalyzeHubCall(expression);
            return conn.StreamAsyncCore<TResult>(name, arguments, token);
        }

        private static (string Name, object[] Arguments, CancellationToken Token) AnalyzeHubCall(LambdaExpression exp)
        {
            var call = (MethodCallExpression) exp.Body;
            var name = call.Method.Name;
            var vals = call.Arguments
                .Select(arg =>
                    Expression.Lambda<Func<object>>(Expression.Convert(arg, typeof(object))).Compile()())
                .ToArray(); // Converting arbitrary type to object per https://stackoverflow.com/a/2200247
            var (args, token) = HubInteractionHelpers.ParseIntoArgsAndToken(vals);
            return (name, args, token);
        }
    }
}