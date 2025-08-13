using System;
using System.Collections.Generic;
using Mirror;

public static class ChatService
{
    public static event Action<string> OnMessageReceived;

    private static List<string> messages = new List<string>();
    public static IReadOnlyList<string> Messages => messages.AsReadOnly();

    public static void ReceiveMessage(string message)
    {
        messages.Add(message);
        OnMessageReceived?.Invoke(message);
    }

    public static void Clear()
    {
        messages.Clear();
    }

    public static void ServerAddMessage(string message)
    {
        if (!NetworkServer.active)
            return;

        string serverMessage = $"[SERVER]: {message}";
        messages.Add(serverMessage);
        OnMessageReceived?.Invoke(serverMessage);
    }
}
