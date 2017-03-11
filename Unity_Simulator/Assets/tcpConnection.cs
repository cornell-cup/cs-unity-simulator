using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System;
using System.Threading;


//Shared class with values that can be shared across threads
public class SharedData
{
    private readonly object _Lock = new object();
    public volatile string x = "";

    public string getX()
    {
        lock (_Lock)
        {
            return x;
        }
    }
    public void setX(string val)
    {
        lock (_Lock)
        {
            x = val;
        }
    }
}

public class MyBot
{
    public SharedData state;
    public bool isConnection, senddata1, senddata2, valuechanged, valuechanged2;
    public TcpListener server;
    public TcpClient client;
    public NetworkStream stream;
    public int x;
    //byte[] msg1;
    //string toSend;
    public String data, prevdata;
    public MyBot(SharedData data)
    {
        state = data;
    }
    void OnApplicationQuit()
    {
        server.Stop();
    }

    public void Update1()
    {
        server = null;
        try
        {

            // Set the TcpListener on port .
            Int32 port = 10000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");

            // TcpListener server = new TcpListener(port);
            server = new TcpListener(localAddr, port);

            // Start listening for client requests.
            server.Start();

            // Buffer for reading data
            Byte[] bytes = new Byte[1024];
            String data = null;

            // Enter the listening loop.
            while (true)
            {
                Thread.Sleep(10);

                Debug.Log("Waiting for a connection... ");

                // Perform a blocking call to accept requests.
                // You could also user server.AcceptSocket() here.
                client = server.AcceptTcpClient();
                Debug.Log("Waiting for a ... ");
                if (client != null)
                {

                    Debug.Log("Connected!");
                    //isConnection=true;
                    //client.Close();
                    //break;

                }
                data = "";

                // Get a stream object for reading and writing
                stream = client.GetStream();
                //StreamWriter swriter = new StreamWriter(stream);
                int i;

                // Loop to receive all the data sent by the client.
                while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    //msg1 = System.Text.Encoding.ASCII.GetBytes(prevdata);
                    // Send back a response.
                    //stream.Write(msg1, 0, msg1.Length);
                    // Translate data bytes to a ASCII string.
                    data += System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                    if (data.IndexOf(">>>>") > 0) {
                        tcpConnection.data.setX(data);
                        data = "";
                    }
                    //Debug.Log("Received:" + tcpConnection.data.getX());


                    //Debug.Log("Sent:"+ data);
                    // Process the data sent by the client.
                }
                // Shutdown and end connection
                client.Close();
            }
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException:" + e);
        }
        finally
        {
            // Stop listening for new clients.
            server.Stop();
        }

        //yield return null;

    }

}
public class tcpConnection : MonoBehaviour
{
    public Thread mThread;
    public static volatile SharedData data = new SharedData();
    //byte[] msg1;
    //string toSend;

    // Use this for initialization
    public void Start()
    {
        MyBot bot = new MyBot(data);
        ThreadStart tcpThreadS = new ThreadStart(bot.Update1);
        Thread tcpThread = new Thread(tcpThreadS);
        tcpThread.Start(); 
    }
    public string getTransform()
    {
        return tcpConnection.data.getX();
    }
    public void resetTransform()
    {
        tcpConnection.data.setX("");
    }
}