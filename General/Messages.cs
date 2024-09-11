using System;
using System.Collections.Generic;
using System.Text.Json;

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
        //serializar
        public string JSONToString<T>(T obj) where T : class
        {
            var options = new JsonSerializerOptions{WriteIndented = true};
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

        //las demas 
        
    }
}