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
        //private Socket? s_Client;
        static ConcurrentDictionary<string, Socket> clientesConectados = new ConcurrentDictionary<string, Socket>();
        private Messages mensajes = new Messages();

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
                    Socket s_Client = await s_Server.AcceptAsync();  // Acepta una conexión
                    Task.Run(() => HandleClient(s_Client));  
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task HandleClient(Socket client)
        {
            string? username = null;
            bool isIdentified = false;
            byte[] buffer = new byte[1024];
            while (true)
            {
                int receivedBytes = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

                if (receivedBytes == 0)
                {
                    Console.WriteLine("Cliente desconectado.");
                    break;
                }
                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                Messages.Identify? identifyMessage = Messages.StringToJSON<Messages.Identify>(jsonMessage);

                if (!isIdentified && identifyMessage != null && identifyMessage.type == messageType.IDENTIFY)
                {
                    username = identifyMessage.username;
                    if (!clientesConectados.ContainsKey(username!))
                    {
                        isIdentified = true;  
                        clientesConectados.TryAdd(username!, client); 
                        Console.WriteLine(username + " se conecto"); 

                        Messages.Identify response = new Messages.Identify(messageType.RESPONSE, messageType.IDENTIFY, "SUCCESS", username!);
                        await SendMessageToClient(client, response);
                        BroadcastNewUser(username!);
                    }
                    else
                    {
                        Messages.Identify response = new Messages.Identify(messageType.RESPONSE, messageType.IDENTIFY, "USER_ALREADY_EXISTS", username!);
                        await SendMessageToClient(client,response);
                    }
                } 
                else if(isIdentified)
                {
                    Console.WriteLine($"[{username}]: {jsonMessage}");
                }
                else
                {
                    Messages.Invalid response = new Messages.Invalid(messageType.RESPONSE, messageType.INVALID, "NOT_IDENTIFIED");
                    await SendMessageToClient(client,response);
                }
            }
        }

        public async Task SendMessageToClient<T>(Socket client, T message)where T : class
        {
            string jsonMessage = mensajes.JSONToString(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            await client.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
        }

        private void BroadcastNewUser(string username)
        {
            foreach (var client in clientesConectados)
            {
                if (client.Key != username)
                {
                    Messages.Identify newUserMessage = new Messages.Identify(messageType.NEW_USER, username);
                    SendMessageToClient(client.Value, newUserMessage).Wait();
                }
            }
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