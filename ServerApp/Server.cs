using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            Thread h;
            while(true){
                Console.Write("Esperando conexiones...");
                s_Client = s_Server.Accept();
                h = new Thread(ConectClient);
                h.Start(s_Client);
                Console.WriteLine("Se ha conectado exitosamenre");
            } 
        }

        public void ConectClient(object s){
            Socket s_Client = (Socket)s;//instancia que se conecta con el cliente 
            byte [] buffer;
            String msg;
            int cadenaIndex;

            while(true){
                buffer = new byte[1024];
                 s_Client.Receive(buffer);
                 msg = Encoding.ASCII.GetString(buffer);
                 cadenaIndex = msg.IndexOf('\0');
                 if(cadenaIndex > 0){
                    msg = msg.Substring(0,cadenaIndex);
                 }
                 Console.WriteLine("Se recibio el msg: " + msg);
                 Console.Out.Flush();
            } 
        }
        
    }
}