using System.Threading.Tasks;

namespace SignalR.Strong.Samples.Common.Hubs
{
    public interface IMockBase
    {
        Task<string> Hello(string name);
    }
}