using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class CharacterAppearanceDownloader
{
#if UNITY_EDITOR
    private static string BaseUrl = "http://localhost/v1/characterappearance";
#else
    private static string BaseUrl = GetArgs.Get("baseUrl") + "v1/characterappearance";
#endif
    private static string GlobalBaseUrl = GetArgs.Get("baseUrl");

    public static async Task DownloadFullAppearanceAsync(int id, Action<Dictionary<string, string>, string, string, string, string, string, string> onSuccess, Action<string> onError = null)
    {
        try
        {
            var url = $"{BaseUrl}/index.php?id={id}";
            var json = await GetTextAsync(url);

            var data = JsonUtility.FromJson<CharacterAppearanceData>(json);

            var colors = await DownloadBodyColorsAsync(data.bodycolorsid);
            var faceUrl = await DownloadFaceTextureAsync(data.faceid);
            var (hatObjUrl, hatTexUrl) = await DownloadHatDataAsync(data.hatid);
            var shirtUrl = await DownloadShirtTextureAsync(data.shirtid);
            var pantsUrl = await DownloadPantsTextureAsync(data.pantsid);
            string bodyType = data.bodytype;

            onSuccess?.Invoke(colors, faceUrl, hatObjUrl, hatTexUrl, shirtUrl, pantsUrl, bodyType);
        }
        catch (Exception e)
        {
            onError?.Invoke(e.Message);
        }
    }

    private static async Task<string> GetTextAsync(string url)
    {
        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        var operation = request.SendWebRequest();

        while (!operation.isDone)
            await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
            throw new Exception(request.error);

        return request.downloadHandler.text;
    }

    private static async Task<Dictionary<string, string>> DownloadBodyColorsAsync(int id)
    {
        var json = await GetTextAsync($"{BaseUrl}/bodycolors.php?id={id}");
        string fixedJson = "{\"colors\":" + json + "}";
        var wrapper = JsonUtility.FromJson<BodyColorsWrapper>(fixedJson);
        return wrapper.ToDictionary();
    }

    private static async Task<string> DownloadFaceTextureAsync(int id)
    {
        return (await GetTextAsync($"{BaseUrl}/face.php?id={id}")).Trim();
    }

    private static async Task<(string, string)> DownloadHatDataAsync(int id)
    {
        string response = (await GetTextAsync($"{BaseUrl}/hat.php?id={id}")).Trim();
        var parts = response.Split(';');
        if (parts.Length < 2) throw new Exception("Malformed hat data: " + response);
        return (parts[0].Trim(), parts[1].Trim());
    }

    private static async Task<string> DownloadShirtTextureAsync(int id)
    {
#if !UNITY_EDITOR
        if (id == 0) return "";
#endif
        string textureId = (await GetTextAsync($"{BaseUrl}/shirt.php?id={id}")).Trim();
        if (int.TryParse(textureId, out int texId))
            return $"{GlobalBaseUrl}catalog_storage/shirts/{texId}.png";
        return "";
    }

    private static async Task<string> DownloadPantsTextureAsync(int id)
    {
#if !UNITY_EDITOR
        if (id == 0) return "";
#endif
        string textureId = (await GetTextAsync($"{BaseUrl}/pants.php?id={id}")).Trim();
        if (int.TryParse(textureId, out int texId))
            return $"{GlobalBaseUrl}catalog_storage/pants/{texId}.png";
        return "";
    }

    [Serializable]
    private class CharacterAppearanceData
    {
        public int bodycolorsid;
        public int faceid;
        public int hatid;
        public int shirtid;
        public int pantsid;
        public string bodytype;
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