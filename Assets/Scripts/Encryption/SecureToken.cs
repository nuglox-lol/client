using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public static class SecureToken
{
	public static string key = "omgsosecureomgomgomgBTWifYouSeeThisThenIHateYouBecauseYouAreAnHackerOmgOmgOMgStopReadingThisHolyShitGetOutOkImActuallyGonnaStopWritingCauseThisIsGettingAnnoyingButYeahPleaseStopTryingToHackNugloxOrDebugOrWhateverOkNowGoodbye";
	public static string salt = SystemInfo.deviceUniqueIdentifier;

	public static void SetAuthToken(string token)
	{
		string encrypted = EncryptStringAES(token, key + salt);
		File.WriteAllText(Application.persistentDataPath + "/authtoken.dat", encrypted);
	}
	
	public static string GetAuthToken()
	{
		try
		{
			string path = Application.persistentDataPath + "/authtoken.dat";
			if(!File.Exists(path))
				return null;

			string savedEncrypted = File.ReadAllText(path);
			return DecryptStringAES(savedEncrypted, key + salt);
		}
		catch
		{
			return null;
		}
	}

    public static async Task<Dictionary<string, object>> GetAccountData()
    {
        string authtoken = GetAuthToken();
        if (string.IsNullOrEmpty(authtoken))
        {
            Debug.LogWarning("[NUGLOX.Log] No auth token found.");
            return null;
        }

        WWWForm form = new WWWForm();
        form.AddField("authtoken", authtoken);

        using (UnityWebRequest www = UnityWebRequest.Post("https://nuglox.xyz/v1/mobile/getinfo", form))
        {
            var operation = www.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("[NUGLOX.Log] Error fetching account data: " + www.error);
                return null;
            }
            else
            {
				string json = www.downloadHandler.text;
                try
                {
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    if (dict.TryGetValue("success", out var successValue) &&
                        successValue is bool success &&
                        !success)
                    {
                        Debug.LogWarning("[NUGLOX.Log] Account data fetch unsuccessful.");
                        return null;
                    }

                    return dict;
                }
                catch (Exception e)
                {
                    Debug.LogError("[NUGLOX.Log] JSON parse error: " + e.Message);
                    return null;
                }
            }
        }
    }

    public static string EncryptStringAES(string plainText, string key)
    {
        byte[] keyBytes = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes(salt), 1000).GetBytes(32);
        byte[] iv = new byte[16];

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (var sw = new StreamWriter(cs)) sw.Write(plainText);

            return Convert.ToBase64String(ms.ToArray());
        }
    }

    public static string DecryptStringAES(string cipherText, string key)
    {
        byte[] keyBytes = new Rfc2898DeriveBytes(key, Encoding.UTF8.GetBytes(salt), 1000).GetBytes(32);
        byte[] iv = new byte[16];

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }
}