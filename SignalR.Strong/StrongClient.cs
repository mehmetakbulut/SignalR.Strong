using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace SignalR.Strong
{
    public class StrongClient
    {
        private IServiceProvider _provider;
        private ProxyGenerator _proxyGenerator = new ProxyGenerator();
        private Dictionary<System.Type, HubConnection> _hubConnections = new Dictionary<Type, HubConnection>();
        private Dictionary<System.Type, object> _hubs = new Dictionary<Type, object>();
        private Dictionary<System.Type, System.Type> _spokeToHubMapping = new Dictionary<Type, Type>();
        private Dictionary<System.Type, object> _spokes = new Dictionary<Type, object>();

        public bool IsBuilt { get; private set; }
        
        public bool IsConnected { get; private set; }

        public StrongClient()
        {

        }

        public StrongClient(IServiceProvider provider)
        {
            _provider = provider;
        }

        public StrongClient RegisterSpoke<TSpoke, THub>()
            where TSpoke : Spoke
        {
            if(IsBuilt) throw new AccessViolationException("Can not map spokes after the client has been built");
            _spokeToHubMapping.Add(typeof(TSpoke), typeof(THub));
            return this;
        }

        public StrongClient RegisterHub<THub>(HubConnection hubConnection) where THub : class
        {
            if (IsBuilt) throw new AccessViolationException("Can not map spokes after the client has been built");
            var hubInterceptor = new HubInterceptor(hubConnection);
            var hubProxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget<THub>(hubInterceptor.ToInterceptor());
            //_services.AddSingleton<THub>(hubProxy);
            _hubConnections[typeof(THub)] = hubConnection;
            _hubs[typeof(THub)] = hubProxy;
            return this;
        }

        public THub GetHub<THub>()
        {
            return (THub)_hubs[typeof(THub)];
        }

        public HubConnection GetConnection<THub>()
        {
            return _hubConnections[typeof(THub)];
        }

        public TSpoke GetSpoke<TSpoke>()
        {
            if(!IsBuilt) throw new AccessViolationException("Client must first be built!");
            return (TSpoke)_spokes[typeof(TSpoke)];
        }

        public async Task<StrongClient> ConnectToHubsAsync()
        {
            foreach (var connection in _hubConnections.Values)
            {
                await connection.StartAsync();
            }

            IsConnected = true;
            return this;
        }

        public StrongClient Build()
        {
            IsBuilt = true;
            foreach (var types in _spokeToHubMapping)
            {
                var spokeType = types.Key;
                var hubType = types.Value;
                Spoke spoke;
                if (_provider is null)
                {
                    spoke = (Spoke) Activator.CreateInstance(spokeType);
                }
                else
                {
                    spoke = (Spoke) ActivatorUtilities.CreateInstance(_provider, spokeType);
                }
                spoke.Connection = _hubConnections[hubType];
                spoke.Client = this;
                var selfMethods = spokeType.GetMethods(BindingFlags.Instance | BindingFlags.Public).ToDictionary(info => info.Name);
                var baseType = spokeType.BaseType;
                if (baseType == null || baseType.BaseType != typeof(Spoke))
                {
                    throw new TypeAccessException("A spoke must inherit Spoke<TSpokeInterface> which inherits from Spoke");
                }
                var intfType = baseType.GetGenericArguments()[0];
                var intfMethods = intfType.GetMethods().ToDictionary(info => info.Name);
                foreach (var method in intfMethods)
                {
                    if (selfMethods.TryGetValue(method.Key, out var x))
                    {
                        spoke.Connection.On(method.Key, method.Value.GetParameters().Select(a => a.ParameterType).ToArray(),
                            objects =>
                            {
                                x.Invoke(spoke, objects);
                                return Task.CompletedTask;
                            });
                    }
                    else
                    {
                        throw new MissingMethodException(spokeType.FullName, method.Key);
                    }
                }
                _spokes[spokeType] = spoke;
            }
            return this;
        }
    }
}
