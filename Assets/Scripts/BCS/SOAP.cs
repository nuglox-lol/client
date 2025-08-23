using UnityEngine;
using System.Net;
using System.Threading;
using System.IO;

public class SOAP : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;
    public ScriptService scriptService;
    private float timer = 0f;

    void Start()
    {
        scriptService.Init();
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8081/");
        listener.Start();

        listenerThread = new Thread(() =>
        {
            while (listener.IsListening)
            {
                var context = listener.GetContext();
                var request = context.Request;

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/run")
                {
                    using var reader = new StreamReader(request.InputStream);
                    string luaCode = reader.ReadToEnd();

                    var resetEvent = new ManualResetEvent(false);
                    string responseString = "error";

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        scriptService.RunScript(luaCode, result =>
                        {
                            responseString = result;
                            resetEvent.Set();
                        });
                    });

                    resetEvent.WaitOne();

                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        foreach (var obj in GameObject.FindGameObjectsWithTag("Object"))
                        {
                            Destroy(obj);
                        }
                        foreach (var obj in GameObject.FindGameObjectsWithTag("MainCamera"))
                        {
                            Destroy(obj);
                        }
                    });

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
            }
        });
        listenerThread.Start();

        Debug.Log("Lua HTTP Server started on http://localhost:8081/run");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            timer = 0f;
        }

        foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.name == "PlayerCamera")
                Destroy(obj);
        }
    }

    void OnApplicationQuit()
    {
        listener?.Stop();
        listenerThread?.Abort();
    }
}
