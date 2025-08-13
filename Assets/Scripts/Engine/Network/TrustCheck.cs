using UnityEngine.Networking;
using UnityEngine;
using System;

public static class TrustCheck
{
    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    public static void Apply(UnityWebRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.url))
        {
            Debug.LogWarning("TrustCheck: Invalid UnityWebRequest or URL.");
            return;
        }

        Uri uri;
        try
        {
            uri = new Uri(request.url);
        }
        catch (UriFormatException)
        {
            Debug.LogWarning("TrustCheck: Malformed URL.");
            return;
        }

        string host = uri.Host.ToLower();

        if (host == "nuglox.com" || host == "localhost" || host == "127.0.0.1")
        {
            request.certificateHandler = new BypassCertificateHandler();
            Debug.Log($"TrustCheck: SSL bypass applied to {host}");
        }
        else
        {
            Debug.LogWarning($"TrustCheck: SSL bypass denied for untrusted domain '{host}'.");
        }
    }
}