using UnityEngine;
using TMPro;
using Mirror;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Transform content;
    public int maxMessages = 50;
    public ScrollRect scrollRect;

    private Player localPlayer;
    private Queue<float> messageTimes = new();
    private bool isRestricted = false;
    private float restrictionEndTime;

    private void OnEnable()
    {
        ChatService.OnMessageReceived += HandleMessageReceived;
        inputField.onSubmit.AddListener(OnInputSubmitted);
        inputField.onSelect.AddListener(_ => inputField.text = "");
    }

    private void OnDisable()
    {
        ChatService.OnMessageReceived -= HandleMessageReceived;
        inputField.onSubmit.RemoveListener(OnInputSubmitted);
        inputField.onSelect.RemoveAllListeners();
    }

    private void Update()
    {
        if (localPlayer == null)
        {
            if (NetworkClient.active && NetworkClient.localPlayer != null)
                localPlayer = NetworkClient.localPlayer.GetComponent<Player>();
            return;
        }

        if (isRestricted && Time.time >= restrictionEndTime)
            isRestricted = false;

        if (!inputField.isFocused && Input.GetKeyDown(KeyCode.Slash))
        {
            inputField.ActivateInputField();
            inputField.text = "";
        }
    }

    private void OnInputSubmitted(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (localPlayer == null) return;

        if (isRestricted)
        {
            ChatService.ReceiveMessage("[ChatService]: You've been restricted to talk!");
            inputField.text = "";
            inputField.DeactivateInputField();
            return;
        }

        float now = Time.time;
        messageTimes.Enqueue(now);

        while (messageTimes.Count > 0 && now - messageTimes.Peek() > 10f)
            messageTimes.Dequeue();

        if (messageTimes.Count > 5)
        {
            isRestricted = true;
            restrictionEndTime = now + 30f;
            ChatService.ReceiveMessage("[ChatService]: You've been restricted to talk!");
            inputField.text = "";
            inputField.DeactivateInputField();
            return;
        }

        localPlayer.CmdSendChatMessage($"[{localPlayer.username}]: {text}");
        inputField.text = "";
        inputField.DeactivateInputField();
    }

    private void HandleMessageReceived(string message)
    {
        var prefab = Resources.Load<GameObject>("ChatMessageEntry");
        if (prefab == null) return;

        var instance = Instantiate(prefab, content);
        var text = instance.GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = message;

        if (content.childCount > maxMessages)
            Destroy(content.GetChild(0).gameObject);

        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }
    }
}