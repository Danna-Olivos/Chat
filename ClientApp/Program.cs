using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApp
{
    class Programa
    {
        public static async Task Main(string[]args){
            String? ip;
            int port; 
            Console.Write("Por favor ingrese una direccion IPv4: ");
            ip = Console.ReadLine();

            Console.Write("Ingrese un puerto: ");
            string? portInput = Console.ReadLine();
            while (!int.TryParse(portInput, out port))
            {
                Console.WriteLine("Por favor, ingrese un numero valido para el puerto.");
                portInput = Console.ReadLine();
            }

            Client c = new Client(ip!,port);
            String? msg;
            await c.Start();
            while(true){
                Console.WriteLine("Escriba: ");
                msg = Console.ReadLine();
                // _ = c.Send(msg!);
                // if(msg == "exit"){
                //     c.Disconnect();
                //     break; 
                // }
                c.RecognizeCommand(msg!);
            }
        }
    }
}