using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using CHAT;


namespace Client
{
    class Client{
        private IPHostEntry host;
        private IPAddress address;
        private IPEndPoint endPoint;

        private Socket s_Client;

        String? userName{get; set;}
        String? status{get; set;}
        
        private Messages mensajes = new Messages(); 
        public Client(String ip, int port){
            host = Dns.GetHostEntry(ip);
            address = host.AddressList[0];
            endPoint = new IPEndPoint(address, port);

            s_Client = new Socket(address.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            
        }

        public async void Start(){
            await s_Client.ConnectAsync(endPoint);
        }

        public void NameUser()
        {
            Console.Write("Registre un nombre de usuario: ");
            userName = Console.ReadLine();
            IdentifyInServer(messageType.IDENTIFY, userName);
        }

        public void IdentifyInServer(messageType type, string userName)
        {
            Dictionary<string, object> jsonType = mensajes.IdentifyUser(type, userName);
            byte [] json = mensajes.JSONToByte(jsonType); 
            SendBytes(json); 
        }

        public async void Send(String msg){
            byte[] byteMsg = Encoding.UTF8.GetBytes(msg);
            await s_Client.SendAsync(byteMsg);
            Console.WriteLine("Mensaje enviado");
        }

        public async void SendBytes(byte[] msg){
            await s_Client.SendAsync(msg);
            Console.WriteLine("Mensaje enviado");
        }

        public async void Receive(){
            byte[]buffer = new byte[1024];
            await s_Client.ReceiveAsync(buffer);
            //agregar nameuser y estado 
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