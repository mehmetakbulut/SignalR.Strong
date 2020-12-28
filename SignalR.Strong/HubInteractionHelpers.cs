using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("SignalR.Strong.Dynamic")]
[assembly: InternalsVisibleTo("SignalR.Strong.Expressive")]
namespace SignalR.Strong
{
    internal static class HubInteractionHelpers
    {
        public static (object[], CancellationToken) ParseIntoArgsAndToken(object[] arguments)
        {
            var array = new object[arguments.Length];
            var token = default(CancellationToken);
            
            for (var i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is CancellationToken)
                {
                    token = (CancellationToken) arguments[i];
                    array = new object[arguments.Length - 1];
                }
            }
            
            int j = 0;
            for (var i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is CancellationToken)
                {
                    continue;
                }
                else
                {
                    array[j] = arguments[i];
                    j++;
                }
            }

            return (array, token);
        }
    }
}
