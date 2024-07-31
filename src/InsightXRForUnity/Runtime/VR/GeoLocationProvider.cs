using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;

public class GeoLocationProvider : MonoBehaviour
{
    string apiUrl = "https://ipinfo.io/json";
    public event Action<GeolocationData> OnGeolocationFetched;

    void Start()
    {
        StartCoroutine(FetchGeolocation());
    }

    private IEnumerator FetchGeolocation()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to get geolocation data: " + request.error);
                OnGeolocationFetched?.Invoke(null); // Notify even if fetching failed
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                GeolocationData data = JsonUtility.FromJson<GeolocationData>(jsonResponse);
                OnGeolocationFetched?.Invoke(data); // Notify with the fetched data
            }
        }
    }
}

[Serializable]
public class GeolocationData
{
    public string city;
    public string country;
    public string region;
    public string loc;
}
