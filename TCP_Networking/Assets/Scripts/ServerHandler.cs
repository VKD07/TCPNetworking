using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerHandler
{
    public static void WelcomeReceived(int fromClient, Packet packet)
    {
        int id = packet.ReadInt();
        string userName = packet.ReadString();

        Debug.Log($"{Server.clients[fromClient].tcp.client.Client.RemoteEndPoint} connected successfully and is now player {fromClient}");
    }

    public static void MessageReceived(int fromClient, Packet packet)
    {
        string message = packet.ReadString();
        //send to all clients
        ServerSend.SendMessage(message);
    }

    public static void CharacterSpawnReceived(int fromClient, Packet packet)
    {

        Vector3 spawnPos = packet.ReadVector3();
        Quaternion rot = packet.ReadQuaternion();

        //sending spawn info to all clients
        ServerSend.SpawnCharacter(fromClient, spawnPos, rot);
    }
}
