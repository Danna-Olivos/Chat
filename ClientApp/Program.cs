using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Programa
    {
        public static void Main(string[]args){
            Client c = new Client("localhost",4404);
            String msg;
            c.Start();
            while(true){
                Console.Write("Escriba: ");
                msg = Console.ReadLine();
                c.Send(msg);
                if(msg == "exit"){
                    c.Disconnect();
                    break; 
                }
            }
            //Console.ReadKey();
        }
    }
}