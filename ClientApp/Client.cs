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
                // Intentar conectarse al servidor
                await s_Client.ConnectAsync(endPoint);
                Console.WriteLine("Te conectaste al servidor");

                _ = Task.Run(() => Receive());  
                //NameUser();
            }
            catch (Exception ex)
            {
            Console.WriteLine($"Error de conexión: {ex.Message}");
            }
        }

        public void NameUser()
        {
            Console.Write("Registre un nombre de usuario(8 caracteres): ");
            userName = Console.ReadLine();
            IdentifyInServer();
            Task.Delay(2000).Wait(); // mnot sure
        }

        public async void IdentifyInServer()
        {
            Messages.Identify identificador = new Messages.Identify(messageType.IDENTIFY,userName);
            string json = mensajes.JSONToString(identificador);
            await Send(json);
        }

        public async Task Send(String msg){
            byte[] byteMsg = Encoding.UTF8.GetBytes(msg);
            await s_Client.SendAsync(byteMsg);
            Console.WriteLine("Mensaje enviado" + msg);
        }

        // public async Task SendBytes(byte[] msg){
        //     await s_Client.SendAsync(msg);
        //     Console.WriteLine("Mensaje enviado");
        // }

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
                        HandleMessage(jsonMessage);
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la recepción: {ex.Message}");
            }
        }

        private void HandleMessage(string jsonMessage)
        {
            Messages.Identify? response = Messages.StringToJSON<Messages.Identify>(jsonMessage);

            if (response != null && response.type == messageType.RESPONSE && response.operation == messageType.IDENTIFY)
            {
                if (response.result == "SUCCES")
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

        public void Disconnect(){

             try
            {
                s_Client.Shutdown(SocketShutdown.Both);
                s_Client.Close();
            
                Console.WriteLine("Te haz desconectado.");
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
    
}