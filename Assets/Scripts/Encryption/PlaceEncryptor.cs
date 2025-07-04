using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class PlaceEncryptor
{
    private static string encryptionKey = "MuchSecretVerySecureSoWowMuchEncryptionPlacefileV3";
    private static string encryptionSalt = SystemInfo.deviceUniqueIdentifier + "_placefile";

    public static string Encrypt(string plainText)
    {
        byte[] keyBytes = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes(encryptionSalt), 1500).GetBytes(32);
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

    public static string Decrypt(string cipherText)
    {
        byte[] keyBytes = new Rfc2898DeriveBytes(encryptionKey, Encoding.UTF8.GetBytes(encryptionSalt), 1500).GetBytes(32);
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
