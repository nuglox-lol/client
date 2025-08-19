using UnityEngine;
using TMPro;
using Mirror;
using System.Collections.Generic;

public class ChatUI : MonoBehaviour
{
    public TMP_InputField inputField;
    public Transform content;
    public int maxMessages = 3;

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

        if (messageTimes.Count > 10)
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
        if (text != null)
        {
            text.text = message;
            if (message.Length > 0)
            {
                char firstChar = message[1];
                text.color = GetColorFromChar(firstChar, message);
            }
        }

        if (content.childCount > maxMessages)
            Destroy(content.GetChild(0).gameObject);
    }

    private Color GetColorFromChar(char c, string fullMessage)
    {
        if (fullMessage.StartsWith("[ChatService]"))
            return Color.white;

        if (fullMessage.StartsWith("[Server]"))
            return Color.white;

        c = char.ToUpper(c);
        int code = c - 'A';
        switch (code)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return new Color(1f, 0.5f, 0f);
            case 5: return Color.cyan;
            case 6: return Color.magenta;
            case 7: return Color.gray;
            case 8: return new Color(0.5f, 0f, 1f);
            case 9: return new Color(1f, 0f, 0.5f);
            default: return Color.white;
        }
    }
}
