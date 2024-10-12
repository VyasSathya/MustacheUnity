using UnityEngine;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

public class DialogueManager : MonoBehaviour
{
    private static readonly HttpClient client = new HttpClient();

    public string npcName;
    public string interactionType; // e.g., "greeting", "quest", "farewell"
    private AudioSource audioSource;

    private string apiUrl = "http://localhost:5000/get_response";

    void Start()
    {
        // Automatically find and assign the Audio Source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.LogWarning("AudioSource component was missing. Added a new AudioSource component to the GameObject.");
        }
    }

    public async void SendPlayerMessage(string playerMessage)
    {
        byte[] audioResponse = await GetNPCResponse(playerMessage);
        PlayAudioResponse(audioResponse);
    }

    private async Task<byte[]> GetNPCResponse(string playerMessage)
    {
        var payload = new
        {
            character = npcName,
            interaction = interactionType,
            message = playerMessage
        };

        string json = JsonUtility.ToJson(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            byte[] audioBytes = await response.Content.ReadAsByteArrayAsync();
            return audioBytes;
        }
        catch (HttpRequestException e)
        {
            Debug.LogError("Request error: " + e.Message);
            return null;
        }
    }

    public void PlayAudioResponse(byte[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
        {
            Debug.LogError("No audio data received or audio data is empty.");
            return;
        }

        // Create an AudioClip from the received audio data
        AudioClip clip = WavUtility.ToAudioClip(audioData, 0);
        if (clip != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("Failed to convert audio data to AudioClip.");
        }
    }
}
