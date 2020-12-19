using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace SignalR.Strong
{
    public abstract class Spoke : IDisposable
    {
        public HubConnection Connection;
        public StrongClient Client;
        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
        }
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Dispose(true);

            _disposed = true;
        }
    }

    public abstract class Spoke<T> : Spoke
    {
    }
}
