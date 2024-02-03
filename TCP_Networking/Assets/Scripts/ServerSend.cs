using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerSend
{
    //Packets
    public static void Welcome(int toClient, string message)
    {
        using (Packet packet = new Packet((int)ServerPackets.welcome)) // for disposing
        {
            packet.Write(message);
            packet.Write(toClient);
            SendTCPData(toClient, packet);
        }
    }

    public static void SendMessage(string message)
    {
        using (Packet packet = new Packet((int)ServerPackets.message)) // for disposing
        {
            packet.Write(message);
            SendToAllClients(packet);
        }
    }

    public static void SpawnCharacter(int exceptClient, Vector3 pos, Quaternion rot)
    {
        using (Packet packet = new Packet((int)ServerPackets.spawnCharacter)) // for disposing
        {
            packet.Write(pos);
            packet.Write(rot);
            SendToAllClients(exceptClient, packet);
        }
    }

    //preparing the packet to be sent
    private static void SendTCPData(int toClient, Packet packet)
    {
        //take the length of the byte list that we want to send
        // and insert it in the beginning of the packet
        packet.WriteLength();
        Server.clients[toClient].tcp.SendData(packet);
    }

    private static void SendToAllClients(Packet packet)
    {
        packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            Server.clients[i].tcp.SendData(packet);
        }
    }

    private static void SendToAllClients(int exceptClient, Packet packet)
    {
        packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            if (i != exceptClient)
            {
                Server.clients[i].tcp.SendData(packet);
            }
        }
        Debug.Log($"Successfully sent data except {exceptClient}");
    }
}
