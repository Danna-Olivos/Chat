using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using General;

namespace ClientApp
{
    class Client{
        private IPHostEntry host;
        private IPAddress address;
        private IPEndPoint endPoint;

        private Socket s_Client;

        String? userName{get; set;}
        String? status{get; set;}
        bool isIdentified{get;set;} = false;
        
        private Messages mensajes = new Messages(); 
        public Client(String ip, int port){
            host = Dns.GetHostEntry(ip);
            address = host.AddressList[0];
            endPoint = new IPEndPoint(address, port);

            s_Client = new Socket(address.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            
        }

        public async Task Start()
        {
            try
            {
                await s_Client.ConnectAsync(endPoint);
                Console.WriteLine("Te conectaste al servidor");

                _ = Task.Run(() => Receive()); 
                await NameUser();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
            }
        }

        public async Task NameUser()
        {
            Console.Write("Registre un nombre de usuario(8 caracteres): ");
            userName = Console.ReadLine();
            await IdentifyInServer();
        }

        public async Task IdentifyInServer()//switch para enviar?
        {
            Messages.Identify identificador = new Messages.Identify(messageType.IDENTIFY,userName!);
            string json = mensajes.JSONToString(identificador);
            await Send(json);
        }
        public async Task Send(string msg){
            try
            {
                byte[] byteMsg = Encoding.UTF8.GetBytes(msg);
                await s_Client.SendAsync(byteMsg);
                Console.WriteLine("Mensaje enviado: " + msg);
            }
            catch(SocketException ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
            
        }
        public async Task Receive(){
            
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int receivedBytes = await s_Client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                    if (receivedBytes > 0)
                    {
                        string jsonMessage = Encoding.UTF8.GetString(buffer, 0, receivedBytes);
                        Console.WriteLine($"Received message from server: {jsonMessage}"); 
                        HandleMessage(jsonMessage);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la recepción: {ex.Message}");
            }
        }

        private void HandleMessage(string jsonMessage)//switch for all messages
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
                        HandleIdentifyMessage(jsonMessage);
                        break;
                    case messageType.NEW_USER:
                        HandleNewUserMessage(jsonMessage);
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

        private void HandleIdentifyMessage(string jsonMessage)
        {
            Messages.Identify? response = Messages.StringToJSON<Messages.Identify>(jsonMessage);

            if (response.type == messageType.RESPONSE && response.operation == messageType.IDENTIFY)
            {
                if (response.result == "SUCCESS")
                {
                    Console.WriteLine($"Identificación exitosa. Bienvenido {response.extra}");
                    isIdentified = true;
                }
                else if (response.result == "USER_ALREADY_EXISTS")
                {
                    Console.WriteLine($"Error: El nombre de usuario {response.extra} ya está en uso.");
                    isIdentified = false;
                }
            }
            else
            {
                Console.WriteLine("Mensaje desconocido recibido");
            }
        }

        private void HandleNewUserMessage(string jsonMessage)
        {
            Messages.Identify? response = Messages.StringToJSON<Messages.Identify>(jsonMessage);
            if (response.type == messageType.NEW_USER)
            {
                Console.WriteLine($"{response.username} se ha unido al chat");
            }

        }

        public async Task Disconnect(){
            Messages.Disconnect desconexion= new Messages.Disconnect(messageType.DISCONNECTED,userName!);
            string json = mensajes.JSONToString(desconexion);
            await Send(json);
        } 

        public async void RecognizeCommand(string msg){
            switch(msg)
            {
                case "*exit":
                    await Disconnect();
                    break;

                default:
                    break;
            }
        }
    }
    
}