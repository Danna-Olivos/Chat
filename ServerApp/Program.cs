using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerApp
{
    class Programa
    {
        public static async Task Main(string[]args){
            Server s = new Server("localhost", 4404);
            await s.Start();
            Console.ReadKey();
        }
    }
}