# Chat Cliente/Servidor en C#
Proyecto 1 para modelado y programacion 2025-1
EL proyecto es un chat sincronico que se comunica mediante el envio de JSON.

## Requisitos Previos
Para hacer uso del proyecto, asegurate de contar con:
**.NET**: Herramienta para compilar y ejecutar aplicaciones en C#

## Uso del programa

1. **Compilar el proyecto**
   ```bash
   dotnet build
   
2. **Ejecutar el proyecto**
   Primero ejecutamos el servidor y despues el Cliente
   ```bash
   Servidor:
   dotnet run --project ServerApp
   Cliente:
   dotnet run --project ClientApp

   
3. **Comandos del Cliente**
    ```bash
    *status* ACTIVE - Para cambiar el estado del usuario a ACTIVE, AWAY,BUSY
    *users* - Para saber que usuarios estan conectados
    *sendMessage* mensaje - Para enviar un mensaje a todos los usuarios conectados
    *senPrivateMessage* usuario/ mensaje - Para enviar un mensaje privado al usuario elegido
    *leaveRoom* nombreSala - Para abandonar una sala 
    *sendMessageToRoom* mensaje- Para enviar un mensaje a todos los usuarios de la sala
    *listOfRoomUsers* - Para obtener una lista de los usuarios conectados a la sala
    *joinRoom* nombreSala - Para unirse a una sala
    *inviteToRoom* listaDeUsuarios/ nombreSala - Para invitar a una lista de usuarios a una sala
    *makeRoom* nombre - Para crear una sala
    *exit* - Para abandonar el programa 
