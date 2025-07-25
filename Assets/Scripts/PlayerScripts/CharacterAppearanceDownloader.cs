using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class CharacterAppearanceDownloader
{
    #if UNITY_EDITOR
        private static string BaseUrl = "http://localhost/v1/characterappearance";
    #else
        private static string BaseUrl = GetArgs.Get("baseUrl") + "v1/characterappearance";
    #endif

    public static void DownloadFullAppearance(int id, Action<Dictionary<string, string>, string, string, string, string> onSuccess, Action<string> onError = null)
    {
        string url = $"{BaseUrl}/index.php?id={id}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SendWebRequest().completed += (AsyncOperation op) =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                return;
            }

            try
            {
                CharacterAppearanceData data = JsonUtility.FromJson<CharacterAppearanceData>(request.downloadHandler.text);
                DownloadBodyColors(data.bodycolorsid,
                    colors =>
                    {
                        DownloadTShirtTexture(data.tshirtid,
                            tshirtUrl =>
                            {
                                DownloadFaceTexture(data.faceid,
                                    faceUrl =>
                                    {
                                        DownloadHatData(data.hatid,
                                            (hatObjUrl, hatTexUrl) =>
                                            {
                                                onSuccess?.Invoke(colors, tshirtUrl, faceUrl, hatObjUrl, hatTexUrl);
                                            },
                                            onError);
                                    },
                                    onError);
                            },
                            onError);
                    },
                    onError);
            }
            catch (Exception e)
            {
                onError?.Invoke($"Parsing error: {e.Message}");
            }
        };
    }

    private static void DownloadFaceTexture(int id, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/face.php?id={id}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SendWebRequest().completed += (op) =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                return;
            }

            string imageUrl = request.downloadHandler.text.Trim();
            onSuccess?.Invoke(imageUrl);
        };
    }

    private static void DownloadBodyColors(int id, Action<Dictionary<string, string>> onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/bodycolors.php?id={id}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SendWebRequest().completed += (op) =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                return;
            }

            try
            {
                string fixedJson = "{\"colors\":" + request.downloadHandler.text + "}";
                BodyColorsWrapper wrapper = JsonUtility.FromJson<BodyColorsWrapper>(fixedJson);
                onSuccess?.Invoke(wrapper.ToDictionary());
            }
            catch (Exception e)
            {
                onError?.Invoke($"Parsing body colors: {e.Message}");
            }
        };
    }

    private static void DownloadTShirtTexture(int id, Action<string> onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/tshirt.php?id={id}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SendWebRequest().completed += (op) =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                return;
            }

            string imageUrl = request.downloadHandler.text.Trim();
            onSuccess?.Invoke(imageUrl);
        };
    }

    private static void DownloadHatData(int id, Action<string, string> onSuccess, Action<string> onError)
    {
        string url = $"{BaseUrl}/hat.php?id={id}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SendWebRequest().completed += (op) =>
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke(request.error);
                return;
            }

            string responseText = request.downloadHandler.text.Trim();

            string[] parts = responseText.Split(';');
            if (parts.Length < 2)
            {
                onError?.Invoke("Malformed hat data: " + responseText);
                return;
            }

            string objUrl = parts[0].Trim();
            string textureUrl = parts[1].Trim();

            onSuccess?.Invoke(objUrl, textureUrl);
        };
    }

    [Serializable]
    private class CharacterAppearanceData
    {
        public int bodycolorsid;
        public int tshirtid;
        public int faceid;
        public int hatid;
    }

    [Serializable]
    private class BodyColorsWrapper
    {
        public BodyColor[] colors;

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();
            foreach (var color in colors)
            {
                dict[color.part] = color.color;
            }
            return dict;
        }
    }

    [Serializable]
    private class BodyColor
    {
        public string part;
        public string color;
    }
}
