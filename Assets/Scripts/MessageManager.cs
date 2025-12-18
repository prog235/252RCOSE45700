using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class MessageManager : MonoBehaviour
{
    [SerializeField] private string endpointUrl;
    [SerializeField] private GameObject toastPrefab;
    [SerializeField] private Transform parentTransform;

    public void SendUserMessage(string message, int curRoom, int truthLevel, bool gifted)
    {
        StartCoroutine(SendAndStream(message, curRoom, truthLevel, gifted));
    }

    private IEnumerator SendAndStream(string message, int curRoom, int truthLevel, bool gifted)
    {
        GameObject bubbleObj = Instantiate(toastPrefab, parentTransform);
        ToastBubble bubble = bubbleObj.GetComponent<ToastBubble>();

        // JSON payload
        string payload = "{"
            + "\"message\":\"" + Escape(message) + "\","
            + "\"curRoom\":" + curRoom + ","
            + "\"truthLevel\":" + truthLevel + ","
            + "\"gifted\":" + gifted.ToString().ToLower()
            + "}";

        byte[] body = Encoding.UTF8.GetBytes(payload);

        using (var req = new UnityWebRequest(endpointUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(body);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            var operation = req.SendWebRequest();
            string fullText = "";

            int lastProcessedLength = 0;

            while (!operation.isDone || lastProcessedLength < req.downloadHandler.text.Length)
            {
                string newText = req.downloadHandler.text;

                if (newText.Length > lastProcessedLength)
                {
                    string delta = newText.Substring(lastProcessedLength);
                    string[] lines = delta.Split("\n");

                    foreach (var line in lines)
                    {
                        if (line.StartsWith("data: "))
                        {
                            string token = line.Substring(6);
                            if (token == "[DONE]") continue;

                            bubble.AppendText(token);  // Append only new token

                            yield return new WaitForSecondsRealtime(0.1f);
                        }
                    }

                    lastProcessedLength = newText.Length; // 처리한 길이 업데이트
                }

                yield return null;
            }
            // 마지막에 FadeOut 시작
            bubble.Finish();
        }
    }


    private string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
