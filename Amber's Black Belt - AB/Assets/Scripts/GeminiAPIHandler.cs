using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GeminiRequest
{
    public List<Content> contents;
}

[System.Serializable]
public class Content
{
    public List<Part> parts;
}

[System.Serializable]
public class Part
{
    public string text;
}

[System.Serializable]
public class GeminiResponse
{
    public List<Candidate> candidates;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

public class GeminiAPIHandler : MonoBehaviour
{
    [SerializeField] private string apiKey = "AIzaSyAbccU_o38uDpVHMNtKn6oQx3YWRs-4F58"; // Set this in the inspector
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    
    public delegate void OnResponseReceived(string response);
    public delegate void OnError(string error);
    
    /// <summary>
    /// Generate AI response using Gemini API
    /// </summary>
    /// <param name="prompt">The text prompt to send to the AI</param>
    /// <param name="onSuccess">Callback when response is received</param>
    /// <param name="onError">Callback when an error occurs</param>
    public void GenerateResponse(string prompt, OnResponseReceived onSuccess, OnError onError)
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY")
        {
            onError?.Invoke("API Key not set. Please set your Gemini API key in the GeminiAPIHandler component.");
            return;
        }
        
        StartCoroutine(SendRequest(prompt, onSuccess, onError));
    }
    
    private IEnumerator SendRequest(string prompt, OnResponseReceived onSuccess, OnError onError)
    {
        // Create request data
        GeminiRequest request = new GeminiRequest
        {
            contents = new List<Content>
            {
                new Content
                {
                    parts = new List<Part>
                    {
                        new Part { text = prompt }
                    }
                }
            }
        };
        
        string jsonData = JsonUtility.ToJson(request);
        
        using (UnityWebRequest www = new UnityWebRequest(API_URL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            
            // Set headers
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("X-goog-api-key", apiKey);
            
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.ConnectionError || 
                www.result == UnityWebRequest.Result.ProtocolError)
            {
                onError?.Invoke($"API Error: {www.error}\nResponse: {www.downloadHandler.text}");
            }
            else
            {
                try
                {
                    GeminiResponse response = JsonUtility.FromJson<GeminiResponse>(www.downloadHandler.text);
                    
                    if (response.candidates != null && response.candidates.Count > 0 &&
                        response.candidates[0].content != null && 
                        response.candidates[0].content.parts != null &&
                        response.candidates[0].content.parts.Count > 0)
                    {
                        string aiResponse = response.candidates[0].content.parts[0].text;
                        onSuccess?.Invoke(aiResponse);
                    }
                    else
                    {
                        onError?.Invoke("Invalid response format from API");
                    }
                }
                catch (Exception e)
                {
                    onError?.Invoke($"Error parsing response: {e.Message}\nResponse: {www.downloadHandler.text}");
                }
            }
        }
    }
}
