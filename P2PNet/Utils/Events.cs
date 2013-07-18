using System;
using System.Threading.Tasks;

namespace P2PNet.Utils
{
    internal static class Events
    {
        internal static void RaiseAsync<T>(EventHandler<T> handler, object sender, T args) where T : System.EventArgs
        {
            Task.Factory.StartNew(() =>
                {
                    if (handler != null)
                    {
                        handler(sender, args);
                    }
                });
        }

        internal static void Raise<T>(EventHandler<T> handler, object sender, T args) where T : System.EventArgs
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }
    }
}