using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {
        private IPHostEntry host;
        private IPAddress address;
        private IPEndPoint endPoint;

        private Socket s_Server;
        private Socket? s_Client;
        public Server(String ip, int port){
            host = Dns.GetHostEntry(ip);
            address = host.AddressList[0];
            endPoint = new IPEndPoint(address, port);

            s_Server = new Socket(address.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            s_Server.Bind(endPoint);
            s_Server.Listen(10);

            s_Client = null;
        }

        public void Start(){
            byte [] buffer = new byte[1024];
            String msg;
            s_Client = s_Server.Accept();
            s_Client.Receive(buffer);
            msg = Encoding.ASCII.GetString(buffer);
            Console.WriteLine("Se recibio el msg");
        }
    }
}