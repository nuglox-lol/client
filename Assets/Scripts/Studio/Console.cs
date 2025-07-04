using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

public static class Console
{
    private static Transform panel;
    private static GameObject prefab;
    private static readonly Queue<GameObject> messages = new();

    public static void Report(string message)
    {
        if (panel == null)
        {
            GameObject panelObj = GameObject.Find("ConsolePanel");
            if (panelObj == null)
            {
                UnityEngine.Debug.LogWarning("ConsolePanel not found in scene.");
                return;
            }

            panel = panelObj.transform;
        }

        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Editor/Console/ConsolePrint");
            if (prefab == null)
            {
                UnityEngine.Debug.LogError("ConsolePrint prefab not found in Resources/UI.");
                return;
            }
        }

        if (messages.Count >= 4)
        {
            GameObject oldest = messages.Dequeue();
            UnityEngine.Object.Destroy(oldest);
        }

        GameObject newMessage = UnityEngine.Object.Instantiate(prefab, panel);
        TextMeshProUGUI textComponent = newMessage.GetComponent<TextMeshProUGUI>();

        if (textComponent != null)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string caller = GetCallingClassName();
            textComponent.text = $"{timestamp} - {caller} - {message}";
        }

        messages.Enqueue(newMessage);
    }
	
	public static void Clear()
	{
        if (panel == null)
        {
            GameObject panelObj = GameObject.Find("ConsolePanel");
            if (panelObj == null)
            {
                UnityEngine.Debug.LogWarning("ConsolePanel not found in scene.");
                return;
            }

            panel = panelObj.transform;
        }
		
		foreach(Transform child in panel)
		{
			if(child.GetComponent<TextMeshProUGUI>().text != "The console will appear here."){
				GameObject.Destroy(child.gameObject);	
			}
		}
	}

    private static string GetCallingClassName()
    {
        StackTrace stackTrace = new StackTrace();
        for (int i = 2; i < stackTrace.FrameCount; i++)
        {
            var method = stackTrace.GetFrame(i).GetMethod();
            if (method.DeclaringType != typeof(Console))
            {
                return method.DeclaringType.Name;
            }
        }
        return "Unknown";
    }
}
