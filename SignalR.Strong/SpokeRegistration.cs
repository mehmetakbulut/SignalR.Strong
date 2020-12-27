using System;
using System.Collections.Generic;
using System.Linq;

namespace SignalR.Strong
{
    public sealed class SpokeRegistration : IDisposable
    {
        private List<IDisposable> disposables;

        public SpokeRegistration(List<IDisposable> registrations)
        {
            this.disposables = disposables;
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