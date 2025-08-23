using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using Mirror;
using TMPro;

public static class DataService
{
    [System.Serializable]
    public class SavedObjectData
    {
        public string Name;
        public Vector3Serializable Position;
        public Vector3Serializable Rotation;
        public Vector3Serializable Scale;
        public ColorSerializable Color;
        public string ClassName;
        public string ScriptContent;
        public string ParentName;
        public bool Anchored;
        public bool Gravity;
        public bool CanCollide;
        public bool IsLocalScript;
        public float Mass;
        public float ExplosionRadius;
        public float ExplosionForce;
        public float ExplosionUpwardsModifier;
        public float ExplosionMassThreshold;
        public float WalkSpeed;
        public float JumpPower;
        public float RespawnTime;
        public string Text3DText;
        public string NPCBehaviour;
        public int SoundID;
        public bool SoundIsPlaying;
        public bool SoundAutoplay;
        public float SoundVolume;
        public float SoundTime;
        public bool SoundLoop;
        public bool SoundPlayInWorld;
        public float SoundPitch;
        public ColorSerializable TeamColor;
        public string TeamName;
        public int MeshID;
        public string Text;
        public Vector3Serializable LightRotation;
        public int LightExposure;
        public float LightContrast;
        public ColorSerializable LightTint;
        public int SkyID;
        public int IntValue;
        public string StringValue;
        public bool BoolValue;
        public float FloatValue;
        public int DecalId;
        public string DecalFace;
    }

    [System.Serializable]
    public class SaveData
    {
        public List<SavedObjectData> Objects = new List<SavedObjectData>();
    }

    private class CoroutineStarter : MonoBehaviour { }

    private static void RunCoroutine(IEnumerator routine)
    {
        var runner = new GameObject("TempCoroutineRunner").AddComponent<CoroutineStarter>();
        Object.DontDestroyOnLoad(runner.gameObject);
        runner.StartCoroutine(DestroyAfter(routine, runner));
    }

    private static IEnumerator DestroyAfter(IEnumerator routine, MonoBehaviour runner)
    {
        yield return runner.StartCoroutine(routine);
        Object.Destroy(runner.gameObject);
    }

    public static void New()
    {
        RunCoroutine(NewRoutine());
    }

    private static IEnumerator NewRoutine()
    {
        var objectsToDelete = GameObject.FindGameObjectsWithTag("Object");
        foreach (var obj in objectsToDelete)
            GameObject.Destroy(obj);

        yield return new WaitForSeconds(1f);
    }

