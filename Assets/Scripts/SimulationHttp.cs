using UnityEngine;
using UnityEngine.Networking;

public static class SimulationHttp
{
    // 에디터 테스트용. 필요하면 ScriptableObject/Config로 빼도 됨.
    public static string BaseUrl = "http://127.0.0.1:8000";

    public static void PostJsonFireAndForget(string path)
    {
        string url = $"{BaseUrl}{path}";

        var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        // 코루틴 없이 요청만 시작
        req.SendWebRequest();
    }
}
