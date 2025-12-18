// ChatManager.cs
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private MessageManager postMessage;
    [SerializeField] private ToastBubble playerChatPrefab;
    [SerializeField] private Transform parent;

    private void Awake()
    {
        sendButton.onClick.AddListener(OnSendClicked);
        inputField.onSubmit.AddListener(_ => OnSendClicked());
    }

    private void OnSendClicked()
    {
        string msg = inputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        CreatePlayerChat(msg);
        int curRoom = GameManager.Instance.curRoom;
        int truthLevel = StateManager.Instance.truthSum;
        bool gifted = StateManager.Instance.gifted;

        inputField.text = "";
        postMessage.SendUserMessage(msg, curRoom, truthLevel, gifted); 
    }

    private void CreatePlayerChat(string msg)
    {
        var bubble = Instantiate(playerChatPrefab, parent);
        bubble.Play(msg);
    }
}
