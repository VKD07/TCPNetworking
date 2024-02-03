using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Client
{
    public static int dataBufferSize = 4096;

    public int id;
    public Character character;
    public TCP tcp;

    public Client(int clientID)
    {
        this.id = clientID;
        tcp = new TCP(id);
    }
    private void Disconnect()
    {
        Debug.Log($"{tcp.client.Client.RemoteEndPoint} has disconnected");

        tcp.Disconnect();
    }

    public class TCP
    {
        public TcpClient client;
        private readonly int id;
        NetworkStream stream;
        private Packet receivedData;
        byte[] receiveBuffer;
        public TCP(int id)
        {
            this.id = id;
        }

        public void Connect(TcpClient client)
        {
            this.client = client;
            //setting the buffer size to send to network stream
            client.ReceiveBufferSize = dataBufferSize;
            client.SendBufferSize = dataBufferSize;

            //setting this stream to the client stream
            stream = client.GetStream();

            receivedData = new Packet();

            //setting the buffer size of the client network stream
            receiveBuffer = new byte[dataBufferSize];
            //beginning to read network stream
            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveDataCallBack, null);

            //Welcome messsage
            ServerSend.Welcome(id, "Welcome to the server!");
        }

        public void SendData(Packet packet)
        {
            try
            {
                if (client != null)
                {
                    stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception)
            {
                Debug.Log($"Error sending data to player {id} via TCP");
            }
        }

        private void ReceiveDataCallBack(IAsyncResult ar)
        {
            try
            {
                //getting the number of bytes that is received
                int byteLength = stream.EndRead(ar);
                // if no bytes receive then disconnect client
                if (byteLength <= 0)
                {
                    Server.clients[id].Disconnect();
                    return;
                }

                byte[] newData = new byte[byteLength];
                Array.Copy(receiveBuffer, newData, byteLength);

                //TODO: Handle data
                receivedData.Reset(HandleData(newData));
                //to continue reading the data
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveDataCallBack, null);
            }
            catch (Exception)
            {
                Debug.Log("Error receiving data");
                Server.clients[id].Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            receivedData.SetBytes(data);

            //this means we have the integer packet, the first data we wanted to send
            if (receivedData.UnreadLength() >= 4)
            {
                packetLength = receivedData.ReadInt();

                if (packetLength <= 0)
                {
                    return true;
                }
            }

            //that means that there are still remaining bytes that we want to read
            //other than the integer
            while (packetLength > 0 && packetLength <= receivedData.UnreadLength())
            {
                byte[] packetBytes = receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Server.packetHandlers[packetId](id, packet);
                    }
                });
                packetLength = 0;

                if (receivedData.UnreadLength() >= 4)
                {
                    packetLength = receivedData.ReadInt();

                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (packetLength <= 1)
            {
                return true;
            }

            return false;
        }

        public void Disconnect()
        {
            client.Close();
            stream = null;
            receivedData = null;
            receiveBuffer = null;
            client = null;
        }
    }
}
