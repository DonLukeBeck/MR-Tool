using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using System.IO;
using Newtonsoft.Json;
using System.Text;

/*
 *              Whisper Inference Code
 *              ======================
 *  
 *  Go to https://huggingface.co/unity/sentis-whisper-tiny/tree/main and download audio-decoder-tiny.sentis, audio-encoder-tiny.sentis,
 *                                                                                logmel-spectro.sentis and vocab.json
 *  
 *  In Assets/StreamingAssets put:
 *  
 *  AudioDecoder_Tiny.sentis
 *  AudioEncoder_Tiny.sentis
 *  LogMelSepctro.sentis
 *  vocab.json
 * 
 *  
 * 
 *  Install package com.unity.nuget.newtonsoft-json from packagemanger
 *  Install package com.unity.sentis
 * 
 */


public class RunWhisper : MonoBehaviour
{
    IWorker decoderEngine, encoderEngine, spectroEngine;

    const BackendType backend = BackendType.GPUCompute;

    // Link your audioclip here. Format must be 16Hz mono non-compressed.
    public AudioClip audioClip;

    [SerializeField]
    GoalManager goalManager;

    // This is how many tokens you want. It can be adjusted.
    const int maxTokens = 100;

    //Special tokens see added tokens file for details
    const int END_OF_TEXT = 50257;
    const int START_OF_TRANSCRIPT = 50258;
    const int ENGLISH = 50259;
    const int GERMAN = 50261;
    const int FRENCH = 50265;  
    const int TRANSCRIBE = 50359; //for speech-to-text in specified language
    const int TRANSLATE = 50358;  //for speech-to-text then translate to English
    const int NO_TIME_STAMPS = 50363; 
    const int START_TIME = 50364;

    int numSamples;
    float[] data;
    string[] tokens;

    int currentToken = 0;
    int[] outputTokens = new int[maxTokens];

    // Used for special character decoding
    int[] whiteSpaceCharacters = new int[256];

    TensorFloat encodedAudio;

    bool transcribe = false;
    public string outputString = "";

    // Maximum size of audioClip (30s at 16kHz)
    const int maxSamples = 30 * 16000;

    void Start()
    {
        SetupWhiteSpaceShifts();

        string streamingAssetsPath = Application.streamingAssetsPath;
        string persistentDataPath = Application.persistentDataPath;

        string[] modelFiles = new string[]
        {
            "vocab.json",
            "AudioDecoder_Tiny.sentis",
            "AudioEncoder_Tiny.sentis",
            "LogMelSepctro.sentis"
        };

        foreach (var file in modelFiles)
        {
            string sourcePath = Path.Combine(streamingAssetsPath, file);
            string destinationPath = Path.Combine(persistentDataPath, file);
            if (!File.Exists(destinationPath))
            {
                // Copy file from StreamingAssets to persistentDataPath
                if (Application.platform == RuntimePlatform.Android)
                {
                    // On Android, use WWW or UnityWebRequest to read from StreamingAssets
                    using (WWW www = new WWW(sourcePath))
                    {
                        while (!www.isDone) { }
                        if (string.IsNullOrEmpty(www.error))
                        {
                            File.WriteAllBytes(destinationPath, www.bytes);
                        }
                        else
                        {
                            Debug.LogError("Failed to load file from StreamingAssets: " + www.error);
                        }
                    }
                }
                else
                {
                    // On other platforms, simply copy the file
                    File.Copy(sourcePath, destinationPath);
                }
            }
        }

        #if UNITY_EDITOR
                string selectedPath = Application.streamingAssetsPath;
        #elif UNITY_ANDROID
                string selectedPath = Application.persistentDataPath;
        #else
                string selectedPath = Application.streamingAssetsPath; // Default to streamingAssetsPath for other platforms
        #endif

        var jsonText = File.ReadAllText(Path.Combine(selectedPath, "vocab.json"));
        var vocab = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(jsonText);
        tokens = new string[vocab.Count];
        foreach (var item in vocab)
        {
            tokens[item.Value] = item.Key;
        }

        // Load the models from persistentDataPath
        Model decoder = ModelLoader.Load(Path.Combine(selectedPath, "AudioDecoder_Tiny.sentis"));
        Model decoderWithArgMax = Functional.Compile(
            (tokens, audio) => Functional.ArgMax(decoder.Forward(tokens, audio)[0], 2),
            (decoder.inputs[0], decoder.inputs[1])
        );

        Model encoder = ModelLoader.Load(Path.Combine(selectedPath, "AudioEncoder_Tiny.sentis"));
        Model spectro = ModelLoader.Load(Path.Combine(selectedPath, "LogMelSepctro.sentis"));

        decoderEngine = WorkerFactory.CreateWorker(backend, decoderWithArgMax);
        encoderEngine = WorkerFactory.CreateWorker(backend, encoder);
        spectroEngine = WorkerFactory.CreateWorker(backend, spectro);
    }

