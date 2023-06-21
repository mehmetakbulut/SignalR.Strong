using System.Threading.Tasks;

namespace SignalR.Strong.Samples.Common.Hubs
{
    public interface IMockBase1 : IMockBase2
    {
        Task<string> Hello1(string name);
    }
}