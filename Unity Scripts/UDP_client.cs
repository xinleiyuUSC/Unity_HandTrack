using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;
using System.Threading.Tasks;
using TMPro; // To use TextMeshPro

public class UDPClient : MonoBehaviour
{
    public static UDPClient Instance { get; private set; }

    private string serverIP = "192.168.86.23";
    private int serverPort = 7777;
    private IPEndPoint serverEndPoint;
    private UdpClient client;

    public TMP_Text latencyText; // For displaying latency in a TextMeshPro object

    private DateTime lastSendTime;
    private bool awaitingResponse = false;
    private double latencyToUpdate = -1; // New field to hold latency value

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            client = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
            StartListening();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void StartListening()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    UdpReceiveResult result = await client.ReceiveAsync();
                    string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                    Debug.Log("Message received from the server: " + receivedMessage);
                    if (awaitingResponse)
                    {
                        TimeSpan latency = DateTime.UtcNow - lastSendTime;
                        Debug.Log($"Round-trip latency: {latency.TotalMilliseconds} ms");
                        // Update the shared latency value
                        latencyToUpdate = latency.TotalMilliseconds;
                        awaitingResponse = false;
                    }
                }
                catch (Exception err)
                {
                    Debug.LogError(err.ToString());
                }
            }
        });
    }

    void Update()
    {
        // Check if there's a new latency value to update
        if (latencyToUpdate >= 0)
        {
            // Update the TextMeshPro text and reset the latency value
            latencyText.text = $"Latency: {latencyToUpdate:F2} ms";
            latencyToUpdate = -1; // Reset the latency value
        }
    }

    public void Send(string message)
    {
        try
        {
            byte[] bytesToSend = Encoding.UTF8.GetBytes(message);
            client.Send(bytesToSend, bytesToSend.Length, serverEndPoint);
            lastSendTime = DateTime.UtcNow;
            awaitingResponse = true;
            Debug.Log("Message sent to the server: " + message);
        }
        catch (Exception err)
        {
            Debug.LogError(err.ToString());
        }
    }

    void OnApplicationQuit()
    {
        if (client != null)
        {
            client.Close();
        }
    }
}
