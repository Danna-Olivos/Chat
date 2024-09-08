namespace CHAT
{
    class Messages
    {
        enum messageType
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

        //switch para identificar el tipo de mensaje
        //metodos que se van a serializar a json
    }
}