    void LoadAudio()
    {
        if(audioClip.frequency != 16000)
        {
            Debug.Log($"The audio clip should have frequency 16kHz. It has frequency {audioClip.frequency / 1000f}kHz");
            return;
        }

        numSamples = audioClip.samples;

        if (numSamples > maxSamples)
        {
            Debug.Log($"The AudioClip is too long. It must be less than 30 seconds. This clip is {numSamples/ audioClip.frequency} seconds.");
            return;
        }

        data = new float[maxSamples];
        numSamples = maxSamples;
        //We will get a warning here if data.length is larger than audio length but that is OK
        audioClip.GetData(data, 0);
    }

    public void Transcribe()
    {
        // Reset output tokens
        outputTokens[0] = START_OF_TRANSCRIPT;
        outputTokens[1] = ENGLISH;
        outputTokens[2] = TRANSCRIBE;
        outputTokens[3] = START_TIME;
        currentToken = 3;

        // Reset output string (transcript)
        outputString = "";

        // Load audio and encode it
        LoadAudio();
        EncodeAudio();
        transcribe = true;
    }

    void EncodeAudio()
    {
        using var input = new TensorFloat(new TensorShape(1, numSamples), data);

        spectroEngine.Execute(input);
        var spectroOutput = spectroEngine.PeekOutput() as TensorFloat;

        encoderEngine.Execute(spectroOutput);
        encodedAudio = encoderEngine.PeekOutput() as TensorFloat;
    }


    // Update is called once per frame
    void Update()
    {
        if (transcribe && currentToken < outputTokens.Length - 1)
        {
            using var tokensSoFar = new TensorInt(new TensorShape(1, outputTokens.Length), outputTokens);

            var inputs = new Dictionary<string, Tensor>
            {
                {"input_0", tokensSoFar },
                {"input_1", encodedAudio }
            };

            decoderEngine.Execute(inputs);
            var tokensPredictions = decoderEngine.PeekOutput() as TensorInt;

            tokensPredictions.CompleteOperationsAndDownload();

            int ID = tokensPredictions[currentToken];

            outputTokens[++currentToken] = ID;

            if (ID == END_OF_TEXT)
            {
                transcribe = false;
            }
            else if (ID >= tokens.Length)
            {
                outputString += $"(time={(ID - START_TIME) * 0.02f})";
            }
            else outputString += GetUnicodeText(tokens[ID]);

            if (!transcribe) { 
                goalManager.outputString = outputString;
            }
        }
    }

    // Translates encoded special characters to Unicode
    string GetUnicodeText(string text)
    {
        var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(ShiftCharacterDown(text));
        return Encoding.UTF8.GetString(bytes);
    }

    string ShiftCharacterDown(string text)
    {
        string outText = "";
        foreach (char letter in text)
        {
            outText += ((int)letter <= 256) ? letter :
                (char)whiteSpaceCharacters[(int)(letter - 256)];
        }
        return outText;
    }

    void SetupWhiteSpaceShifts()
    {
        for (int i = 0, n = 0; i < 256; i++)
        {
            if (IsWhiteSpace((char)i)) whiteSpaceCharacters[n++] = i;
        }
    }

    bool IsWhiteSpace(char c)
    {
        return !(('!' <= c && c <= '~') || ('¡' <= c && c <= '¬') || ('®' <= c && c <= 'ÿ'));
    }

    //private void OnApplicationQuit()
    //{
    //    if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    //}

    private void OnDestroy()
    {
        decoderEngine?.Dispose();
        encoderEngine?.Dispose();
        spectroEngine?.Dispose();
    }
}
