using System;
using UnityEngine;
using Mirror;

public static class NetworkIdentityExtensions
{
    private static uint nextAssetId = 1000;

    public static uint GenerateAssetID()
    {
        return nextAssetId++;
    }

    public static void SetAssetId(this NetworkIdentity identity, uint newAssetId)
    {
        var assetIdField = typeof(NetworkIdentity).GetField("m_AssetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (assetIdField != null)
        {
            assetIdField.SetValue(identity, newAssetId);
        }
        else
        {
            Debug.LogError("Failed to find NetworkIdentity assetId field");
        }
    }
}
