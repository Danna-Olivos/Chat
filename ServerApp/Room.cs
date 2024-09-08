using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Server
{
    class Room
    {
        private String name;
        static ConcurrentDictionary<string, Socket> clientesConectados = new ConcurrentDictionary<string, Socket>();
        public Room(String name){
            this.name = name;
        }

        public String sendMsg()
        {
            return "hi";
        }
    }
}