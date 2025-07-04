using UnityEngine;
using System.Runtime.InteropServices;

public static class ErrorReporting
{
    public enum MessageType
    {
        Info,
        Error,
        Exclamation
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int MessageBox(System.IntPtr hWnd, string text, string caption, uint type);
#endif

    public static void SendMessage(string message, MessageType type)
    {
        string fullMessage = message;

        if (type == MessageType.Error)
        {
            fullMessage += "\n\nPlease report this to a NUGLOX Staff Member.";
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        uint boxType = 0;

        switch (type)
        {
            case MessageType.Info:
                boxType = 0x40; // MB_ICONINFORMATION
                break;
            case MessageType.Error:
                boxType = 0x10; // MB_ICONERROR
                break;
            case MessageType.Exclamation:
                boxType = 0x30; // MB_ICONEXCLAMATION
                break;
        }

        MessageBox(System.IntPtr.Zero, fullMessage, "NUGLOX", boxType);
#else
        switch (type)
        {
            case MessageType.Info:
                Debug.Log(fullMessage);
                break;
            case MessageType.Error:
                Debug.LogError(fullMessage);
                break;
            case MessageType.Exclamation:
                Debug.LogWarning(fullMessage);
                break;
        }
#endif
    }
}
