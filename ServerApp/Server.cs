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
                    Socket s_Client = await s_Server.AcceptAsync();  
                    _ = Task.Run(() => HandleClient(s_Client)); 
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
    
            while (true)
            {
                byte[] buffer = new byte[1024];

                int receivedBytes = await ReceiveDataAsync(client, buffer);
                if (receivedBytes == 0)
                {
                    Console.WriteLine("Cliente desconectado");
                    break;
                }

                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                var identifyMessage = ParseIdentifyMessage(jsonMessage);

                if (!isIdentified && identifyMessage != null)
                {
                    isIdentified = await HandleClientIdentification(client, identifyMessage);
                    username = identifyMessage?.username;
                }
                else if (isIdentified)
                {
                    HandleClientMessage(jsonMessage, username);
                }
                else
                {
                    await SendInvalidResponse(client);
                }
            }
        }

        private async Task<int> ReceiveDataAsync(Socket client, byte[] buffer)
        {
            try
            {
                return await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
                return 0; 
            }
        }

        private Messages.Identify? ParseIdentifyMessage(string jsonMessage)
        {
            try
            {
                return Messages.StringToJSON<Messages.Identify>(jsonMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing message: {ex.Message}");
                return null; 
            }
        }

        private async Task<bool> HandleClientIdentification(Socket client, Messages.Identify identifyMessage)
        {
            if (identifyMessage.type == messageType.IDENTIFY)
            {
                string username = identifyMessage.username!;
                if (!clientesConectados.ContainsKey(username!) && username.Length<=8)
                {
                    clientesConectados.TryAdd(username!, client);
                    Console.WriteLine($"{username} connected");

                    Messages.Identify response = new Messages.Identify(messageType.RESPONSE, messageType.IDENTIFY, "SUCCESS", username!);
                    await SendMessageToClient(client, response);
                    BroadcastNewUser(username!);
                    return true;
                }else if(username.Length >= 8){
                    await SendInvalidResponse(client);
                    return false;
                }
                else
                {
                    Messages.Identify response = new Messages.Identify(messageType.RESPONSE, messageType.IDENTIFY, "USER_ALREADY_EXISTS", username!);
                    await SendMessageToClient(client, response);
                    return false;
                }
            }
            return false;
        }

        private void HandleClientMessage(string jsonMessage, string? username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                Console.WriteLine($"[{username}]: {jsonMessage}");
            }
        }

        private async Task SendInvalidResponse(Socket client)
        {
            Messages.Invalid response = new Messages.Invalid(messageType.RESPONSE, messageType.INVALID, "NOT_IDENTIFIED");
            await SendMessageToClient(client, response);
        }

        public async Task SendMessageToClient<T>(Socket client, T message)where T : class
        {
            string jsonMessage = mensajes.JSONToString(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            await client.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
        }

        private async void BroadcastNewUser(string username)
        {
            foreach (var client in clientesConectados)
            {
                try
                {
                    if (client.Key != username)
                    {
                        Messages.Identify newUserMessage = new Messages.Identify(messageType.NEW_USER, username);
                        await SendMessageToClient(client.Value, newUserMessage);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to client:{ex.Message}");
                    clientesConectados.TryRemove(client.Key, out _);
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