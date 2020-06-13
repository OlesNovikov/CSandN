using System.Net.Sockets;
using System.Threading;

namespace AdditionalLibrary
{
    public static class Close
    {
        public static void CloseSocket(ref Socket socket)
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }

        public static void CloseThread(ref Thread thread)
        {
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }
    }
}
