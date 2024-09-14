using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using General;
using System.Runtime.CompilerServices;

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

                _ = Task.Run(() => Receive()); 
                if(!isIdentified){
                    _ = Task.Run(()=> Connect());
                }
                await NameUser();
                status = "ACTIVE";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error de conexión: {ex.Message}");
            }
        }

        public async Task Connect()
        {
            String? msg;

            while(true){
                Console.WriteLine($"{userName}: ");
                msg = Console.ReadLine();
                await RecognizeCommand(msg!);
            }

        }

        public async Task NameUser()
        {
            Console.Write("Registre un nombre de usuario(8 caracteres): ");
            userName = Console.ReadLine();
            await IdentifyInServer();
        }

        public async Task IdentifyInServer()
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
                    case messageType.RESPONSE:
                        HandleResponseMessage(jsonMessage);
                        break;
                    case messageType.NEW_USER:
                        HandleNewUserMessage(jsonMessage);
                        break;
                    case messageType.NEW_STATUS:
                        HandleNewStatusMessage(jsonMessage);
                        break;
                    case messageType.USER_LIST:
                        break;
                    case messageType.TEXT_FROM:
                        HandlePrivateText(jsonMessage);
                        break;
                    case messageType.PUBLIC_TEXT_FROM:
                        HandlePublicText(jsonMessage);
                        break;
                    case messageType.JOIN_ROOM:
                        break;
                    case messageType.ROOM_USERS_LIST:
                        break;
                    case messageType.ROOM_TEXT_FROM:
                        break;
                    case messageType.LEFT_ROOM:
                        break;
                    case messageType.DISCONNECTED:
                        HandleDisconnectedMessage(jsonMessage);
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

        private void HandlePrivateText(string jsonMessage)
        {
            Messages.Text? response = Messages.StringToJSON<Messages.Text>(jsonMessage);
            string text = response.text!;
            if (status != null && response.type == messageType.TEXT_FROM)
            {
                Console.WriteLine($"Mensaje privado de {response.username}: {text}");
            }
        }

        private void HandlePublicText(string jsonMessage)
        {
            Messages.Text? response = Messages.StringToJSON<Messages.Text>(jsonMessage);
            string text = response.text!;
            if (status != null && response.type == messageType.PUBLIC_TEXT_FROM)
            {
                Console.WriteLine($"{response.username}: {text}");
            }
        }

        private void HandleNewStatusMessage(string jsonMessage)
        {
            Messages.Status? response = Messages.StringToJSON<Messages.Status>(jsonMessage);
            status = response.status;
            if (status != null && response.type == messageType.NEW_STATUS)
            {
                Console.WriteLine($"{response.username} ha cambiado su estado a {response.status}");
            }
        }

        private void HandleDisconnectedMessage(string jsonMessage)
        {
            Messages.Disconnect? response = Messages.StringToJSON<Messages.Disconnect>(jsonMessage);
            if (response != null && response.type == messageType.DISCONNECTED)
            {
                Console.WriteLine($"{response.username} se ha desconectado");
            }
        }

        private async void HandleResponseMessage(string jsonMessage)
        {
            Messages.Identify? response = Messages.StringToJSON<Messages.Identify>(jsonMessage);

            if (response != null && response.type == messageType.RESPONSE && response.operation == messageType.IDENTIFY)
            {
                if (response.result == "SUCCESS")
                {
                    Console.WriteLine($"Identificación exitosa. Bienvenido {response.extra}. Presione enter para comenzar");
                    isIdentified = true;
                }
                else if (response.result == "USER_ALREADY_EXISTS")
                {
                    Console.WriteLine($"Error: El nombre de usuario {response.extra} ya está en uso. Ingrese otro nombre.");
                    userName = null;
                    isIdentified = false;
                    await NameUser();
                }
            }
            else if(response!.operation == messageType.INVALID && response.result == "NOT_IDENTIFIED")
            {
                Console.WriteLine($"Error: El nombre de usuario {response.extra} es muy largo. Ingrese un nombre de 8 caracteres.");
                userName = null;
                isIdentified = false;
                await NameUser();
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
                Console.WriteLine($"{response.username} se unio al chat");
            }
        }

        public async Task Disconnect(){
            Messages.Disconnect desconexion= new Messages.Disconnect(messageType.DISCONNECT);
            string json = mensajes.JSONToString(desconexion);
            await Send(json);
        } 

        public async Task Status(string estado){
            Messages.Status estatus = new Messages.Status(messageType.STATUS,estado);
            string json = mensajes.JSONToString(estatus);
            await Send(json);
        }

        public async Task PublicText(string msg){
            Messages.Text mensajeP = new Messages.Text(messageType.PUBLIC_TEXT, msg);
            string json = mensajes.JSONToString(mensajeP);
            await Send(json);
        }

        public async Task PrivateText(string username,string msg)
        {
            Messages.Text mensajePriv = new Messages.Text(messageType.TEXT,username,msg);
            string json = mensajes.JSONToString(mensajePriv);
            await Send(json);
        }

        public async Task RecognizeCommand(string msg){
            var (command, input,input2)= ParseCommand(msg);
            switch(command) 
            {
                case "*exit*":
                    await Disconnect();
                    break;
                case "*leaveRoom*":
                    break;
                case "*sendMessageToRoom*":
                   
                    break;
                case "*listOfRoomUsers*":
                    
                    break;
                case "*joinRoom*":
                    
                    break;
                case "*inviteToRoom*":
                    
                    break;
                case "*makeRoom*":
                   
                    break;
                case "*sendMessage*":
                    await PublicText(input);
                    break;
                case "*sendPrivateMessage*":
                    await PrivateText(input,input2);
                    break;
                case "*users*":
                    
                    break;
                case "*status*":
                    await Status(input);
                    break;

                default:
                //haz ingresado mal el comando, imprimirle en pantalla la lista de comandos 
                    break;
            }
        }

        // private (string command, string parameter) ParseCommand(string userInput)
        // {
        //     userInput = userInput.Trim();

        //     int spaceIndex = userInput.IndexOf(' ');

        //     if (spaceIndex == -1)
        //     {
        //         return (userInput, string.Empty);
        //     }

        //     string command = userInput.Substring(0, spaceIndex).Trim();
        //     string parameter = userInput.Substring(spaceIndex + 1).Trim();

        //     return (command, parameter);
        // }

        private (string command, string parameter1, string parameter2) ParseCommand(string userInput)
        {
            userInput = userInput.Trim();
            int firstSpaceIndex = userInput.IndexOf(' ');

            if (firstSpaceIndex == -1)
            {
                return (userInput, string.Empty, string.Empty); 
            }

            string command = userInput.Substring(0, firstSpaceIndex).Trim();
            string remaining = userInput.Substring(firstSpaceIndex + 1).Trim();

            int secondSpaceIndex = remaining.IndexOf("/");

            if (secondSpaceIndex == -1)
            {
                return (command, remaining, string.Empty);
            }

            string parameter1 = remaining.Substring(0, secondSpaceIndex).Trim();
            string parameter2 = remaining.Substring(secondSpaceIndex + 1).Trim();

            return (command, parameter1, parameter2);
        }


    }
    
}