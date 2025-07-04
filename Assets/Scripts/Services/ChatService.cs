using System;
using System.Collections.Generic;

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
}
