using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace General
{

    public enum messageType
        {
            IDENTIFY,
            RESPONSE,
            NEW_USER,
            STATUS,
            NEW_STATUS,
            USERS,
            USER_LIST,
            TEXT,
            TEXT_FROM,
            PUBLIC_TEXT,
            PUBLIC_TEXT_FROM,
            NEW_ROOM,
            INVITE,
            INVITATION,
            INVALID,
            JOIN_ROOM,
            JOINED_ROOM,
            ROOM_USERS,
            ROOM_USERS_LIST,
            ROOM_TEXT,
            ROOM_TEXT_FROM,
            LEAVE_ROOM,
            LEFT_ROOM,
            DISCONNECT,
            DISCONNECTED,
        }
    public class Messages
    {
         public messageType? type{get;set;}
        public string? username{get;set;}
        public messageType? operation{get;set;}
        public string? result{get;set;}
        public string? extra{get;set;}
        public string? text{get;set;}

        public Messages(){}
        //serializar
        public string JSONToString<T>(T obj) where T : class
        {
            var options = new JsonSerializerOptions{WriteIndented = true,DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull};
            string jsonString = JsonSerializer.Serialize(obj,options);
            return jsonString; 
        }

        //deserializar
        public static T StringToJSON<T>(string json) where T :class
        {
            return JsonSerializer.Deserialize<T>(json)!;
        }
        public partial class Identify
        {
            public messageType? type{get;set;}
            public string? username{get;set;}
            public messageType? operation{get;set;}
            public string? result{get;set;}
            public string? extra{get;set;}

            public Identify(){}

            public Identify(messageType type, messageType operation, string result, string extra)
            {
                this.type = type;
                this.operation = operation;
                this.result = result;
                this.extra = extra; 
            }
            public Identify(messageType type, string username)
            {
                this.type = type;
                this.username = username;
                
            }
        }

        public partial class Disconnect
        {
            public messageType type{get;set;}
            public string? username{get;set;}
            public string? roomname{get;set;}

            public Disconnect(){}

            public Disconnect(messageType type, string roomname, string username){
                this.type = type;
                this.roomname = roomname;
                this.username = username;
            }

            public Disconnect(messageType type,string username){
                this.type = type;
                this.username = username;
            }
             public Disconnect(messageType type){
                this.type = type;
            }

        }
        
        public partial class Invalid
        {
            public messageType? type{get;set;}
            public messageType? operation{get;set;}
            public string? result{get;set;}

            public Invalid(messageType type, messageType operation, string result)
            {
                this.type = type;
                this.operation = operation;
                this.result = result;
            }
        }

        public partial class Users
        {
            public messageType? type{get;set;}
            public ConcurrentDictionary<string,string> clientesConectados {get;set;}

            public Users(){}

            public Users(messageType type)
            {
                this.type = type;
            }

            public Users(messageType type,ConcurrentDictionary<string,string> clientesConectados)
            {
                this.type = type;
                this.clientesConectados = clientesConectados;
            }
            
        }

        public partial class Status
        {
            public messageType? type{get;set;}
            public string? status{get;set;}
            public string? username{get;set;}

            public Status(){}

            public Status(messageType type, string status)
            {
                this.type = type;
                this.status = status;
            }

            public Status(messageType type, string username, string status)
            {
                this.type = type;
                this.status = status;
                this.username = username;
            }

        }

        public partial class Text
        {

            public messageType? type{get;set;}
            public string? username{get;set;}
            public messageType? operation{get;set;}
            public string? result{get;set;}
            public string? extra{get;set;}
            public string? text{get;set;}

            public Text(){}

            public Text(messageType type, string username, string text)
            {
                this.type = type;
                this.username = username;
                this.text = text;

            }
            public Text(messageType type, messageType operation, string result, string username)
            {
                this.type = type;
                this.operation = operation;
                this.result = result;
                this.extra = username;

            }

            public Text(messageType type, string text)
            {
                this.type = type;
                this.text = text;
            }

        }

        //las demas 
        
    }
}