using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Collections;

public class ImageAssembler : MonoBehaviour
{
    [SerializeField] RawImage rawImage; // Material to hold the reconstructed image

    [SerializeField]
    UdpSocket WebSocket;

    private List<byte[]> imageChunks = new List<byte[]>();
    private int totalChunks = 0;
    private int receivedChunks = 0;

    public float duration = 5f; // Timer duration in seconds
    private float timeRemaining;
    string resend = "";

    void Start()
    {
        timeRemaining = duration;
        StartCoroutine(TimerCoroutine());
    }

    IEnumerator TimerCoroutine()
    {
        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        TimerEnded();
    }

    void TimerEnded()
    {
        resend = "Resend Image";
    }

    public string ProcessImageData(string imageData)
    {
        // Parse metadata
        if (imageData.StartsWith("ImageChunks"))
        {
            receivedChunks = 0;
            totalChunks = int.Parse(imageData.Split(' ')[1]);
            return "";
        }
        // Receive image data chunks
        if (imageData.StartsWith("Base64EncodedChunk"))
        {
            // Extract Base64 encoded image data
            string base64Data = imageData.Split(' ')[1];

            // Ensure length is valid for decoding
            if (base64Data.Length % 4 != 0)
            {
                // Add padding if necessary to make length a multiple of 4
                base64Data += new string('=', (4 - base64Data.Length % 4) % 4);
            }

            // Decode Base64 encoded image data to bytes
            byte[] chunkData = Convert.FromBase64String(base64Data);
            imageChunks.Add(chunkData);
            receivedChunks++;
        }
        // If all chunks received, assemble image
        if (totalChunks == 0)
        {
            return "Resend Image - Image Chunk";
        }
        else if (receivedChunks == totalChunks)
        {
            receivedChunks = 0;
            totalChunks = 0;
            Thread thread = new Thread(AssembleImage);
            thread.Start();
        }
        else if (resend != "") {
            resend = "";
            return "Resend Image - Timeout";
        }
        return "";

    }

    void AssembleImage()
    {

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            byte[] imageData = CombineChunks(imageChunks);
            imageChunks.Clear();
            // Decode the byte image data into a Texture2D
            Texture2D texture = new Texture2D(1920, 1080);
            texture.LoadImage(imageData); // Load the byte image data

            //find ui game object
            if (rawImage == null)
            {
                rawImage = GameObject.Find("RawImage").GetComponent<RawImage>();
            }

            //set image transparency
            if (rawImage.texture == null)
            {
                var tempColor = rawImage.color;
                tempColor.a = 255f;
                rawImage.color = tempColor;
            }

            //Debug.Log($"Image loaded from {imageData.Length} bytes of data");

            rawImage.texture = texture;

            //Debug.Log("Image reconstructed and displayed on Quad.");
        });
    }

    private byte[] CombineChunks(List<byte[]> chunks)
    {
        using (MemoryStream stream = new MemoryStream())
        {
            foreach (byte[] chunk in chunks)
            {
                stream.Write(chunk, 0, chunk.Length);
            }
            return stream.ToArray();
        }
    }
}
