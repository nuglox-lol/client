using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using Mirror;

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

            var colliders = obj.GetComponents<Collider>();
            bool canCollide = true;
            if (colliders.Length > 0)
                canCollide = colliders[0].enabled;

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
                IsLocalScript = classComp.className == "Script" ? isLocalScript : false
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
                IsLocalScript = false
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

    public static void LoadURL(string url, bool isMultiplayer = false)
    {
        RunCoroutine(LoadURLRoutine(url, isMultiplayer));
    }

    private static IEnumerator LoadURLRoutine(string url, bool isMultiplayer)
    {
        var request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Failed to load URL: " + request.error);
            yield break;
        }

        SaveData saveData;
        try
        {
            var serializer = new XmlSerializer(typeof(SaveData));
            using (var reader = new StringReader(request.downloadHandler.text))
            {
                saveData = (SaveData)serializer.Deserialize(reader);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to deserialize data: " + ex.Message);
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

        foreach (var data in saveData.Objects)
        {
            if (data.Name == "MapCamera")
            {
                GameObject camCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                camCube.name = "MapCamera";
                GameObject.Destroy(camCube.GetComponent<Collider>());
                camCube.GetComponent<Renderer>().enabled = false;
                camCube.transform.position = data.Position.ToVector3();
                camCube.transform.eulerAngles = data.Rotation.ToVector3();
                camCube.transform.localScale = data.Scale.ToVector3();
                objectMap[camCube.name] = camCube;
                spawnedObjects.Add(camCube);
                continue;
            }

            GameObject obj = DataModel.SpawnClass(data.ClassName);
            if (obj == null) continue;

            obj.name = data.Name;
            obj.transform.position = data.Position.ToVector3();
            obj.transform.eulerAngles = data.Rotation.ToVector3();
            obj.transform.localScale = data.Scale.ToVector3();

            if (isMultiplayer) NetworkServer.Spawn(obj);

            var rb = obj.GetComponent<Rigidbody>();

            if (data.Anchored)
            {
                if (rb != null)
                    GameObject.Destroy(rb);
            }
            else
            {
                if (rb == null)
                    rb = obj.AddComponent<Rigidbody>();

                rb.isKinematic = false;
                rb.useGravity = data.Gravity;
                rb.constraints = RigidbodyConstraints.None;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.interpolation = RigidbodyInterpolation.None;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            }

            var colliders = obj.GetComponents<Collider>();
            if (colliders.Length > 0)
            {
                foreach (var col in colliders)
                    col.enabled = data.CanCollide;
            }

            var renderer = obj.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.material = new Material(renderer.material);
                renderer.material.color = data.Color.ToColor();
            }

            if (obj.GetComponent<ColorSync>() != null)
                obj.GetComponent<ColorSync>().SetColor(data.Color.ToColor());

            var classComp = obj.GetComponent<ObjectClass>();
            if (classComp != null)
                classComp.className = data.ClassName;

            var luaComp = obj.GetComponent<ScriptInstanceMain>();
            if (luaComp != null && !string.IsNullOrEmpty(data.ScriptContent))
            {
                luaComp.Script = data.ScriptContent;
                if (data.ClassName == "Script")
                    luaComp.isLocalScript = data.IsLocalScript;
                else
                    luaComp.isLocalScript = false;
            }

            objectMap[obj.name] = obj;
            spawnedObjects.Add(obj);
        }

        foreach (var data in saveData.Objects)
        {
            if (data.Name == "MapCamera") continue;

            if (!string.IsNullOrEmpty(data.ParentName) && objectMap.ContainsKey(data.Name) && objectMap.ContainsKey(data.ParentName))
            {
                objectMap[data.Name].transform.SetParent(objectMap[data.ParentName].transform);
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

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Upload failed: " + www.error);
            }
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