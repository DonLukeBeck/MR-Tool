using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

public class ImageAssembler : MonoBehaviour
{
    [SerializeField] Material quadMaterial; // Object to hold the reconstructed image

    private List<byte[]> imageChunks = new List<byte[]>();
    private int totalChunks = 0;
    private int receivedChunks = 0;

    public void ProcessImageData(string imageData)
    {
        // Parse metadata
        if (imageData.StartsWith("ImageChunks"))
        {
            totalChunks = int.Parse(imageData.Split(' ')[1]);
            return;
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

            try
            {
                // Decode Base64 encoded image data to bytes
                byte[] chunkData = Convert.FromBase64String(base64Data);
                imageChunks.Add(chunkData);
                receivedChunks++;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error decoding Base64 data: {e}");
            }
        }
        Debug.Log($"Received {receivedChunks} of {totalChunks} image chunks");
        // If all chunks received, assemble image
        if (receivedChunks == totalChunks)
        {
            Debug.Log("All image chunks received");
            AssembleImage();
        }
    }

    private void AssembleImage()
    {
        byte[] imageData = CombineChunks(imageChunks);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageData); // Load the image data into the texture

        // Create a new material and assign the texture to it
        Material material = new Material(Shader.Find("Standard"));
        material.mainTexture = texture;

        Debug.Log("Image reconstructed and displayed on Quad.");
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
