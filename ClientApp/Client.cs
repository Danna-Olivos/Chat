using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Client{
        private IPHostEntry host;
        private IPAddress address;
        private IPEndPoint endPoint;

        private Socket s_Client;
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
            byte[] byteMsg = Encoding.ASCII.GetBytes(msg);
            s_Client.Send(byteMsg);
            Console.WriteLine("Mensaje enviado");
        }

        public void Disconnect(){

             try
            {
                // Shutdown the socket to stop sending and receiving data
                s_Client.Shutdown(SocketShutdown.Both);

                // Close the socket
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