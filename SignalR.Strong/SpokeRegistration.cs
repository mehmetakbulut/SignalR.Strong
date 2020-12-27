using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalR.Strong
{
    public sealed class SpokeRegistration : IDisposable
    {
        private List<IDisposable> disposables;
        
        public object Spoke { get; private set; }

        public SpokeRegistration(object spoke, List<IDisposable> registrations)
        {
            this.Spoke = spoke;
            this.disposables = disposables ?? new List<IDisposable>();
            if (this.Spoke is IDisposable disposable)
            {
                this.disposables.Add(disposable);
            }
        }
        
        public void Dispose()
        {
            List<Exception> exs = null;
            foreach (var disposable in disposables)
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    if (exs is null)
                    {
                        exs = new List<Exception>();
                    }

                    exs.Add(ex);
                }
            }

            if (exs != null)
            {
                throw new AggregateException(exs);
            }
        }
    }
}