using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.Json;
using System.Collections.Concurrent;
using General;



namespace ServerApp
{
    class Server
    {
        private IPHostEntry host;
        private IPAddress address;
        private IPEndPoint endPoint;

        private Socket s_Server;
        private Socket? s_Client;
        static ConcurrentDictionary<string, Socket> clientesConectados = new ConcurrentDictionary<string, Socket>();

        public Server(String ip, int port){
            host = Dns.GetHostEntry(ip);
            address = host.AddressList[0];
            endPoint = new IPEndPoint(address, port);

            s_Server = new Socket(address.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            s_Server.Bind(endPoint);
            s_Server.Listen(int.MaxValue); 

            Console.WriteLine("El servidor se ha conectado");
        }
        public async Task Start(){
            Console.WriteLine("Esperando conexiones...");
            
            while (true)
            {
                try
                {
                    s_Client = await s_Server.AcceptAsync();  // Acepta una conexión
                    Thread clientThread = new Thread(ConectClient);
                    clientThread.Start(s_Client);
                    Console.WriteLine("Cliente conectado");
                    
                    string clientId = s_Client.RemoteEndPoint.ToString(); // Identify the client by IP and port
                    clientesConectados.TryAdd(clientId, s_Client);
                    //agregar cliente al cuarto
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async void ConectClient(object client){
            Socket s_Client = (Socket)client;//instancia que se conecta con el cliente 
            byte [] buffer;
            string msg;
            string clientId = s_Client.RemoteEndPoint.ToString();
            try 
            {//parte para leer los mensjaes del cliente al servidor
                while(true){
                    buffer = new byte[1024];
                    int quantitieByte = await s_Client.ReceiveAsync(buffer);
                    if(quantitieByte == 0){
                        Console.WriteLine("El cliente se ha desconectado");
                        break;
                    }
                    msg = byteToString(buffer);
                    Console.WriteLine("Mensaje recibido: " + msg);
                    Console.Out.Flush();

                    await SendMessageToClient(s_Client, "Mensaje enviado");
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
                clientesConectados.TryRemove(clientId, out _); 
                Console.WriteLine("Conexión cerrada con el cliente.");
            }  
        }

        public async Task SendMessageToClient(Socket client, string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            await client.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
        }

        public async Task Receive(Socket client)
        {
            //implementar how to recieve the jsonssss
        }

        

        public String byteToString(byte[]buffer){
            String msg;
            int cadenaIndex;
            msg = Encoding.UTF8.GetString(buffer); 
            cadenaIndex = msg.IndexOf('\0');
            if(cadenaIndex > 0){
                msg = msg.Substring(0,cadenaIndex);
            }
            return msg; 
            
        }
        //ocupar lock para cuando el server le responda al cliente 
        //guardar estados de los usuarios identifiacados
        //guardar salas creadas 
    }
}