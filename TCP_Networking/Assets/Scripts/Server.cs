using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;

public class Server
{
    public static int maxPlayers { get; set; }
    public static int port { get; set; }

    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();

    public delegate void PacketHandler(int fromClient, Packet packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

    public static TcpListener tcpListener;

    public static void Start(int _maxPlayers, int _port)
    {
        maxPlayers = _maxPlayers;
        port = _port;

        Debug.Log("Starting Server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();
        //starting to listen to client connections
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);

        Debug.Log($"Server Has started on {port}");
    }

    private static void ClientConnectCallback(IAsyncResult result)
    {
        //creating a new tcp client from the newly connected client
        TcpClient newClient = tcpListener.EndAcceptTcpClient(result);
        //Looping the connection to accept more connectiosn
        tcpListener.BeginAcceptTcpClient(new AsyncCallback(ClientConnectCallback), null);
        Debug.Log($"{newClient.Client.RemoteEndPoint} is trying to connect!");

        //assigning the connected client to the tcp in the clients list
        for (int i = 0; i <= maxPlayers; i++)
        {
            //if tcp client isnt assigned yet
            if (clients[i].tcp.client == null)
            {
                //connect the client
                clients[i].tcp.Connect(newClient);
                return;
            }
        }
        Debug.Log($"{newClient.Client.RemoteEndPoint} failed to connect: Server full");
    }

    //creating clients slot when server has started
    private static void InitializeServerData()
    {
        for (int i = 0; i <= maxPlayers; i++)
        {
            clients.Add(i, new Client(i));
        }

        packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandler.WelcomeReceived },
                 { (int)ClientPackets.spawnReceived, ServerHandler.CharacterSpawnReceived }

            };
        Debug.Log("Initialized Packets");
    }
}
