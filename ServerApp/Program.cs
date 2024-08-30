using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Programa
    {
        public static void Main(string[]args){
            Server s = new Server("localhost", 4404);
            s.Start();
            Console.ReadKey();
        }
    }
}