// Filename: AudioStreamer.cs

using UnityEngine;
using WebSocketSharp;
using System.Collections.Generic;
using System.Threading;

public class AudioStreamer : MonoBehaviour
{
    // WebSocket connection to the Node.js server
    private WebSocket ws;

    // Microphone audio capture variables
    private AudioClip microphoneClip;
    private int lastSamplePosition = 0;
    private const int frequency = 16000; // Microphone recording frequency
    private const int bufferLengthInSeconds = 1; // Length of the microphone buffer in seconds
    private const int MIN_BUFFER_SIZE = 8192; // Minimum size before sending audio data

    // Buffer to accumulate audio data before sending
    private List<byte> accumulatedAudioBuffer = new List<byte>();

    // Queue to hold received audio data for playback
    private Queue<float[]> receivedAudioQueue = new Queue<float[]>();

    // AudioSource component for playing received audio
    private AudioSource audioSource;

    // Mutex for thread safety when accessing the received audio queue
    private Mutex audioQueueMutex = new Mutex();

    // Sample rate of the received audio from OpenAI
    private const int outputFrequency = 24000; // Adjust if necessary
    private const int channels = 1; // Mono audio

    // Unity Lifecycle Methods
    void Start()
    {
        // Initialize WebSocket connection
        ws = new WebSocket("ws://localhost:3000");

        // WebSocket event handlers
        ws.OnMessage += OnWebSocketMessage;
        ws.OnError += (sender, e) => Debug.LogError("WebSocket error: " + e.Message);
        ws.OnOpen += (sender, e) => Debug.Log("Connected to server.");
        ws.OnClose += (sender, e) => Debug.Log("Disconnected from server.");

        // Connect to the server
        ws.Connect();

        // Start capturing audio from the microphone
        StartMicrophoneCapture();

        // Add an AudioSource component for playback
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update()
    {
        // Capture and send microphone audio data
        byte[] audioData = GetAudioData();
        if (audioData.Length > 0)
        {
            accumulatedAudioBuffer.AddRange(audioData);
            if (accumulatedAudioBuffer.Count >= MIN_BUFFER_SIZE)
            {
                if (ws.IsAlive)
                {
                    ws.Send(accumulatedAudioBuffer.ToArray());
                    Debug.Log($"Sent audio data. Length: {accumulatedAudioBuffer.Count} bytes");
                    accumulatedAudioBuffer.Clear();
                }
                else
                {
                    Debug.LogWarning("WebSocket is not connected.");
                }
            }
        }

        // Play received audio data
        if (receivedAudioQueue.Count > 0 && !audioSource.isPlaying)
        {
            audioQueueMutex.WaitOne();
            float[] audioSamples = receivedAudioQueue.Dequeue();
            audioQueueMutex.ReleaseMutex();

            PlayReceivedAudio(audioSamples);
        }
    }

    void OnDestroy()
    {
        // Clean up resources
        if (ws != null && ws.IsAlive) ws.Close();
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }
    }

    // WebSocket message handler
    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
            // Received audio data from server
            Debug.Log("Received audio data from server.");

            // Convert PCM16 bytes to float samples
            float[] audioSamples = ConvertPCM16ToFloat(e.RawData);

            // Enqueue the audio samples for playback
            audioQueueMutex.WaitOne();
            receivedAudioQueue.Enqueue(audioSamples);
            audioQueueMutex.ReleaseMutex();
        }
        else
        {
            Debug.Log("Message from server: " + e.Data);
            // Handle text messages (e.g., transcripts) if necessary
        }
    }

    // Start capturing audio from the microphone
    private void StartMicrophoneCapture()
    {
        if (Microphone.devices.Length > 0)
        {
            microphoneClip = Microphone.Start(null, true, bufferLengthInSeconds, frequency);
            if (microphoneClip == null)
            {
                Debug.LogError("Failed to start microphone.");
            }
            else
            {
                Debug.Log("Microphone started.");
                // Wait until the microphone starts recording
                while (!(Microphone.GetPosition(null) > 0)) { }
                lastSamplePosition = Microphone.GetPosition(null);
            }
        }
        else
        {
            Debug.LogError("No microphone found.");
        }
    }

    // Get audio data from the microphone buffer
    private byte[] GetAudioData()
    {
        if (microphoneClip == null) return new byte[0];

        int currentSamplePosition = Microphone.GetPosition(null);
        if (currentSamplePosition < 0)
        {
            Debug.LogWarning("Microphone position is negative.");
            return new byte[0];
        }

        int sampleCount = 0;
        float[] audioSamples;

        if (currentSamplePosition == lastSamplePosition)
        {
            // No new data
            return new byte[0];
        }

        if (currentSamplePosition < lastSamplePosition)
        {
            // Handle wrap-around
            int samplesToEnd = microphoneClip.samples - lastSamplePosition;
            sampleCount = samplesToEnd + currentSamplePosition;
            audioSamples = new float[sampleCount];

            if (samplesToEnd > 0)
            {
                float[] endSamples = new float[samplesToEnd];
                microphoneClip.GetData(endSamples, lastSamplePosition);
                endSamples.CopyTo(audioSamples, 0);
            }

            if (currentSamplePosition > 0)
            {
                float[] startSamples = new float[currentSamplePosition];
                microphoneClip.GetData(startSamples, 0);
                startSamples.CopyTo(audioSamples, samplesToEnd);
            }
        }
        else
        {
            sampleCount = currentSamplePosition - lastSamplePosition;
            if (sampleCount <= 0) return new byte[0];

            audioSamples = new float[sampleCount];
            microphoneClip.GetData(audioSamples, lastSamplePosition);
        }

        lastSamplePosition = currentSamplePosition;
        return ConvertFloatToPCM16(audioSamples);
    }

    // Convert float samples to PCM16 bytes
    private byte[] ConvertFloatToPCM16(float[] audioSamples)
    {
        byte[] pcm16Bytes = new byte[audioSamples.Length * sizeof(short)];
        for (int i = 0; i < audioSamples.Length; i++)
        {
            float sample = Mathf.Clamp(audioSamples[i], -1f, 1f);
            short pcmSample = (short)(sample * short.MaxValue);
            pcm16Bytes[i * 2] = (byte)(pcmSample & 0xff);
            pcm16Bytes[i * 2 + 1] = (byte)((pcmSample >> 8) & 0xff);
        }
        return pcm16Bytes;
    }

    // Convert PCM16 bytes to float samples
    private float[] ConvertPCM16ToFloat(byte[] pcm16Data)
    {
        int sampleCount = pcm16Data.Length / 2;
        float[] floatData = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short pcmSample = (short)(pcm16Data[i * 2] | (pcm16Data[i * 2 + 1] << 8));
            floatData[i] = pcmSample / (float)short.MaxValue;
        }
        return floatData;
    }

    // Play received audio samples
    private void PlayReceivedAudio(float[] audioSamples)
    {
        if (audioSamples.Length == 0)
        {
            Debug.LogWarning("No audio samples to play.");
            return;
        }

        // Create a new AudioClip
        AudioClip clip = AudioClip.Create("ReceivedAudio", audioSamples.Length, channels, outputFrequency, false);
        clip.SetData(audioSamples, 0);

        // Play the clip using AudioSource
        audioSource.clip = clip;
        audioSource.Play();

        Debug.Log("Playing received audio.");
    }
}