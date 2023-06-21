using System.Threading.Tasks;

namespace SignalR.Strong.Samples.Common.Hubs
{
    public interface IMockBase2
    {
        Task<string> Hello2(string name);
    }
}