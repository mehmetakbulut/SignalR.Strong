using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace SignalR.Strong
{
    public class StrongClient : IStrongClient
    {
        private IServiceProvider _provider;
        private ProxyGenerator _proxyGenerator;
        private Dictionary<System.Type, HubConnection> _hubConnections = new Dictionary<Type, HubConnection>();
        private Dictionary<System.Type, object> _hubs = new Dictionary<Type, object>();
        private Dictionary<System.Type, System.Type> _spokeToHubMapping = new Dictionary<Type, Type>();
        private Dictionary<System.Type, System.Type> _spokeToImplMapping = new Dictionary<Type, Type>();
        private Dictionary<System.Type, List<IDisposable>> _spokeToHandlerRegistrationsMapping = new Dictionary<Type, List<IDisposable>>();
        private Dictionary<System.Type, object> _spokes = new Dictionary<Type, object>();

        private bool isDynamicCodeGenerationSupported;

        public bool IsBuilt { get; private set; }

        public StrongClient()
        {
            createProxyGeneratorIfSupported();
        }

        public StrongClient(IServiceProvider provider)
        {
            _provider = provider;
            createProxyGeneratorIfSupported();
        }

        private void createProxyGeneratorIfSupported()
        {
            try
            {
                _proxyGenerator = new ProxyGenerator();
                var _ = _proxyGenerator.CreateInterfaceProxyWithoutTarget(typeof(IStrongClient));
                isDynamicCodeGenerationSupported = true;
            }
            catch
            {
                isDynamicCodeGenerationSupported = false;
            }
        }

        public IStrongClient RegisterSpoke<TSpoke, THub>()
        {
            return this.RegisterSpoke<TSpoke, TSpoke, THub>();
        }
        
        public IStrongClient RegisterSpoke<TSpokeIntf, TSpokeImpl, THub>()
            where TSpokeImpl : TSpokeIntf
        {
            throwIfBuilt(true);
            _spokeToHubMapping.Add(typeof(TSpokeIntf), typeof(THub));
            _spokeToImplMapping.Add(typeof(TSpokeIntf), typeof(TSpokeImpl));
            _spokeToHandlerRegistrationsMapping.Add(typeof(TSpokeIntf), new List<IDisposable>());
            return this;
        }

        public IStrongClient RegisterSpoke<TSpokeIntf, TSpokeImpl, THub>(TSpokeImpl spoke)
            where TSpokeImpl : TSpokeIntf
        {
            this.RegisterSpoke<TSpokeIntf, TSpokeImpl, THub>();
            _spokes.Add(typeof(TSpokeIntf), spoke);
            return this;
        }

        public IStrongClient RegisterHub<THub>(HubConnection hubConnection) where THub : class
        {
            return this.RegisterHub(typeof(THub), hubConnection);
        }
        
        public IStrongClient RegisterHub(Type hubType, HubConnection hubConnection)
        {
            throwIfBuilt(true);
            _hubConnections[hubType] = hubConnection;
            if (this.isDynamicCodeGenerationSupported)
            {
                try
                {
                    var hubInterceptor = new HubInterceptor(hubConnection);
                    var hubProxy =
                        _proxyGenerator.CreateInterfaceProxyWithoutTarget(hubType, hubInterceptor.ToInterceptor());
                    _hubs[hubType] = hubProxy;
                }
                catch
                {
                    _hubConnections.Remove(hubType);
                    throw;
                }
            }
            return this;
        }

        public THub GetHub<THub>()
        {
            return (THub)this.GetHub(typeof(THub));
        }
        
        public object GetHub(Type hubType)
        {
            return _hubs[hubType];
        }
        
        public ExpressiveHub<THub> GetExpressiveHub<THub>()
        {
            return new ExpressiveHub<THub>(_hubConnections[typeof(THub)]);
        }

        public HubConnection GetConnection<THub>()
        {
            return this.GetConnection(typeof(THub));
        }
        
        public HubConnection GetConnection(Type hubType)
        {
            return _hubConnections[hubType];
        }

        public TSpoke GetSpoke<TSpoke>()
        {
            throwIfBuilt(false);
            return (TSpoke) this.GetSpoke(typeof(TSpoke));
        }
        
        public object GetSpoke(Type spokeType)
        {
            throwIfBuilt(false);
            return _spokes[spokeType];
        }

        private void throwIfBuilt(bool shouldBeBuilt = true)
        {
            if (IsBuilt == shouldBeBuilt) throw new AccessViolationException("Client must first be built!");
        }

        public async Task<IStrongClient> ConnectToHubsAsync()
        {
            await Task.WhenAll(_hubConnections.Values
                .Where(conn => conn.State == HubConnectionState.Disconnected)
                .Select(conn => conn.StartAsync()));

            return this;
        }

        public IStrongClient BuildSpokes()
        {
            IsBuilt = true;
            foreach (var types in _spokeToHubMapping)
            {
                var intfType = types.Key;
                var implType = this._spokeToImplMapping[intfType];
                var hubType = types.Value;
                buildSpoke(intfType, implType, hubType);
            }
            return this;
        }

        private void buildSpoke(Type intfType, Type implType, Type hubType)
        {
            if (!_spokes.TryGetValue(intfType, out object spoke))
            {
                // If spoke doesn't exist, create it
                if (_provider is null)
                {
                    spoke = Activator.CreateInstance(implType);
                }
                else
                {
                    // Perform dependency injection if a service provider is given
                    spoke = ActivatorUtilities.CreateInstance(_provider, implType);
                }
            }
            
            var connection = _hubConnections[hubType];
            
            if (typeof(ISpoke).IsAssignableFrom(implType))
            {
                // If spoke implements the spoke interface, then set its properties
                var ispoke = (ISpoke) spoke;
                ispoke.Connection = connection;
                ispoke.Client = this;
                ispoke.WeakHub = this.GetHub(hubType);
            }

            var methods = intfType.GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var reg = connection.On(method.Name, method.GetParameters().Select(a => a.ParameterType).ToArray(),
                    objects =>
                    {
                        method.Invoke(spoke, objects);
                        return Task.CompletedTask;
                    });
                _spokeToHandlerRegistrationsMapping[intfType].Add(reg);
            }

            _spokes[intfType] = spoke;
        }
    }
}
