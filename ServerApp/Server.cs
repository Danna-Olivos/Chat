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
            s_Server.Listen(int.MaxValue); 

            Console.WriteLine("El servidor se ha conectado");
        }

        public void Start(){
            // Thread h;
            // while(true){
            //     Console.Write("Esperando conexiones...");
            //     s_Client = s_Server.Accept();
            //     h = new Thread(ConectClient);
            //     h.Start(s_Client);
            //     Console.WriteLine("Se ha conectado exitosamenre");
            // } 
            Console.WriteLine("Esperando conexiones...");

            while (true)
            {
                try
                {
                    s_Client = s_Server.Accept();  // Acepta una conexión
                    Console.WriteLine("Cliente conectado");

                    // Asigna el cliente a un hilo en el ThreadPool
                    ThreadPool.QueueUserWorkItem(ConectClient, s_Client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public void ConectClient(object client){
            Socket s_Client = (Socket)client;//instancia que se conecta con el cliente 
            byte [] buffer;
            String msg;
            try 
            {
                while(true){
                buffer = new byte[1024];
                int quantitieByte = s_Client.Receive(buffer);
                if(quantitieByte == 0){
                    Console.WriteLine("El cliente se desconecto");
                    break;
                }
                msg = byteToString(buffer);
                Console.WriteLine("Se recibio el msg: " + msg);
                Console.Out.Flush();
                } 
            }
            catch (SocketException se)
            {
                Console.WriteLine($"Error de socket: {se.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
            }
            finally
            {
                // Cierra la conexión con el cliente de manera adecuada
                try
                {
                    s_Client.Shutdown(SocketShutdown.Both); // Apaga el envío y recepción
                }
                catch (SocketException se)
                {
                    // Puede arrojar una excepción si ya se había cerrado previamente
                    Console.WriteLine($"Error al cerrar el socket: {se.Message}");
                }
                s_Client.Close(); // Cierra el socket y libera recursos
                Console.WriteLine("Conexión cerrada con el cliente.");
            }  
        }

        public String byteToString(byte[]buffer){
            String msg;
            int cadenaIndex;
            msg = Encoding.ASCII.GetString(buffer);
            cadenaIndex = msg.IndexOf('\0');
            if(cadenaIndex > 0){
                msg = msg.Substring(0,cadenaIndex);
            }
            return msg; 
            
        }
        
    }
}