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
                HandleRequest(jsonMessage,client);
                
            }
        }

        public async void HandleRequest(string jsonMessage, Socket client)
        {
            try
                {
                    var toRecognize = JsonSerializer.Deserialize<Messages.Identify>(jsonMessage);

                    if(toRecognize?.type == null)
                    {
                        Console.WriteLine("Unknown or invalid message received.");
                        return;    
                    }

                    switch(toRecognize.type)
                    {
                        case messageType.IDENTIFY:
                            await HandleIdentifyRequest(jsonMessage,client);
                            break;
                        
                        default:
                            Console.WriteLine("Unhandled message type.");
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error parsing message: {ex.Message}");
                }
        }

        private async Task HandleIdentifyRequest(string jsonMessage,Socket client)
        {
            bool isIdentified = false;
            string? username = null;
            Messages.Identify? toRecognize = Messages.StringToJSON<Messages.Identify>(jsonMessage);
            if (!isIdentified && toRecognize != null)
            {
                isIdentified = await HandleClientIdentification(client, toRecognize);
                username = toRecognize?.username;
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

         public async Task Disconnect(Socket client){
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
                // Messages.Disconnect response = new Messages.Disconnect(messageType.DISCONNECTED, username!);
                // await SendMessageToClient(client, response);
                // clientesConectados.TryRemove(client.Key, out _);
            }
            catch (SocketException se)
            {
                Console.WriteLine($"Socket error during disconnection: {se.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during disconnection: {ex.Message}");
            }
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
            foreach (var client in clientesConectados)//para que se envie el mensaje a cada cliente conectado
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

    }
}