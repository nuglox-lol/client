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
    private string bodyType = "blocky";
    private NetworkIdentity networkIdentity;

    private Player playerComponent;

    public InstanceCamera Camera { get; private set; }

    private Renderer torsoRenderer2;
    private Renderer leftArmRenderer2;
    private Renderer rightArmRenderer2;
    private Renderer leftLegRenderer2;
    private Renderer rightLegRenderer2;

    private GameObject torsoMeshObj;
    private GameObject leftArmMeshObj;
    private GameObject rightArmMeshObj;
    private GameObject leftLegMeshObj;
    private GameObject rightLegMeshObj;

    public InstancePlayer(GameObject gameObject, Script lua = null) : base(gameObject, lua)
    {
        if (gameObject.GetComponent<ObjectClass>().className == "Player")
            gameObject.name = gameObject.GetComponent<Player>().username;
        if (gameObject.GetComponent<NetworkIdentity>())
            networkIdentity = gameObject.GetComponent<NetworkIdentity>();

        torsoRenderer2 = Transform.Find("Torso/default")?.GetComponent<Renderer>();
        leftArmRenderer2 = Transform.Find("LeftArm/default")?.GetComponent<Renderer>();
        rightArmRenderer2 = Transform.Find("RightArm/default")?.GetComponent<Renderer>();
        leftLegRenderer2 = Transform.Find("LeftLeg/default")?.GetComponent<Renderer>();
        rightLegRenderer2 = Transform.Find("RightLeg/default")?.GetComponent<Renderer>();

        torsoMeshObj = Transform.Find("Torso/default")?.gameObject;
        leftArmMeshObj = Transform.Find("LeftArm/default")?.gameObject;
        rightArmMeshObj = Transform.Find("RightArm/default")?.gameObject;
        leftLegMeshObj = Transform.Find("LeftLeg/default")?.gameObject;
        rightLegMeshObj = Transform.Find("RightLeg/default")?.gameObject;

        playerComponent = gameObject.GetComponent<Player>();
        Camera = new InstanceCamera(playerComponent);
    }

    public string TeamName
    {
        get => Transform.GetComponent<Player>().TeamName;
        set => Transform.GetComponent<Player>().TeamName = value;
    }

    public int UserId
    {
        get => Transform.GetComponent<Player>().userID;
        set => Transform.GetComponent<Player>().userID = value;
    }

    public ulong NetworkId => networkIdentity?.netId ?? 0;

    public int NetID => (int)NetworkId;

    public int Health
    {
        get => Transform.GetComponent<Player>().health;
        set => Transform.GetComponent<Player>().health = value;
    }

    public int WalkSpeed
    {
        get => (int)Transform.GetComponent<PlayerMovement>().speed;
        set
        {
            Transform.GetComponent<PlayerMovement>().speed = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetApplyWalkSpeed(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    void TargetApplyWalkSpeed(NetworkConnection target, int speed)
    {
        Transform.GetComponent<PlayerMovement>().speed = speed;
    }

    public int JumpForce
    {
        get => (int)Transform.GetComponent<PlayerMovement>().jumpForce;
        set
        {
            Transform.GetComponent<PlayerMovement>().jumpForce = value;
            if (NetworkServer.active && networkIdentity != null)
                TargetApplyJumpForce(networkIdentity.connectionToClient, value);
        }
    }

    [TargetRpc]
    void TargetApplyJumpForce(NetworkConnection target, int jumpForce)
    {
        Transform.GetComponent<PlayerMovement>().jumpForce = jumpForce;
    }

    public int MaximumHealth
    {
        get => Transform.GetComponent<Player>().maximumHealth;
        set => Transform.GetComponent<Player>().maximumHealth = value;
    }

    public string Username
    {
        get => Transform.GetComponent<Player>().username;
        set
        {
            Transform.GetComponent<Player>().username = value;
            Transform.name = value;
        }
    }

    public bool IsAdmin
    {
        get => Transform.GetComponent<Player>().isAdmin;
        set => Transform.GetComponent<Player>().isAdmin = value;
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

    public int ShirtId
    {
        get => Transform.GetComponent<Player>().ShirtId;
        set
        {
            Transform.GetComponent<Player>().ShirtId = value;
        }
    }

    public int PantsId
    {
        get => Transform.GetComponent<Player>().PantsId;
        set
        {
            Transform.GetComponent<Player>().PantsId = value;
        }
    }

    public int HatId
    {
        get => Transform.GetComponent<Player>().HatId;
        set
        {
            Transform.GetComponent<Player>().HatId = value;
        }
    }

    public string BodyType
    {
        get => Transform.GetComponent<Player>().BodyType;
        set
        {
            Transform.GetComponent<Player>().BodyType = value;
        }
    }

    private async void ApplyCharacterAppearance(int id)
    {
        await CharacterAppearanceDownloader.DownloadFullAppearanceAsync(id,
            (colors, faceUrl, hatObjUrl, hatTexUrl, shirtUrl, pantsUrl, bodyType) =>
            {
                this.bodyType = bodyType;
                if (bodyType != "blocky")
                    LoadBodyTypeMeshes(bodyType);
                ApplyColors(colors);
                ApplyFaceTexture(faceUrl);
                ApplyHat(hatObjUrl, hatTexUrl);
                if (!string.IsNullOrEmpty(shirtUrl))
                    StartDownloadTexture(shirtUrl, ApplyShirtTexture);
                else
                    ClearShirtMaterials();
                if (!string.IsNullOrEmpty(pantsUrl))
                    StartDownloadTexture(pantsUrl, ApplyPantsTexture);
            },
            error => Debug.LogError($"Failed to load character appearance: {error}")
        );
    }

    public void LoadBodyTypeMeshes(string bodyType)
    {
        LoadMeshForBodyPart("Torso", torsoMeshObj, ref torsoRenderer2);
        LoadMeshForBodyPart("LeftArm", leftArmMeshObj, ref leftArmRenderer2);
        LoadMeshForBodyPart("RightArm", rightArmMeshObj, ref rightArmRenderer2);
        LoadMeshForBodyPart("LeftLeg", leftLegMeshObj, ref leftLegRenderer2);
        LoadMeshForBodyPart("RightLeg", rightLegMeshObj, ref rightLegRenderer2);
    }

    public void LoadMeshForBodyPart(string bodyPart, GameObject oldObj, ref Renderer renderer)
    {
        if (oldObj == null) return;

        string path = $"Models/Character/{bodyType}/{bodyPart}";
        Mesh mesh = Resources.Load<Mesh>(path);
        if (mesh == null)
        {
            Debug.LogWarning($"Mesh not found at Resources/{path}");
            return;
        }

        var meshFilter = oldObj.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            Debug.LogWarning($"No MeshFilter on oldObj for bodyPart: {bodyPart}");
            return;
        }

        meshFilter.mesh = mesh;

        renderer = oldObj.GetComponent<Renderer>() ?? oldObj.GetComponentInChildren<Renderer>();

        switch (bodyPart)
        {
            case "Torso": torsoMeshObj = oldObj; break;
            case "LeftArm": leftArmMeshObj = oldObj; break;
            case "RightArm": rightArmMeshObj = oldObj; break;
            case "LeftLeg": leftLegMeshObj = oldObj; break;
            case "RightLeg": rightLegMeshObj = oldObj; break;
        }
    }

    public void StartDownloadTexture(string url, Action<Texture2D> onComplete)
    {
        UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
        req.SendWebRequest().completed += _ =>
        {
            if (req.result != UnityWebRequest.Result.Success)
                return;
            Texture2D tex = DownloadHandlerTexture.GetContent(req);
            onComplete?.Invoke(tex);
        };
    }

    public void ApplyFaceTexture(string imageUrl)
    {
        if (Transform == null) return;
        var renderer = Transform.Find("Head/default")?.GetComponent<Renderer>();
        if (renderer == null || renderer.materials.Length < 2) return;
        StartDownloadTexture(imageUrl, texture =>
        {
            var mats = renderer.materials;
            mats[1].mainTexture = texture;
            renderer.materials = mats;
        });
    }

    public void ApplyColors(Dictionary<string, string> colors)
    {
        if (Transform == null) return;
        foreach (var kvp in colors)
        {
            var renderer = Transform.Find($"{kvp.Key}/default")?.GetComponent<Renderer>();
            if (renderer == null) continue;
            if (ColorUtility.TryParseHtmlString(kvp.Value, out var c))
                renderer.material.color = c;
        }
    }

    public void ApplyHat(string objUrl, string textureUrl)
    {
        if (Transform == null) return;
        var head = Transform.Find("Head");
        if (head == null || SceneManager.GetActiveScene().name == "BCS") return;

        objUrl = objUrl.Trim();
        textureUrl = textureUrl.Trim();

        string tempObjPath = Path.Combine(Application.temporaryCachePath, $"hat_{Guid.NewGuid()}.obj");

        UnityWebRequest objRequest = UnityWebRequest.Get(objUrl);
        objRequest.downloadHandler = new DownloadHandlerFile(tempObjPath);
        objRequest.SendWebRequest().completed += _ =>
        {
            if (objRequest.result != UnityWebRequest.Result.Success)
                return;

            UnityWebRequest texRequest = UnityWebRequestTexture.GetTexture(textureUrl);
            texRequest.SendWebRequest().completed += __ =>
            {
                if (texRequest.result != UnityWebRequest.Result.Success)
                {
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
                        foreach (var mat in renderer.materials)
                        {
                            mat.mainTexture = texture;
                            mat.color = Color.white;
                        }
                }

                File.Delete(tempObjPath);
            };
        };
    }

    public Material CreateTransparentMaterial(Texture2D texture)
    {
        texture.Apply();
        Material mat = new Material(Shader.Find("Standard"))
        {
            mainTexture = texture,
            color = Color.white
        };
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        return mat;
    }

    public void ApplyShirtTexture(Texture2D texture)
    {
        if (texture == null || texture.width == 0 || texture.height == 0)
            return;
        
        Material newMaterial = CreateTransparentMaterial(texture);
        ApplyMaterialToRenderers(newMaterial, torsoRenderer2, leftArmRenderer2, rightArmRenderer2);
    }

    public void ApplyPantsTexture(Texture2D texture)
    {
        if (texture == null || texture.width == 0 || texture.height == 0)
            return;

        Material newMaterial = CreateTransparentMaterial(texture);
        ApplyMaterialToRenderers(newMaterial, leftLegRenderer2, rightLegRenderer2);
        if (!HasShirtTexture() && torsoRenderer2)
            ApplyMaterialToRenderers(newMaterial, torsoRenderer2);
    }

    public void ApplyMaterialToRenderers(Material mat, params Renderer[] renderers)
    {
        foreach (var renderer in renderers)
        {
            if (renderer == null) continue;
            var materials = renderer.materials;
            if (materials.Length > 1)
                materials[1] = mat;
            else
                materials = new Material[] { materials[0], mat };
            renderer.materials = materials;
        }
    }

    public bool HasShirtTexture()
    {
        if (torsoRenderer2 == null) return false;
        var mats = torsoRenderer2.materials;
        return mats.Length > 1 && mats[1].mainTexture != null;
    }

    public void ClearShirtMaterials()
    {
        ClearMaterial(torsoRenderer2);
        ClearMaterial(leftArmRenderer2);
        ClearMaterial(rightArmRenderer2);
    }

    public void ClearMaterial(Renderer renderer)
    {
        if (renderer == null) return;
        var mats = renderer.materials;
        if (mats.Length > 1)
        {
            Material mat = new Material(Shader.Find("Standard"))
            {
                color = Color.white,
                mainTexture = null
            };
            mat.SetFloat("_Mode", 0);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            mat.SetInt("_ZWrite", 1);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = -1;
            mats[1] = mat;
            renderer.materials = mats;
        }
    }

    public void Move(Vector3 moveDir)
    {
        var playerMovement = Transform.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.Move(moveDir);
        }
    }

    public void TakeDamage(int damage)
    {
        Transform.GetComponent<Player>().health -= damage;
    }
}