using UnityEngine;
using System.Collections;

public class MobileUIWebRenderer : MonoBehaviour
{
    public UniWebView webView;

    void Start()
    {
        string path = UniWebViewHelper.StreamingAssetURLForPath("MobilePublic/index.html");

        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        webView.SetSupportMultipleWindows(true, false);
        webView.OnMessageReceived += OnWebViewMessageReceived;
        webView.Load(path);
        webView.Show();

        webView.OnPageFinished += AuthenticateWeb;
    }

    public void Show()
    {
        if (webView != null)
        {
            webView.Show();
        }
    }

    public void Hide()
    {
        if (webView != null)
        {
            webView.Hide();
        }
    }

    void AuthenticateWeb(UniWebView view, int statusCode, string url)
    {
        StartCoroutine(DelayedSetAuthToken(view));
    }

    private void OnWebViewMessageReceived(UniWebView view, UniWebViewMessage message)
    {
        if (message.Path == "SetAuthToken")
        {
            string token = message.Args["token"];
            SecureToken.SetAuthToken(token);
            webView.EvaluateJavaScript($"reloadApp();");
        }
    }

    IEnumerator DelayedSetAuthToken(UniWebView view)
    {
        yield return new WaitForSeconds(2f);

        string token = SecureToken.GetAuthToken();
        view.EvaluateJavaScript($"SetAuthToken(\"" + token + "\");");
    }

    public void PostMessage(string message)
    {
        if (webView != null)
        {
            webView.EvaluateJavaScript($"window.postMessage({JsonUtility.ToJson(message)}, '*');");
        }
    }
}