using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine.Android;
using TMPro;

public class UdpSocket : MonoBehaviour
{
    [HideInInspector] public bool isTxStarted = false;

    [SerializeField] string IP = "192.168.1.202"; // nginx server (replace with your own server that hosts the dialogue agent)
    [SerializeField] int rxPort = 8021; // port to receive data from Python on
    [SerializeField] int txPort = 8020; // port to send data to Python on

    [SerializeField] public TextMeshProUGUI m_ResponseText;

    // text-to-speech
    [SerializeField]
    RunJets runJets;

    // Create necessary UdpClient objects
    UdpClient client;
    IPEndPoint remoteEndPoint;
    Thread receiveThread; // Receiving Thread
    private ImageAssembler imageAssembler;

    public void SendData(string message) // Use to send data to Python
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    void Awake()
    {
        // Request user permissions
        Permission.RequestUserPermission("android.permission.WRITE_EXTERNAL_STORAGE");
        Permission.RequestUserPermission("android.permission.READ_EXTERNAL_STORAGE");
        Permission.RequestUserPermission("android.permission.CAMERA");
        Permission.RequestUserPermission("android.permission.INTERNET");
        Permission.RequestUserPermission("android.permission.RECORD_AUDIO");

        // Create remote endpoint 
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), txPort);

        // Create local client
        client = new UdpClient(rxPort);

        // local endpoint definition (where messages are received)
        // Create a new thread for reception of incoming messages
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        imageAssembler = new ImageAssembler();
    }

    // Receive data, update packets received
    private void ReceiveData()
    {
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string text = Encoding.UTF8.GetString(data);
                print(">> " + text);
                ProcessInput(text);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    private void ProcessInput(string input)
    {
        print(input);
        if (!isTxStarted) // First data arrived so tx started
        {
            isTxStarted = true;
        }

        //if data starts with "ImageChunks" or "Base64EncodedChunk" process image data
        if (input.StartsWith("ImageChunks") || input.StartsWith("Base64EncodedChunk"))
        {
            imageAssembler.ProcessImageData(input);
        }
        else if (input.StartsWith("Step"))
        {
            // Remove "Step x: " from the start of the string
            string text = input.Substring(8);
            //print("Step Received" + text);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                // Display the text in the UI
                m_ResponseText.transform.parent.gameObject.SetActive(false);
                m_ResponseText.text = text;
                runJets.inputText = text;
                runJets.TextToSpeech();
                m_ResponseText.transform.parent.gameObject.SetActive(true);
            });
        }
        else if (input.StartsWith("Answer: "))
        {
            string answer = input.Substring("Answer: ".Length);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                m_ResponseText.transform.parent.gameObject.SetActive(false);
                m_ResponseText.text = answer;
                runJets.inputText = answer;
                runJets.TextToSpeech();
                m_ResponseText.transform.parent.gameObject.SetActive(true);
            });
        }

    }

    //Prevent crashes - close clients and threads
    void OnDisable()
    {
        if (receiveThread != null)
            receiveThread.Abort();

        client.Close();
    }

}