using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text.Json;
using System.Collections.Concurrent;
using General;
using System.Diagnostics.CodeAnalysis;

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
                string username = GetUsernameBySocket(client)!;

                int receivedBytes = await ReceiveDataAsync(client, buffer);
                if (receivedBytes == 0)
                {
                    Console.WriteLine("Cliente desconectado");
                    await BroadcastMessage(username, messageType.DISCONNECT);
                    break;
                }

                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                Console.WriteLine($"Received request from client: {jsonMessage}"); 
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
                        case messageType.STATUS:
                            await HandleStatusRequest(jsonMessage,client);
                            break;
                        case messageType.USERS:
                            await HandleUserListRequest(jsonMessage,client);
                            break;
                        case messageType.TEXT:
                            await HandlePrivateText(jsonMessage,client);
                            break;
                        case messageType.PUBLIC_TEXT:
                            await HandlePublicText(jsonMessage,client);
                            break;
                        case messageType.NEW_ROOM:
                            
                            break;
                        case messageType.INVITE:
                            
                            break;
                        case messageType.JOIN_ROOM:
                            
                            break;
                        case messageType.ROOM_USERS:
                            
                            break;
                        case messageType.LEAVE_ROOM:
                            
                            break;
                        case messageType.DISCONNECT:
                            await HandelDisconnectRequest(jsonMessage,client);
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

        private async Task HandleUserListRequest(string jsonMessage, Socket client)
        {
            throw new NotImplementedException();
        }

        private async Task HandlePrivateText(string jsonMessage, Socket client)
        {
            Messages.Text? toRecognize = Messages.StringToJSON<Messages.Text>(jsonMessage);
            string userToSendTo = toRecognize.username!;
            string username = GetUsernameBySocket(client)!;
            await BroadcastMessageToClient(userToSendTo,username, toRecognize.text!,client);
        }
        private async Task HandlePublicText(string jsonMessage, Socket client)
        {
            Messages.Text? toRecognize = Messages.StringToJSON<Messages.Text>(jsonMessage);
            string username = GetUsernameBySocket(client)!;
            await BroadcastMessageToChat(username, toRecognize.text!);
        }

        private async Task HandleStatusRequest(string jsonMessage, Socket client)
        {
            Messages.Status? toRecognize = Messages.StringToJSON<Messages.Status>(jsonMessage);
            string status = toRecognize.status!;
            string username = GetUsernameBySocket(client)!;
            if(status == "ACTIVE"){

                Messages.Status response = new Messages.Status(messageType.NEW_STATUS, username,status);
                await SendMessageToClient(client, response);
                await BroadcastMessage(username!,messageType.STATUS);//falta implementar 

            }else if(status == "AWAY"){

                Messages.Status response = new Messages.Status(messageType.NEW_STATUS, username,status);
                await SendMessageToClient(client, response);
                await BroadcastMessage(username!,messageType.STATUS);//fatla implementar 

            }else if(status == "BUSY"){
                
                Messages.Status response = new Messages.Status(messageType.NEW_STATUS, username,status);
                await SendMessageToClient(client, response);
                await BroadcastMessage(username!,messageType.STATUS);//falta implementar

            }else{
                Messages.Invalid response = new Messages.Invalid(messageType.RESPONSE, messageType.INVALID, "INVALID");
                await SendMessageToClient(client, response);
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
                username = toRecognize.username;
            }
            else if (isIdentified)
            {
                HandleClientMessage(jsonMessage, username);
            }
            else
            {
                Messages.Invalid response = new Messages.Invalid(messageType.RESPONSE, messageType.INVALID, "NOT_IDENTIFIED");
                await SendMessageToClient(client, response);
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
                    await BroadcastMessage(username!,messageType.IDENTIFY);

                    return true;
                }else if(username.Length >= 8){
                    Messages.Invalid response = new Messages.Invalid(messageType.RESPONSE, messageType.INVALID, "NOT_IDENTIFIED");
                    await SendMessageToClient(client, response);
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

        public async Task HandelDisconnectRequest(string jsonMessage,Socket client){
            Messages.Disconnect? toRecognize = Messages.StringToJSON<Messages.Disconnect>(jsonMessage);
            string username = GetUsernameBySocket(client)!;
            await BroadcastMessage(username, messageType.DISCONNECT);
            if(toRecognize.type == messageType.DISCONNECT)
            {
                try
                {
        
                    clientesConectados.TryRemove(username!,out client!);
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                    
                    
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
            
        } 

        private void HandleClientMessage(string jsonMessage, string? username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                Console.WriteLine($"[{username}]: {jsonMessage}");
            }
        }

        public async Task SendMessageToClient<T>(Socket client, T message)where T : class
        {
            string jsonMessage = mensajes.JSONToString(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
            await client.SendAsync(new ArraySegment<byte>(messageBytes), SocketFlags.None);
        }

        private async Task BroadcastMessage(string username, messageType operation)
        {
            foreach (var client in clientesConectados)
            {
                try
                {
                    if (client.Key != username)
                    {
                        switch(operation)
                        {
                            case messageType.DISCONNECT:
                                Messages.Disconnect response = new Messages.Disconnect(messageType.DISCONNECTED, username);
                                await SendMessageToClient(client.Value, response);
                                break;
                            case messageType.IDENTIFY:
                                Messages.Identify newUserMessage = new Messages.Identify(messageType.NEW_USER, username);
                                await SendMessageToClient(client.Value, newUserMessage);
                                break;
                            default:
                                Console.WriteLine("Unhandled message type.");
                                break;
                        }
                        
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to client:{ex.Message}");
                    clientesConectados.TryRemove(client.Key, out _);
                }
            }
        }

        private async Task BroadcastMessageToChat(string username,string text)
        {
            foreach (var client in clientesConectados)
            {
                try
                {
                    if (client.Key != username)
                    {
                        Messages.Text message = new Messages.Text(messageType.PUBLIC_TEXT_FROM, username,text);
                        await SendMessageToClient(client.Value, message);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to client:{ex.Message}");
                    clientesConectados.TryRemove(client.Key, out _);
                }
            }
        }

        private async Task BroadcastMessageToClient(string usernameToSendTo,string username, string msg, Socket c)
        {
            Socket value;
            foreach (var client in clientesConectados)
            {
                if(client.Key == usernameToSendTo)
                {
                    value = client.Value;
                    try
                    {
                        if (client.Key != username)
                        {
                            Messages.Text messagePriv = new Messages.Text(messageType.TEXT_FROM, username, msg);
                            await SendMessageToClient(value, messagePriv);
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"Error broadcasting to client:{ex.Message}");
                        clientesConectados.TryRemove(client.Key, out _);
                    }
                }
                else if(client.Key != usernameToSendTo)
                {
                    Messages.Text response = new Messages.Text(messageType.RESPONSE, messageType.TEXT, "NO_SUCH_USER",usernameToSendTo);
                    await SendMessageToClient(c,response);
                    break;
                    
                }
                
            }
        }

        private string? GetUsernameBySocket(Socket client)
        {
            foreach (var entry in clientesConectados)
            {
                if (entry.Value == client) 
                {
                    return entry.Key;
                }
            }
            return null;
        }

    }
}