using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;


namespace Client
{
    class Client{
        private IPHostEntry host;
        private IPAddress address;
        private IPEndPoint endPoint;

        private Socket s_Client;

        String userName; 
        String status;
        
        public Client(String ip, int port){
            host = Dns.GetHostEntry(ip);
            address = host.AddressList[0];
            endPoint = new IPEndPoint(address, port);

            s_Client = new Socket(address.AddressFamily,SocketType.Stream, ProtocolType.Tcp);
            
        }

        public void Start(){
            s_Client.Connect(endPoint);
        }

        public void Send(String msg){
            byte[] byteMsg = Encoding.UTF8.GetBytes(msg);
            s_Client.Send(byteMsg);
            Console.WriteLine("Mensaje enviado");
        }

        public void Receive(){
            byte[]buffer = new byte[1024];
            s_Client.Receive(buffer);
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