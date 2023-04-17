using UnityEngine;
using System;
using System.Net.Sockets;
using Unity.VisualScripting;
using static GlobalMethods;

public class Client: MonoBehaviour
{
    #region Implement Singleton
    public static Client Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            // Destroy any additional instances of the singleton
            Destroy(gameObject);
        }
        else
        {
            // Make sure the instance is not destroyed when loading a new scene
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
    }
    #endregion

    //Local variable
    string latestMsg = "";
    public string LatestMsg { get => latestMsg; private set => latestMsg = value; }

    //Events
    public delegate void ClientEvent();
    public ClientEvent OnReceiveMessage;



    // Replace with the IP address of the server you want to connect to
    public string ipAddress = "127.0.0.1";

    // Replace with the port number of the server you want to connect to
    public int port = 8080;

    private TcpClient client;

    void Start()
    {
        try
        {
            // Create a new TcpClient object and connect to the server
            client = new TcpClient(ipAddress, port);
            Debug.Log("Connected to server");

            // Start a new thread to read incoming messages from the server
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ThreadStart(ReceiveTCPMessages));
            thread.Start();
        }
        catch (Exception e)
        {
            Debug.Log("Error connecting to server: " + e.Message);
        }
    }

    void OnDestroy()
    {
        if (client != null)
        {
            // Close the TcpClient when the application is closed
            client.Close();
        }
    }

    void ReceiveTCPMessages()
    {
        try
        {
            // Get the NetworkStream object associated with the TcpClient
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            string message = "";

            while (true)
            {
                // Read incoming messages from the server
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                message += System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Process the message
                if (message.EndsWith("\n"))
                {
                    string processedMsg = message.TrimEnd();
                    Debug.Log("Received message from server: " + processedMsg);
                    LatestMsg = processedMsg;
                    message = "";

                    if (OnReceiveMessage != null)
                        OnReceiveMessage.Invoke();
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error receiving messages: " + e.Message);
        }
    }

    public void SendTCPMessage(string message)
    {
        try
        {
            // Get the NetworkStream object associated with the TcpClient
            NetworkStream stream = client.GetStream();

            // Convert the message to a byte array and send it to the server
            byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
            Debug.Log("Sent message to server: " + message);
        }
        catch (Exception e)
        {
            Debug.Log("Error sending message: " + e.Message);
        }
    }
}
