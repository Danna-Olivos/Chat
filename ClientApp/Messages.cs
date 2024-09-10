using System;
using System.Collections.Generic;
using System.Text.Json;

namespace CHAT
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
        //switch para identificar el tipo de mensaje y crear json 
        public Dictionary<string, object> IdentifyUser(messageType type, string username)
        {
            Dictionary<string, object> jsonType = new Dictionary<string, object>();

            switch (type)
            {
                case messageType.IDENTIFY:
                    jsonType["type"] = "IDENTIFY";
                    jsonType["username"] = username;
                    break;

                case messageType.RESPONSE:
                    jsonType["type"] = "RESPONSE";
                    jsonType["operation"] = "IDENTIFY";
                    jsonType["result"] = "USER_ALREADY_EXISTS"; //hacer que este sea succes
                    jsonType["extra"] = username;
                    break;

                case messageType.NEW_USER:
                    jsonType["type"] = "NEW_USER";
                    jsonType["username"] = username;
                    break;

                default:
                    throw new ArgumentException("Invalid message type");
            }
            return jsonType; 
        }
        
        public Dictionary<string, object> InvalidOP(messageType type, string result)
        {
            Dictionary<string, object> jsonType = new Dictionary<string, object>();
            jsonType["type"] = "RESPONSE";
            jsonType["operation"]= "IDENTIFY";
            jsonType["result"]= result; 
            return jsonType; 
        }

         public Dictionary<string, object> DesconectUser(messageType type, string user)
        {
            Dictionary<string, object> jsonType = new Dictionary<string, object>();
            jsonType["type"] = "DISCONNECTED";
            jsonType["username"]= user;
            return jsonType; 
        }

        //serializar 
        public byte[] JSONToByte(Dictionary<string, object> jsonType)
        {
            var options = new JsonSerializerOptions {WriteIndented = true };
            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(jsonType,options);
            return jsonBytes; 
        }

        //deserializar
        public Dictionary<string, object> ByteToJSON(byte[] jsonBytes)
        {
            var readOnlySpan = new ReadOnlySpan<byte>(jsonBytes);
            Dictionary<string, object> jsonType = JsonSerializer.Deserialize<Dictionary<string,object>>(readOnlySpan)!;
            return jsonType; 
        }
        
        //metodos que se van a serializar a json
    }
}