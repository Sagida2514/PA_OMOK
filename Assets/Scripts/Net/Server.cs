using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    private static Server instance;

    public static Server Instance
    {
        get { return instance; }
    }

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeppAlive;

    public Action connectionDropped;

    public void Init(ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endPoint = NetworkEndPoint.AnyIpv4;
        endPoint.Port = port;

        if (driver.Bind(endPoint) != 0)
        {
            Debug.Log("바인드 실패함 " + endPoint.Port);
            return;
        }
        else
        {
            driver.Listen();
            Debug.Log("현재 해당 포트 Listen " + endPoint.Port);
        }

        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }

    public void Shutdown()
    {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }

    public void OnDestroy()
    {
        Shutdown();
    }


    public void Update()
    {
        if (!isActive)
            return;

        KeepAlive();

        driver.ScheduleUpdate().Complete();
        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }


    private void KeepAlive()
    {
        if (Time.time - lastKeppAlive > keepAliveTickRate)
        {
            lastKeppAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }


    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated == false)
            {
                connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }


    private void AcceptNewConnections()
    {
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
        }
    }


    private void UpdateMessagePump()
    {
        DataStreamReader stream;

        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;

            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    connections[i] = default(NetworkConnection);
                    Shutdown(); // 2인용 게임이라서 한명 연결 끊으면 셧다운
                }
            }
        }
    }


    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }


    public void Broadcast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated)
            {
                SendToClient(connections[i], msg);
            }
        }
    }
}