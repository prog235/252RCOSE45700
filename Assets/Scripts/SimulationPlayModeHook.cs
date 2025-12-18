using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[ExecuteAlways]
public class SimulationPlayModeHook : MonoBehaviour
{
    [SerializeField] private string baseUrl = "http://127.0.0.1:8000";

    private bool isPlaying = false;

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            isPlaying = true;
            StartCoroutine(StartSimulation());
        }
    }

    private void OnDisable()
    {
        // Play Mode 종료 시점
        if (isPlaying)
        {
            StartCoroutine(EndSimulation());
            isPlaying = false;
        }
    }

    private IEnumerator StartSimulation()
    {
        string url = $"{baseUrl}/simulation/start";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(
                System.Text.Encoding.UTF8.GetBytes("{}")
            );
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[PlayMode] start failed: {req.error}");
            }
            else
            {
                Debug.Log("[PlayMode] simulation started");
            }
        }
    }

    private IEnumerator EndSimulation()
    {
        string url = $"{baseUrl}/simulation/end";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(
                System.Text.Encoding.UTF8.GetBytes("{}")
            );
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[PlayMode] end failed: {req.error}");
            }
            else
            {
                Debug.Log("[PlayMode] simulation ended (log backed up)");
            }
        }
    }
}
