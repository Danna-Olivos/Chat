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
            c.Start();
            c.Send("Hole pinole");
            Console.ReadKey();
        }
    }
}