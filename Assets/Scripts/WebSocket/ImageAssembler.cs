using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

public class ImageAssembler
{
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
        Debug.Log("Assembling image");
        byte[] imageData = CombineChunks(imageChunks);
        Debug.Log(imageData.Length);
        Texture2D texture = new Texture2D(1, 1);
        texture.LoadImage(imageData); // Load the image data into the texture
        Debug.Log(CombineChunks(imageChunks).Length);
        // Save texture as PNG
        byte[] pngBytes = texture.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, "reconstructed_image.png");
        File.WriteAllBytes(filePath, pngBytes);

        Debug.Log($"Image reconstructed and saved to: {filePath}");
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
