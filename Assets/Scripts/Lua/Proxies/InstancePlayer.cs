using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Dummiesman;
using System;
using System.IO;
using Mirror;

[MoonSharpUserData]
public class InstancePlayer : InstanceDatamodel
{
    private int characterAppearanceId;

    private NetworkIdentity networkIdentity;

    public InstancePlayer(GameObject gameObject, Script lua = null) : base(gameObject, lua) {
        if(gameObject.GetComponent<ObjectClass>().className == "Player")
            gameObject.name = gameObject.GetComponent<Player>().username;
        if(gameObject.GetComponent<NetworkIdentity>())
            networkIdentity = gameObject.GetComponent<NetworkIdentity>();
    }

    public int UserId
    {
        get => Transform.GetComponent<Player>().userID;
        set { Transform.GetComponent<Player>().userID = value; }
    }

    public ulong NetworkId => networkIdentity != null ? networkIdentity.netId : 0;

    public int NetID => (int)NetworkId;

    public int Health
    {
        get => Transform.GetComponent<Player>().health;
        set { Transform.GetComponent<Player>().health = value; }
    }

    public int MaximumHealth
    {
        get => Transform.GetComponent<Player>().maximumHealth;
        set { Transform.GetComponent<Player>().maximumHealth = value; }
    }

    public string Username
    {
        get => Transform.GetComponent<Player>().username;
        set { Transform.GetComponent<Player>().username = value; Transform.name = value; }
    }

    public bool IsAdmin
    {
        get => Transform.GetComponent<Player>().isAdmin;
        set { Transform.GetComponent<Player>().isAdmin = value; }
    }

    public int CharacterAppearance
    {
        get => characterAppearanceId;
        set
        {
            Transform.GetComponent<Player>().characterAppearanceId = value;
            characterAppearanceId = value;
            ApplyCharacterAppearance(characterAppearanceId);
        }
    }

    private void ApplyCharacterAppearance(int id)
    {
        CharacterAppearanceDownloader.DownloadFullAppearance(id,
            (colors, tshirtUrl, faceUrl, hatObjUrl, hatTexUrl) =>
            {
                ApplyColors(colors);
                ApplyTShirtTexture(tshirtUrl);
                ApplyFaceTexture(faceUrl);
                ApplyHat(hatObjUrl, hatTexUrl);
            },
            error => Debug.LogError($"Failed to load character appearance: {error}")
        );
    }

    public void TakeDamage(int damage)
    {
        Transform.GetComponent<Player>().health = Transform.GetComponent<Player>().health - damage;
    }

    private void ApplyFaceTexture(string imageUrl)
    {
        if (Transform == null) return;
        var head = Transform.Find("Head");
        var defaultChild = head?.Find("default");
        var renderer = defaultChild?.GetComponent<Renderer>();
        if (renderer == null || renderer.materials.Length < 2) return;

        var faceRequest = UnityWebRequestTexture.GetTexture(imageUrl);
        faceRequest.SendWebRequest().completed += _ =>
        {
            if (faceRequest.result != UnityWebRequest.Result.Success) return;
            Texture2D texture = DownloadHandlerTexture.GetContent(faceRequest);
            var mats = renderer.materials;
            mats[1].mainTexture = texture;
            renderer.materials = mats;
        };
    }

    private void ApplyTShirtTexture(string imageUrl)
    {
        if (Transform == null) return;
        var tshirt = Transform.Find("TShirt");
        var renderer = tshirt?.GetComponent<Renderer>();
        if (renderer == null) return;

        var textureRequest = UnityWebRequestTexture.GetTexture(imageUrl);
        textureRequest.SendWebRequest().completed += _ =>
        {
            if (textureRequest.result != UnityWebRequest.Result.Success) return;
            renderer.material.color = Color.white;
            renderer.material.mainTexture = DownloadHandlerTexture.GetContent(textureRequest);
        };
    }

    private void ApplyColors(Dictionary<string, string> colors)
    {
        if (Transform == null) return;
        foreach (var kvp in colors)
        {
            var part = Transform.Find(kvp.Key);
            var defaultChild = part?.Find("default");
            var renderer = defaultChild?.GetComponent<Renderer>();
            if (renderer == null) continue;
            if (ColorUtility.TryParseHtmlString(kvp.Value, out var c))
                renderer.material.color = c;
        }
    }

    private void ApplyHat(string objUrl, string textureUrl)
    {
        if (Transform == null) return;
        var head = Transform.Find("Head");
        if (head == null) return;
        if (SceneManager.GetActiveScene().name == "BCS") return;

        objUrl = objUrl.Trim();
        textureUrl = textureUrl.Trim();

        string tempObjPath = Path.Combine(Application.temporaryCachePath, $"hat_{Guid.NewGuid()}.obj");

        UnityWebRequest objRequest = UnityWebRequest.Get(objUrl);
        objRequest.downloadHandler = new DownloadHandlerFile(tempObjPath);
        objRequest.SendWebRequest().completed += _ =>
        {
            if (objRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("OBJ download failed: " + objRequest.error);
                return;
            }

            UnityWebRequest texRequest = UnityWebRequestTexture.GetTexture(textureUrl);
            texRequest.SendWebRequest().completed += __ =>
            {
                if (texRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Texture download failed: " + texRequest.error);
                    File.Delete(tempObjPath);
                    return;
                }

                Texture2D texture = DownloadHandlerTexture.GetContent(texRequest);

                var loader = new OBJLoader();
                GameObject hat = loader.Load(tempObjPath);
                if (hat != null)
                {
                    hat.transform.SetParent(head, false);
                    hat.tag = "Object";
                    hat.transform.localPosition = Vector3.zero;
                    hat.transform.localRotation = Quaternion.identity;
                    hat.transform.localScale = new Vector3(-1, 1, 1);
                    hat.name = "Hat";

                    foreach (var renderer in hat.GetComponentsInChildren<Renderer>())
                    {
                        foreach (var mat in renderer.materials)
                        {
                            mat.mainTexture = texture;
                            mat.color = Color.white;
                        }
                    }
                }

                File.Delete(tempObjPath);
            };
        };
    }
}