    public static void Save(string path)
    {
        var objectsToSave = GameObject.FindGameObjectsWithTag("Object");
        var saveData = new SaveData();

        foreach (var obj in objectsToSave)
        {
            var classComp = obj.GetComponent<ObjectClass>();
            if (classComp == null) continue;

            var renderer = obj.GetComponent<Renderer>();
            Color objColor = renderer ? renderer.material.color : Color.white;

            string luaScriptText = "";
            bool isLocalScript = false;
            var luaComp = obj.GetComponent<ScriptInstanceMain>();
            if (luaComp != null)
            {
                luaScriptText = luaComp.Script;
                isLocalScript = luaComp.isLocalScript;
            }

            var rb = obj.GetComponent<Rigidbody>();
            bool anchored = rb == null;
            bool gravity = rb != null ? rb.useGravity : false;
            float mass = rb != null ? rb.mass : 1f;

            var colliders = obj.GetComponents<Collider>();
            bool canCollide = true;
            if (colliders.Length > 0)
                canCollide = colliders[0].enabled;

            var explosion = obj.GetComponent<Explosion>();
            float explosionRadius = explosion ? explosion.radius : 0f;
            float explosionForce = explosion ? explosion.explosionForce : 0f;
            float upwardsMod = explosion ? explosion.upwardsModifier : 0f;
            float massThreshold = explosion ? explosion.massThreshold : 0f;

            var pd = obj.GetComponent<PlayerDefaults>();
            float walkSpeed = 16f, jumpPower = 50f, respawnTime = 1f;
            if (pd != null)
            {
                walkSpeed = pd.walkSpeed;
                jumpPower = pd.jumpPower;
                respawnTime = pd.respawnTime;
            }

            string text3DText = null;
            if (obj.GetComponent<ObjectClass>().className == "Text3D")
            {
                var text3Dd = obj.GetComponent<Text3DComponent>();
                if (text3Dd != null)
                {
                    text3DText = text3Dd.GetText();
                }
            }

            string npcBehaviour = null;
            if(obj.GetComponent<ObjectClass>().className == "NPC")
            {
                npcBehaviour = obj.GetComponent<NPCMovement>().currentBehavior;
            }

            var soundComp = obj.GetComponent<Sound>();

            int soundID = 0;
            bool soundIsPlaying = false;
            bool soundAutoplay = false;
            float soundVolume = 1f;
            float soundTime = 0f;
            bool soundLoop = false;
            bool soundPlayInWorld = false;
            float soundPitch = 1f;

            if (soundComp != null)
            {
                soundID = soundComp.SoundID;
                soundIsPlaying = soundComp.Playing;
                soundAutoplay = soundComp.Autoplay;
                soundVolume = soundComp.Volume;
                soundTime = soundComp.Time;
                soundLoop = soundComp.Loop;
                soundPlayInWorld = soundComp.PlayInWorld;
                soundPitch = soundComp.Pitch;
            }

            var teamComp = obj.GetComponent<Team>();

            Color teamColor = new Color(0, 0, 0);
            string teamName = "";

            if(teamComp != null)
            {
                teamColor = teamComp.TeamColor;
                teamName = teamComp.TeamName;
            }

            var spComp = obj.GetComponent<SpawnPoint>();

            if(spComp != null)
            {
                teamName = spComp.teamName;
            }

            var mComp = obj.GetComponent<MeshComponent>();

            int meshId = 0;

            if(mComp != null)
            {
                meshId = mComp.meshID;
            }

            string text = null;
            var text3D = obj.GetComponent<TMP_Text>();
            if (text3D != null)
                text = obj.GetComponent<TMP_Text>().text;

            var lightComp = obj.GetComponent<LightComponent>();
            Vector3 lightRotation = Vector3.zero;
            int lightExposure = 0;
            float lightContrast = 0f;
            Color lightTint = Color.white;
            if (lightComp != null)
            {
                lightRotation = lightComp.Rotation;
                lightExposure = Mathf.RoundToInt(lightComp.Exposure);
                lightContrast = 1f;
                lightTint = lightComp.Tint;
            }

            var skyComp = obj.GetComponent<Sky>();
            int skyId = 0;
            if(skyComp != null)
            {
                skyId = skyComp.id;
            }

            int intValue = 0;
            string stringValue = "";
            bool boolValue = false;
            float floatValue = 0;

            var valueCompInt = obj.GetComponent<IntValue>();
            var valueCompString = obj.GetComponent<StringValue>();
            var valueCompBool = obj.GetComponent<BoolValue>();
            var valueCompFloat = obj.GetComponent<FloatValue>();

            if (valueCompInt != null)
            {
                intValue = valueCompInt.Value;
            }

            if (valueCompString != null)
            {
                stringValue = valueCompString.Value;
            }

            if (valueCompBool != null)
            {
                boolValue = valueCompBool.Value;
            }

            if (valueCompFloat != null)
            {
                floatValue = valueCompFloat.Value;
            }

            int decalId = 0;
            string decalFace = null;

            var decalComp = obj.GetComponent<Decal>();

            if(decalComp != null)
            {
                decalId = decalComp.DecalId;
                decalFace = decalComp.Face;
            }

            saveData.Objects.Add(new SavedObjectData
            {
                Name = obj.name,
                Position = new Vector3Serializable(obj.transform.position),
                Rotation = new Vector3Serializable(obj.transform.eulerAngles),
                Scale = new Vector3Serializable(obj.transform.localScale),
                Color = new ColorSerializable(objColor),
                ClassName = classComp.className,
                ScriptContent = luaScriptText,
                ParentName = obj.transform.parent ? obj.transform.parent.name : null,
                Anchored = anchored,
                Gravity = gravity,
                CanCollide = canCollide,
                IsLocalScript = classComp.className == "Script" ? isLocalScript : false,
                Mass = mass,
                ExplosionRadius = explosionRadius,
                ExplosionForce = explosionForce,
                ExplosionUpwardsModifier = upwardsMod,
                ExplosionMassThreshold = massThreshold,
                WalkSpeed = walkSpeed,
                JumpPower = jumpPower,
                RespawnTime = respawnTime,
                Text3DText = text3DText,
                NPCBehaviour = npcBehaviour,
                SoundID = soundID,
                SoundIsPlaying = soundIsPlaying,
                SoundAutoplay = soundAutoplay,
                SoundVolume = soundVolume,
                SoundTime = soundTime,
                SoundLoop = soundLoop,
                SoundPlayInWorld = soundPlayInWorld,
                SoundPitch = soundPitch,
                TeamColor = new ColorSerializable(teamColor),
                TeamName = teamName,
                MeshID = meshId,
                Text = text,
                LightRotation = new Vector3Serializable(lightRotation),
                LightExposure = lightExposure,
                LightContrast = lightContrast,
                LightTint = new ColorSerializable(lightTint),
                SkyID = skyId,
                IntValue = intValue,
                StringValue = stringValue,
                BoolValue = boolValue,
                FloatValue = floatValue,
                DecalId = decalId,
                DecalFace = decalFace,
            });
        }

        GameObject cam = GameObject.Find("BuildCam");
        if (cam != null)
        {
            saveData.Objects.Add(new SavedObjectData
            {
                Name = "MapCamera",
                Position = new Vector3Serializable(cam.transform.position),
                Rotation = new Vector3Serializable(cam.transform.eulerAngles),
                Scale = new Vector3Serializable(Vector3.one),
                Color = new ColorSerializable(Color.clear),
                ClassName = "MapCameraCube",
                ScriptContent = "",
                ParentName = null,
                Anchored = true,
                Gravity = false,
                CanCollide = false,
                IsLocalScript = false,
                Mass = 1f,
                ExplosionRadius = 0f,
                ExplosionForce = 0f,
                ExplosionUpwardsModifier = 0f,
                ExplosionMassThreshold = 0f,
                WalkSpeed = 16f,
                JumpPower = 50f,
                RespawnTime = 1f
            });
        }

        var serializer = new XmlSerializer(typeof(SaveData));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, saveData);
        }
    }

    public static void Load(string path, bool isMultiplayer = false)
    {
        RunCoroutine(LoadRoutine(path, isMultiplayer));
    }

    private static IEnumerator LoadRoutine(string path, bool isMultiplayer)
    {
        if (!File.Exists(path)) yield break;

        var objectsToDelete = GameObject.FindGameObjectsWithTag("Object");
        foreach (var obj in objectsToDelete)
            GameObject.Destroy(obj);
        if(SceneManager.GetActiveScene().name == "Studio")
            yield return new WaitForSeconds(1f);

        var serializer = new XmlSerializer(typeof(SaveData));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            var saveData = (SaveData)serializer.Deserialize(stream);
            LoadFromSaveData(saveData, isMultiplayer);
        }
    }

    public static GameObject[] LoadFromString(string xml, bool isMultiplayer = false)
    {
        var serializer = new XmlSerializer(typeof(SaveData));
        using (var reader = new StringReader(xml))
        {
            var saveData = (SaveData)serializer.Deserialize(reader);
            return LoadFromSaveData(saveData, isMultiplayer);
        }
    }

    public static void LoadURL(string url, bool isMultiplayer = false)
    {
        RunCoroutine(LoadURLRoutine(url, isMultiplayer));
    }

    private static IEnumerator LoadURLRoutine(string url, bool isMultiplayer)
    {
        var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            yield break;

        SaveData saveData;
        try
        {
            var serializer = new XmlSerializer(typeof(SaveData));
            using (var reader = new StringReader(request.downloadHandler.text))
            {
                saveData = (SaveData)serializer.Deserialize(reader);
            }
        }
        catch
        {
            yield break;
        }

        var objectsToDelete = GameObject.FindGameObjectsWithTag("Object");
        foreach (var obj in objectsToDelete)
            GameObject.Destroy(obj);

        yield return new WaitForSeconds(1f);

        LoadFromSaveData(saveData, isMultiplayer);
    }

    private static GameObject[] LoadFromSaveData(SaveData saveData, bool isMultiplayer)
    {
        var objectMap = new Dictionary<string, GameObject>();
        List<GameObject> spawnedObjects = new List<GameObject>();
        bool hasPlayerDefaults = false;

        foreach (var data in saveData.Objects)
        {
            GameObject obj;

            if (data.Name == "MapCamera")
            {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.name = "MapCamera";
                obj.tag = "MapCamera";
                GameObject.Destroy(obj.GetComponent<Collider>());
                obj.GetComponent<Renderer>().enabled = false;
            }
            else
            {
                obj = DataModel.SpawnClass(data.ClassName);
                if (obj == null) continue;
                obj.name = data.Name;
            }

            if (data.ClassName == "PlayerDefaults")
                hasPlayerDefaults = true;

            objectMap[obj.name] = obj;
            spawnedObjects.Add(obj);
        }

        foreach (var data in saveData.Objects)
        {
            if (!objectMap.TryGetValue(data.Name, out var obj)) continue;

            obj.transform.position = data.Position.ToVector3();
            obj.transform.eulerAngles = data.Rotation.ToVector3();
            obj.transform.localScale = data.Scale.ToVector3();

            if (!string.IsNullOrEmpty(data.ParentName) && objectMap.TryGetValue(data.ParentName, out var parent))
                obj.transform.SetParent(parent.transform, true);
        }

        foreach (var data in saveData.Objects)
        {
            if (!objectMap.TryGetValue(data.Name, out var obj)) continue;

            var luaComp = obj.GetComponent<ScriptInstanceMain>();
            if (luaComp != null && !string.IsNullOrEmpty(data.ScriptContent))
            {
                luaComp.Script = data.ScriptContent;
                luaComp.isLocalScript = data.ClassName == "Script" && data.IsLocalScript;
            }

            if (isMultiplayer && data.Name != "MapCamera")
            {
                bool shouldNotSpawn = false;

                Transform parent = obj.transform.parent;
                Transform grandparent = parent != null ? parent.parent : null;

                if (parent != null && parent.name == "ToolAttachmentPoint" &&
                    obj.TryGetComponent<ObjectClass>(out var objClass) && objClass.className == "Tool" &&
                    grandparent != null && grandparent.name == "PlayerDefaults" &&
                    grandparent.TryGetComponent<ObjectClass>(out var gpClass) && gpClass.className == "PlayerDefaults")
                {
                    shouldNotSpawn = true;
                }

                NetworkIdentity parentNetId = parent ? parent.GetComponent<NetworkIdentity>() : null;
                NetworkIdentity grandparentNetId = grandparent ? grandparent.GetComponent<NetworkIdentity>() : null;

                if ((parentNetId != null && !parentNetId.isServer) || 
                    (grandparentNetId != null && !grandparentNetId.isServer))
                {
                    shouldNotSpawn = true;
                }

                if (shouldNotSpawn)
                {
                    obj.SetActive(true);
                    GameObject.Destroy(obj.GetComponent<NetworkIdentity>());
                    GameObject.Destroy(obj.GetComponent<NetworkTransformUnreliable>());
                }
                else
                {
                    var parentSync = obj.GetComponent<ParentSync>();
                    if (parentSync == null)
                        parentSync = obj.AddComponent<ParentSync>();

                    parentSync.ForceUpdate();
                    if (NetworkServer.active)
                    {
                        NetworkServer.Spawn(obj);
                    }else{
                        continue;
                    }
                }
            }


            if (data.Name == "MapCamera") continue;

            var rb = obj.GetComponent<Rigidbody>();
            if (data.Anchored)
            {
                if (rb != null) GameObject.Destroy(rb);
            }
            else
            {
                if (rb == null) rb = obj.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = data.Gravity;
                rb.constraints = RigidbodyConstraints.None;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.interpolation = RigidbodyInterpolation.None;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                rb.mass = Mathf.Max(0.01f, data.Mass);
            }

            foreach (var col in obj.GetComponents<Collider>())
                col.enabled = data.CanCollide;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.material = new Material(renderer.material);
                renderer.material.color = data.Color.ToColor();
            }

            var colorSync = obj.GetComponent<ColorSync>();
            if (colorSync != null)
                colorSync.SetColor(data.Color.ToColor());

            var classComp = obj.GetComponent<ObjectClass>();
            if (classComp != null)
                classComp.className = data.ClassName;

            var explosion = obj.GetComponent<Explosion>();
            if (explosion != null)
            {
                explosion.radius = data.ExplosionRadius;
                explosion.explosionForce = data.ExplosionForce;
                explosion.upwardsModifier = data.ExplosionUpwardsModifier;
                explosion.massThreshold = data.ExplosionMassThreshold;
            }

            var text3D = obj.GetComponent<Text3DComponent>();
            if(text3D != null)
            {
                    text3D.ChangeText(data.Text3DText ?? "");
                    text3D.ChangeTextColor(data.Color.ToColor());
            }

            var pd = obj.GetComponent<PlayerDefaults>();
            if (pd != null)
            {
                pd.SetWalkSpeed(data.WalkSpeed);
                pd.SetJumpPower(data.JumpPower);
                pd.SetRespawnTime(data.RespawnTime);
            }

            var lightComp = obj.GetComponent<LightComponent>();
            if (lightComp != null)
            {
                lightComp.Rotation = data.LightRotation.ToVector3();
                lightComp.Exposure = data.LightExposure;
                lightComp.Tint = data.LightTint.ToColor();
            }

            var skyComp = obj.GetComponent<Sky>();
            if (skyComp != null)
            {
                skyComp.id = data.SkyID;
            }    

            var soundComp = obj.GetComponent<Sound>();
            if (soundComp != null)
            {
                soundComp.SoundID = data.SoundID;
                soundComp.Autoplay = data.SoundAutoplay;
                soundComp.Volume = data.SoundVolume;
                soundComp.Time = data.SoundTime;
                soundComp.Loop = data.SoundLoop;
                soundComp.PlayInWorld = data.SoundPlayInWorld;
                soundComp.Pitch = data.SoundPitch;

                if (data.SoundIsPlaying)
                {
                    soundComp.Play();
                }
                else
                {
                    soundComp.Stop();
                }
            }

            var teamComp = obj.GetComponent<Team>();
            if (teamComp != null)
            {
                teamComp.TeamName = data.TeamName;
                teamComp.TeamColor = data.TeamColor.ToColor();
            }

            var mComp = obj.GetComponent<MeshComponent>();
            if(mComp != null)
            {
                mComp.meshID = data.MeshID;
            }

            var spComp = obj.GetComponent<SpawnPoint>();
            if (spComp != null)
            {
                spComp.teamName = data.TeamName;
            }

            var nm = obj.GetComponent<NPCMovement>();
            if(nm != null)
            {
                nm.currentBehavior = data.NPCBehaviour;
            }

            var tmpro = obj.GetComponent<TMP_Text>();
            if(tmpro != null)
            {
                tmpro.text = data.Text;
            }

            var intComp = obj.GetComponent<IntValue>();
            if (intComp != null)
                intComp.Value = data.IntValue;

            var stringComp = obj.GetComponent<StringValue>();
            if (stringComp != null)
                stringComp.Value = data.StringValue;

            var boolComp = obj.GetComponent<BoolValue>();
            if (boolComp != null)
                boolComp.Value = data.BoolValue;

            var floatComp = obj.GetComponent<FloatValue>();
            if (floatComp != null)
                floatComp.Value = data.FloatValue;

            var decalComp = obj.GetComponent<Decal>();
            if(decalComp != null)
            {
                decalComp.DecalId = data.DecalId;
                decalComp.Face = data.DecalFace;
            }
        }

        if (!hasPlayerDefaults)
        {
            GameObject defaultPlayer = DataModel.SpawnClass("PlayerDefaults");
            if (defaultPlayer != null)
            {
                defaultPlayer.name = "PlayerDefaults";
                spawnedObjects.Add(defaultPlayer);
                if (isMultiplayer)
                {
                    NetworkServer.Spawn(defaultPlayer);
                }
            }
        }

        return spawnedObjects.ToArray();
    }

    public static IEnumerator SaveToWebsite(string path, string uploadUrl)
    {
        Save(path);

        byte[] fileData = File.ReadAllBytes(path);

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", fileData, Path.GetFileName(path), "text/xml");

        using (UnityWebRequest www = UnityWebRequest.Post(uploadUrl, form))
        {
            yield return www.SendWebRequest();
        }
    }

    public static string SaveModel(GameObject obj)
    {
        var saveData = new SaveData();
        void Add(GameObject o)
        {
            if (o == null) return;
            var classComp = o.GetComponent<ObjectClass>();
            if (classComp == null) return;

            var renderer = o.GetComponent<Renderer>();
            Color objColor = renderer ? renderer.material.color : Color.white;

            string luaScriptText = "";
            bool isLocalScript = false;
            var luaComp = o.GetComponent<ScriptInstanceMain>();
            if (luaComp != null)
            {
                luaScriptText = luaComp.Script;
                isLocalScript = luaComp.isLocalScript;
            }

            var rb = o.GetComponent<Rigidbody>();
            bool anchored = rb == null;
            bool gravity = rb != null ? rb.useGravity : false;
            float mass = rb != null ? rb.mass : 1f;

            var colliders = o.GetComponents<Collider>();
            bool canCollide = true;
            if (colliders.Length > 0)
                canCollide = colliders[0].enabled;

            var explosion = o.GetComponent<Explosion>();
            float explosionRadius = explosion ? explosion.radius : 0f;
            float explosionForce = explosion ? explosion.explosionForce : 0f;
            float upwardsMod = explosion ? explosion.upwardsModifier : 0f;
            float massThreshold = explosion ? explosion.massThreshold : 0f;

            var pd = o.GetComponent<PlayerDefaults>();
            float walkSpeed = 16f, jumpPower = 50f, respawnTime = 1f;
            if (pd != null)
            {
                walkSpeed = pd.walkSpeed;
                jumpPower = pd.jumpPower;
                respawnTime = pd.respawnTime;
            }

            string text3DText = null;
            if (o.GetComponent<ObjectClass>().className == "Text3D")
            {
                var text3Dd = o.GetComponent<Text3DComponent>();
                if (text3Dd != null)
                {
                    text3DText = text3Dd.GetText();
                }
            }

            string npcBehaviour = null;
            if(o.GetComponent<ObjectClass>().className == "NPC")
            {
                npcBehaviour = o.GetComponent<NPCMovement>().currentBehavior;
            }

            var soundComp = o.GetComponent<Sound>();

            int soundID = 0;
            bool soundIsPlaying = false;
            bool soundAutoplay = false;
            float soundVolume = 1f;
            float soundTime = 0f;
            bool soundLoop = false;
            bool soundPlayInWorld = false;
            float soundPitch = 1f;

            if (soundComp != null)
            {
                soundID = soundComp.SoundID;
                soundIsPlaying = soundComp.Playing;
                soundAutoplay = soundComp.Autoplay;
                soundVolume = soundComp.Volume;
                soundTime = soundComp.Time;
                soundLoop = soundComp.Loop;
                soundPlayInWorld = soundComp.PlayInWorld;
                soundPitch = soundComp.Pitch;
            }

            var teamComp = o.GetComponent<Team>();

            Color teamColor = new Color(0, 0, 0);
            string teamName = "";

            if(teamComp != null)
            {
                teamColor = teamComp.TeamColor;
                teamName = teamComp.TeamName;
            }

            var spComp = o.GetComponent<SpawnPoint>();

            if(spComp != null)
            {
                teamName = spComp.teamName;
            }

            var mComp = o.GetComponent<MeshComponent>();

            int meshId = 0;

            if(mComp != null)
            {
                meshId = mComp.meshID;
            }

            string text = null;
            var text3D = o.GetComponent<TMP_Text>();
            if (text3D != null)
                text = o.GetComponent<TMP_Text>().text;

            var lightComp = o.GetComponent<LightComponent>();
            Vector3 lightRotation = Vector3.zero;
            int lightExposure = 0;
            float lightContrast = 0f;
            Color lightTint = Color.white;
            if (lightComp != null)
            {
                lightRotation = lightComp.Rotation;
                lightExposure = Mathf.RoundToInt(lightComp.Exposure);
                lightContrast = 1f;
                lightTint = lightComp.Tint;
            }

            var skyComp = o.GetComponent<Sky>();
            int skyId = 0;
            if(skyComp != null)
            {
                skyId = skyComp.id;
            }

            int intValue = 0;
            string stringValue = "";
            bool boolValue = false;
            float floatValue = 0;

            var valueCompInt = o.GetComponent<IntValue>();
            var valueCompString = o.GetComponent<StringValue>();
            var valueCompBool = o.GetComponent<BoolValue>();
            var valueCompFloat = o.GetComponent<FloatValue>();

            if (valueCompInt != null)
            {
                intValue = valueCompInt.Value;
            }

            if (valueCompString != null)
            {
                stringValue = valueCompString.Value;
            }

            if (valueCompBool != null)
            {
                boolValue = valueCompBool.Value;
            }

            if (valueCompFloat != null)
            {
                floatValue = valueCompFloat.Value;
            }

            int decalId = 0;
            string decalFace = null;

            var decalComp = o.GetComponent<Decal>();

            if(decalComp != null)
            {
                decalId = decalComp.DecalId;
                decalFace = decalComp.Face;
            }

            saveData.Objects.Add(new SavedObjectData
            {
                Name = o.name,
                Position = new Vector3Serializable(o.transform.position),
                Rotation = new Vector3Serializable(o.transform.eulerAngles),
                Scale = new Vector3Serializable(o.transform.localScale),
                Color = new ColorSerializable(objColor),
                ClassName = classComp.className,
                ScriptContent = luaScriptText,
                ParentName = o.transform.parent ? o.transform.parent.name : null,
                Anchored = anchored,
                Gravity = gravity,
                CanCollide = canCollide,
                IsLocalScript = classComp.className == "Script" ? isLocalScript : false,
                Mass = mass,
                ExplosionRadius = explosionRadius,
                ExplosionForce = explosionForce,
                ExplosionUpwardsModifier = upwardsMod,
                ExplosionMassThreshold = massThreshold,
                WalkSpeed = walkSpeed,
                JumpPower = jumpPower,
                RespawnTime = respawnTime,
                Text3DText = text3DText,
                NPCBehaviour = npcBehaviour,
                SoundID = soundID,
                SoundIsPlaying = soundIsPlaying,
                SoundAutoplay = soundAutoplay,
                SoundVolume = soundVolume,
                SoundTime = soundTime,
                SoundLoop = soundLoop,
                SoundPlayInWorld = soundPlayInWorld,
                SoundPitch = soundPitch,
                TeamColor = new ColorSerializable(teamColor),
                TeamName = teamName,
                MeshID = meshId,
                Text = text,
                LightRotation = new Vector3Serializable(lightRotation),
                LightExposure = lightExposure,
                LightContrast = lightContrast,
                LightTint = new ColorSerializable(lightTint),
                SkyID = skyId,
                IntValue = intValue,
                StringValue = stringValue,
                BoolValue = boolValue,
                FloatValue = floatValue,
                DecalId = decalId,
                DecalFace = decalFace
            });

            foreach (Transform child in o.transform)
                Add(child.gameObject);
        }

        Add(obj);

        var serializer = new XmlSerializer(typeof(SaveData));
        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, saveData);
        return stringWriter.ToString();
    }

    public static SaveData Deserialize(string xml)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
        using (StringReader reader = new StringReader(xml))
        {
            return (SaveData)serializer.Deserialize(reader);
        }
    }

    [System.Serializable]
    public struct Vector3Serializable
    {
        public float x, y, z;
        public Vector3Serializable(Vector3 v) { x = v.x; y = v.y; z = v.z; }
        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [System.Serializable]
    public class ColorSerializable
    {
        public float R { get; set; }
        public float G { get; set; }
        public float B { get; set; }
        public float A { get; set; }

        public ColorSerializable() { }

        public ColorSerializable(Color c)
        {
            R = c.r;
            G = c.g;
            B = c.b;
            A = c.a;
        }

        public Color ToColor() => new Color(R, G, B, A);
    }

    public static bool LuaLoadLocal(string path)
    {
        try
        {
            Load(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool LuaLoadURL(string path)
    {
        try
        {
            LoadURL(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}