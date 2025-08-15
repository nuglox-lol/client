using UnityEngine;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Linq;

public class StudioUI : MonoBehaviour
{   
    private bool showSpawnWindow = false;
    private string spawnCategoryInput = "";
    private GameObject selectedObject = null;
    private string selectedObjectName = "";
    private float colorR = 1f, colorG = 1f, colorB = 1f;
    private enum ViewTab { GameView, ScriptEditor }
    private ViewTab activeTab = ViewTab.GameView;
    private bool showScriptEditor = false;
    private string scriptContent = "";
    private GameObject contextTarget = null;
    private bool showInsertFor = false;

    private GameObject makeParentTarget = null;
    private bool showMakeParentPopup = false;

    private bool showExportWindow = false;
    private bool mouseFree = false;

    private bool showSaveChoiceWindow = false;
    private string authKey = null;
    private string uploadPath = null;

    private bool anchored = true;
    private bool gravity = false;

    private bool showImageLoadPopup = false;
    private string imageURL = "";
    private enum ImageSide { Top, Bottom, Right, Left, Front, Back }
    private ImageSide selectedSide = ImageSide.Front;
    private string imageUrl = "";

    void OnEnable() => ImGuiUn.Layout += OnLayout;
    void OnDisable() => ImGuiUn.Layout -= OnLayout;

    async void Start()
    {
        authKey = GetArgs.Get("authkey");
        uploadPath = Application.persistentDataPath + "/UploadFile.npf";

        if (!string.IsNullOrEmpty(authKey))
        {
            string authURL = "https://nuglox.com/placefiles/GetFromAuth.php?auth=" + authKey;
            UnityWebRequest request = UnityWebRequest.Get(authURL);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string finalURL = request.downloadHandler.text.Trim();
                DataService.LoadURL(finalURL);
            }
            else
            {
                Debug.LogError("Failed to get URL from auth: " + request.error);
            }
        }
    }

    void OnLayout()
    {
        var io = ImGui.GetIO();

        if (Input.GetKeyDown(KeyCode.RightControl))
        {
            mouseFree = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (mouseFree && Input.GetKeyDown(KeyCode.LeftControl))
        {
            mouseFree = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (mouseFree)
        {
            var center = new Vector2(io.DisplaySize.x / 2f, io.DisplaySize.y / 2f);
            var text = "Press Left Control to return to editing";
            var size = ImGui.CalcTextSize(text);
            ImGui.SetNextWindowPos(new Vector2(center.x - size.x / 2f, center.y - size.y / 2f));

            ImGui.Begin("MouseFreeInfo", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoBackground);

            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0f, 0f, 0f, 1f));
            ImGui.Text(text);
            ImGui.PopStyleColor();

            ImGui.End();
        }

        float menuBarHeight = 0;
        if (ImGui.BeginMainMenuBar())
        {
            menuBarHeight = ImGui.GetWindowHeight();
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New")) DataService.New();
                if (ImGui.MenuItem("Load")) DataService.Load(Application.persistentDataPath + "/SaveFile.npf");
                if (ImGui.MenuItem("Save"))
                {
                    if (string.IsNullOrEmpty(authKey))
                    {
                        DataService.Save(Application.persistentDataPath + "/SaveFile.npf");
                    }
                    else
                    {
                        showSaveChoiceWindow = true;
                    }
                }
                ImGui.Separator();
                if (ImGui.MenuItem("Exit")) Application.Quit();
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Edit"))
            {
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Object"))
            {
                if (ImGui.MenuItem("Spawn"))
                {
                    showSpawnWindow = true;
                    spawnCategoryInput = "";
                }
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Help"))
            {
                ImGui.MenuItem("About");
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }

        if (showSaveChoiceWindow)
        {
            ImGui.SetNextWindowSize(new Vector2(300, 120), ImGuiCond.Appearing);
            ImGui.Begin("Save Options", ref showSaveChoiceWindow, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
            ImGui.Text("Choose Save Method:");
            if (ImGui.Button("Local Save", new Vector2(120, 0)))
            {
                DataService.Save(Application.persistentDataPath + "/SaveFile.npf");
                showSaveChoiceWindow = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Server Save", new Vector2(120, 0)))
            {
                string url = "https://nuglox.com/upload/game.php?authkey=" + authKey;
                StartCoroutine(DataService.SaveToWebsite(uploadPath, url));
                showSaveChoiceWindow = false;
            }
            ImGui.End();
        }

        if (showSpawnWindow) DrawSpawnWindow();

        if (showMakeParentPopup)
        {
            ImGui.OpenPopup("MakeParentPopup");
            ImGui.SetNextWindowSize(new Vector2(300, 400), ImGuiCond.Appearing);
            if (ImGui.BeginPopup("MakeParentPopup"))
            {
                ImGui.Text($"Make parent of selected object: {makeParentTarget?.name}");
                ImGui.Separator();

                foreach (var candidate in GameObject.FindGameObjectsWithTag("Object"))
                {
                    if (candidate == makeParentTarget) continue;
                    if (IsChildOf(makeParentTarget.transform, candidate.transform)) continue;

                    if (ImGui.Selectable(candidate.name))
                    {
                        candidate.transform.SetParent(makeParentTarget.transform);
                        showMakeParentPopup = false;
                        ImGui.CloseCurrentPopup();
                    }
                }

                if (ImGui.Button("Cancel"))
                {
                    showMakeParentPopup = false;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
            else
            {
                showMakeParentPopup = false;
            }
        }

        if (showInsertFor && contextTarget != null)
        {
            spawnCategoryInput = "";
            showSpawnWindow = true;
        }

        if (showExportWindow)
        {
            ImGui.SetNextWindowSize(new Vector2(320, 120), ImGuiCond.Appearing);
            ImGui.Begin("Export to BRIKZ Workshop", ref showExportWindow, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);
            ImGui.Text("Export to BRIKZ Workshop");
            ImGui.Spacing();
            if (ImGui.Button("Go!", new Vector2(120, 0))) showExportWindow = false;
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(120, 0))) showExportWindow = false;
            ImGui.End();
        }

        float windowWidth = io.DisplaySize.x;
        float windowHeight = io.DisplaySize.y - menuBarHeight;
        float panelWidth = windowWidth * 0.25f;
        float centerWidth = windowWidth * 0.5f;

        ImGui.SetNextWindowPos(new Vector2(0, menuBarHeight));
        ImGui.SetNextWindowSize(new Vector2(panelWidth, windowHeight));
        ImGui.Begin("Explorer", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize);
        ImGui.BeginChild("ExplorerList", new Vector2(0, 0), true);
        foreach (var go in GameObject.FindGameObjectsWithTag("Object"))
        {
            if (go.transform.parent != null) continue;
            DrawObjectHierarchy(go);
        }
        ImGui.EndChild();
        ImGui.End();

        ImGui.SetNextWindowPos(new Vector2(panelWidth, menuBarHeight));
        ImGui.SetNextWindowSize(new Vector2(centerWidth, windowHeight));
        ImGui.Begin("CenterPanel", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoBackground);
        if (ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Game View"))
            {
                activeTab = ViewTab.GameView;
                ImGui.EndTabItem();
            }
            if (showScriptEditor && ImGui.BeginTabItem("Script Editor"))
            {
                activeTab = ViewTab.ScriptEditor;
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        ImGui.End();

        if (activeTab == ViewTab.GameView)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 0, 0));
            ImGui.SetNextWindowPos(new Vector2(panelWidth, menuBarHeight + 28));
            ImGui.SetNextWindowSize(new Vector2(centerWidth, windowHeight - 28));
            ImGui.Begin("GameView", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoResize);
            ImGui.End();
            ImGui.PopStyleColor();
        }
        else if (activeTab == ViewTab.ScriptEditor)
        {
            ImGui.SetNextWindowPos(new Vector2(panelWidth, menuBarHeight + 28));
            ImGui.SetNextWindowSize(new Vector2(centerWidth, windowHeight - 28));
            ImGui.Begin("Editor", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
            ImGui.InputTextMultiline("##ScriptContent", ref scriptContent, 10240, new Vector2(centerWidth - 16, windowHeight - 64));
            if (selectedObject != null)
            {
                var scriptComp = selectedObject.GetComponent<ScriptInstanceMain>();
                if (scriptComp != null) scriptComp.Script = scriptContent;
            }
            ImGui.End();
        }

        ImGui.SetNextWindowPos(new Vector2(panelWidth + centerWidth, menuBarHeight));
        ImGui.SetNextWindowSize(new Vector2(panelWidth, windowHeight));
        ImGui.Begin("Properties", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
        ImGui.BeginChild("Props", new Vector2(0, 0), true);

        if (selectedObject != null)
        {
            ImGui.Text("Object Properties");
            ImGui.Spacing();

            if (ImGui.InputText("Name", ref selectedObjectName, 256))
            {
                string uniqueName = selectedObjectName;
                selectedObject.name = uniqueName;
                selectedObjectName = uniqueName;
            }

            var pos = selectedObject.transform.position;
            if (ImGui.DragFloat3("Position", ref pos)) selectedObject.transform.position = pos;

            var rot = selectedObject.transform.eulerAngles;
            if (ImGui.DragFloat3("Rotation", ref rot, 1f)) selectedObject.transform.rotation = Quaternion.Euler(rot);

            var scale = selectedObject.transform.localScale;
            if (ImGui.DragFloat3("Scale", ref scale, 0.1f)) selectedObject.transform.localScale = scale;

            ImGui.Separator();
            ImGui.Text("Color");

            var colorVec = new Vector3(colorR, colorG, colorB);
            if (ImGui.ColorEdit3("Color", ref colorVec))
            {
                colorR = colorVec.x; colorG = colorVec.y; colorB = colorVec.z;
                UpdateSelectedObjectColor();
            }

            int r = Mathf.Clamp((int)(colorR * 255), 0, 255);
            int g = Mathf.Clamp((int)(colorG * 255), 0, 255);
            int b = Mathf.Clamp((int)(colorB * 255), 0, 255);

            if (ImGui.InputInt("R", ref r)) { colorR = Mathf.Clamp01(r / 255f); UpdateSelectedObjectColor(); }
            if (ImGui.InputInt("G", ref g)) { colorG = Mathf.Clamp01(g / 255f); UpdateSelectedObjectColor(); }
            if (ImGui.InputInt("B", ref b)) { colorB = Mathf.Clamp01(b / 255f); UpdateSelectedObjectColor(); }

            ImGui.Separator();
            ImGui.Text("Physics");

            var rb = selectedObject.GetComponent<Rigidbody>();
            anchored = rb == null;

            if (ImGui.Checkbox("Anchored", ref anchored))
            {
                rb = selectedObject.GetComponent<Rigidbody>();

                if (anchored)
                {
                    if (rb != null)
                        GameObject.Destroy(rb);
                }
                else
                {
                    if (rb == null)
                        rb = selectedObject.AddComponent<Rigidbody>();

                    rb.isKinematic = false;
                    rb.useGravity = gravity;
                    rb.constraints = RigidbodyConstraints.None;
                }
            }

            if (!anchored)
            {
                rb = selectedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    if (ImGui.Checkbox("Gravity", ref gravity))
                    {
                        rb.useGravity = gravity;
                    }

                    float mass = rb.mass;
                    if (ImGui.DragFloat("Mass", ref mass, 0.1f, 0.01f, 1000f))
                    {
                        rb.mass = Mathf.Max(0.01f, mass);
                    }
                }
            }

            bool canCollide = true;
            var colliders = selectedObject.GetComponents<Collider>();
            if (colliders.Length > 0)
            {
                canCollide = colliders[0].enabled;

                if (ImGui.Checkbox("Can Collide", ref canCollide))
                {
                    foreach (var col in colliders)
                        col.enabled = canCollide;
                }
            }
            else
            {
                bool dummy = false;
                ImGui.Checkbox("Can Collide (No Collider)", ref dummy);
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "Text3D")
            {
                ImGui.Separator();
                ImGui.Text("Text3D Settings");

                var text3D = selectedObject.GetComponent<Text3DComponent>();
                var text3DInput = selectedObject.GetComponent<Text3DComponent>().GetText() ?? "Label";

                if (text3D != null)
                {
                    text3DInput = text3D.GetText();
                    if (ImGui.InputText("Text", ref text3DInput, 256))
                    {
                        text3D.ChangeText(text3DInput);
                    }
                }
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "Explosion")
            {
                ImGui.Separator();
                ImGui.Text("Explosion Settings");

                var explosion = selectedObject.GetComponent<Explosion>();
                if (explosion != null)
                {
                    float radius = explosion.radius;
                    if (ImGui.DragFloat("Radius", ref radius, 0.1f, 0f, 100f))
                        explosion.radius = radius;

                    float force = explosion.explosionForce;
                    if (ImGui.DragFloat("Force", ref force, 10f, 0f, 10000f))
                        explosion.explosionForce = force;

                    float upwards = explosion.upwardsModifier;
                    if (ImGui.DragFloat("Upwards Modifier", ref upwards, 0.1f, -10f, 10f))
                        explosion.upwardsModifier = upwards;

                    float threshold = explosion.massThreshold;
                    if (ImGui.DragFloat("Mass Threshold", ref threshold, 1f, 0f, 1000f))
                        explosion.massThreshold = threshold;
                }
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "Script")
            {
                ImGui.Separator();
                ImGui.Text("Script Management");

                var scriptInstance = selectedObject.GetComponent<ScriptInstanceMain>();
                if (scriptInstance != null)
                {
                    bool localscriptEnabled = scriptInstance.isLocalScript;
                    if (ImGui.Checkbox("Enable Local Script", ref localscriptEnabled))
                    {
                        scriptInstance.isLocalScript = localscriptEnabled;
                    }
                }
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "PlayerDefaults")
            {
                var playerDefaults = selectedObject.GetComponent<PlayerDefaults>();
                ImGui.Separator();
                ImGui.Text("PlayerDefaults Settings");

                float health = playerDefaults.GetMaxHealth();
                if (ImGui.DragFloat("Max Health", ref health, 1f, 1f, 1000f)) playerDefaults.SetMaxHealth(health);

                float walk = playerDefaults.GetWalkSpeed();
                if (ImGui.DragFloat("WalkSpeed", ref walk, 0.1f, 0f, 100f)) playerDefaults.SetWalkSpeed(walk);

                float jump = playerDefaults.GetJumpPower();
                if (ImGui.DragFloat("Jump Power", ref jump, 0.1f, 0f, 100f)) playerDefaults.SetJumpPower(jump);

                float respawn = playerDefaults.GetRespawnTime();
                if (ImGui.DragFloat("Respawn Time", ref respawn, 0.1f, 0f, 60f)) playerDefaults.SetRespawnTime(respawn);
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "Sound")
            {
                var sound = selectedObject.GetComponent<Sound>();

                ImGui.Separator();
                ImGui.Text("Sound Settings");
                ImGui.Text("");

                int soundID = sound.SoundID;
                if (ImGui.InputInt("Sound ID", ref soundID))
                {
                    sound.SoundID = soundID;
                }

                bool autoplay = sound.Autoplay;
                if (ImGui.Checkbox("Autoplay", ref autoplay))
                {
                    sound.Autoplay = autoplay;
                }

                bool loop = sound.Loop;
                if (ImGui.Checkbox("Loop", ref loop))
                {
                    sound.Loop = loop;
                }

                bool playInWorld = sound.PlayInWorld;
                if (ImGui.Checkbox("Play In World", ref playInWorld))
                {
                    sound.PlayInWorld = playInWorld;
                }

                float volume = sound.Volume;
                if (ImGui.SliderFloat("Volume", ref volume, 0f, 1f))
                {
                    sound.Volume = volume;
                }

                float pitch = sound.Pitch;
                if (ImGui.SliderFloat("Pitch", ref pitch, 0.5f, 2f))
                {
                    sound.Pitch = pitch;
                }
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "Team")
            {
                var team = selectedObject.GetComponent<Team>();

                string teamName = team.TeamName;
                if (ImGui.InputText("Team Name", ref teamName, 64)) team.TeamName = teamName;

                Color teamColor = team.TeamColor;
                Vector3 colorVecTEAM = new Vector3(teamColor.r, teamColor.g, teamColor.b);

                if (ImGui.ColorEdit3("Team Color", ref colorVecTEAM))
                {
                    team.TeamColor = new Color(colorVecTEAM.x, colorVecTEAM.y, colorVecTEAM.z, 1f);
                }
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "SpawnPoint")
            {
                var sp = selectedObject.GetComponent<SpawnPoint>();

                string teamName = sp.teamName;
                if (ImGui.InputText("Team Name", ref teamName, 64)) sp.teamName = teamName;
            }

            if (selectedObject.GetComponent<ObjectClass>().className == "Mesh")
            {
                var mc = selectedObject.GetComponent<MeshComponent>();

                int meshID = mc.meshID;
                if (ImGui.InputInt("MeshID", ref meshID, 64)) mc.meshID = meshID;
            }

            if (selectedObject.transform.parent != null)
            {
                if (ImGui.Button("Unparent")) selectedObject.transform.SetParent(null);
            }
        }
        else
        {
            ImGui.Text("No object selected");
        }

        ImGui.EndChild();
        ImGui.End();

        io.WantCaptureMouse = ImGui.IsWindowHovered() || ImGui.IsAnyItemActive();
        io.WantCaptureKeyboard = io.WantCaptureMouse;
    }

    private IEnumerator LoadImage(string url, ImageSide side)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            if (selectedObject != null)
            {
                Vector3 localPos = Vector3.zero;
                Vector3 localRot = Vector3.zero;
                Vector3 localScale = Vector3.one;

                Vector3 size = Vector3.one;
                var meshFilter = selectedObject.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    size = meshFilter.sharedMesh.bounds.size;
                    size = Vector3.Scale(size, selectedObject.transform.localScale);
                }
                else
                {
                    size = selectedObject.transform.localScale;
                }

                switch (side)
                {
                    case ImageSide.Top:
                        localPos = new Vector3(0, size.y / 2f, 0);
                        localRot = new Vector3(90, 0, 0);
                        localScale = new Vector3(size.x, size.z, 1);
                        break;
                    case ImageSide.Bottom:
                        localPos = new Vector3(0, -size.y / 2f, 0);
                        localRot = new Vector3(-90, 0, 0);
                        localScale = new Vector3(size.x, size.z, 1);
                        break;
                    case ImageSide.Left:
                        localPos = new Vector3(-size.x / 2f, 0, 0);
                        localRot = new Vector3(0, 90, 0);
                        localScale = new Vector3(size.z, size.y, 1);
                        break;
                    case ImageSide.Right:
                        localPos = new Vector3(size.x / 2f, 0, 0);
                        localRot = new Vector3(0, -90, 0);
                        localScale = new Vector3(size.z, size.y, 1);
                        break;
                    case ImageSide.Front:
                        localPos = new Vector3(0, 0, size.z / 2f);
                        localRot = Vector3.zero;
                        localScale = new Vector3(size.x, size.y, 1);
                        break;
                    case ImageSide.Back:
                        localPos = new Vector3(0, 0, -size.z / 2f);
                        localRot = new Vector3(0, 180, 0);
                        localScale = new Vector3(size.x, size.y, 1);
                        break;
                }

                string faceName = side.ToString() + "Face";
                Transform faceTransform = selectedObject.transform.Find(faceName);

                if (faceTransform == null)
                {
                    GameObject faceObject = new GameObject(faceName);
                    faceObject.transform.SetParent(selectedObject.transform);
                    faceObject.transform.localPosition = localPos;
                    faceObject.transform.localEulerAngles = localRot;
                    faceObject.transform.localScale = localScale;

                    MeshFilter mf = faceObject.AddComponent<MeshFilter>();
                    MeshRenderer mr = faceObject.AddComponent<MeshRenderer>();

                    mf.mesh = CreateQuadMesh();

                    Shader standardShader = Shader.Find("Standard");
                    if (standardShader == null)
                    {
                        Debug.LogError("Standard shader not found!");
                        yield break;
                    }

                    Material mat = new Material(standardShader);
                    mat.mainTexture = texture;
                    mr.material = mat;
                }
                else
                {
                    faceTransform.localPosition = localPos;
                    faceTransform.localEulerAngles = localRot;
                    faceTransform.localScale = localScale;

                    Renderer renderer = faceTransform.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Material mat = renderer.material;
                        mat.mainTexture = texture;
                        renderer.material = mat;
                    }
                    else
                    {
                        MeshRenderer mr = faceTransform.gameObject.AddComponent<MeshRenderer>();
                        Shader standardShader = Shader.Find("Standard");
                        if (standardShader == null)
                        {
                            Debug.LogError("Standard shader not found!");
                            yield break;
                        }
                        Material mat = new Material(standardShader);
                        mat.mainTexture = texture;
                        mr.material = mat;

                        if (faceTransform.GetComponent<MeshFilter>() == null)
                            faceTransform.gameObject.AddComponent<MeshFilter>().mesh = CreateQuadMesh();
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Image download failed: " + www.error);
        }
    }

    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0)
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        mesh.triangles = new int[]
        {
            0, 2, 1,
            0, 3, 2
        };
        mesh.RecalculateNormals();
        return mesh;
    }

    void DrawObjectHierarchy(GameObject go)
    {
        if (go.tag != "Object") return;

        bool hasChildren = false;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).gameObject.tag == "Object")
            {
                hasChildren = true;
                break;
            }
        }

        ImGuiTreeNodeFlags nodeFlags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;
        if (selectedObject == go) nodeFlags |= ImGuiTreeNodeFlags.Selected;
        if (!hasChildren) nodeFlags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

        bool nodeOpen = ImGui.TreeNodeEx(go.GetInstanceID().ToString(), nodeFlags, go.name);
        if (ImGui.IsItemClicked())
        {
            selectedObject = go;
            selectedObjectName = go.name;
            var col = GetObjectColor(go);
            colorR = col.r; colorG = col.g; colorB = col.b;
            var script = go.GetComponent<ScriptInstanceMain>();
            if (script != null)
            {
                showScriptEditor = true;
                scriptContent = script.Script;
            }
            else
            {
                showScriptEditor = false;
                scriptContent = "";
            }
        }

        if (ImGui.BeginPopupContextItem())
        {
            if (ImGui.MenuItem("Delete"))
            {
                if (selectedObject == go)
                {
                    selectedObject = null;
                    selectedObjectName = "";
                    showScriptEditor = false;
                    scriptContent = "";
                }
                Destroy(go);
            }
            if (ImGui.MenuItem("Rename"))
            {
                selectedObject = go;
                selectedObjectName = go.name;
            }
            if (ImGui.MenuItem("Export"))
            {
                showExportWindow = true;
            }
            if (ImGui.MenuItem("Make Parent Of..."))
            {
                makeParentTarget = go;
                showMakeParentPopup = true;
            }
            if (ImGui.MenuItem("Insert"))
            {
                contextTarget = go;
                showInsertFor = true;
            }
            ImGui.EndPopup();
        }

        if (hasChildren && nodeOpen)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                var child = go.transform.GetChild(i).gameObject;
                if (child.tag == "Object")
                    DrawObjectHierarchy(child);
            }
            ImGui.TreePop();
        }
    }

    void DrawSpawnWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(400, 300), ImGuiCond.Appearing);
        ImGui.Begin("Spawn Class", ref showSpawnWindow, ImGuiWindowFlags.NoCollapse);

        ImGui.Text("Click to spawn an object:");

        foreach (string className in DataModel.AllowedClassNames)
        {
            if (ImGui.Button(className, new Vector2(200, 0)))
            {
                GameObject spawned = DataModel.SpawnClass(className);

                if (showInsertFor && contextTarget != null && spawned != null)
                    spawned.transform.SetParent(contextTarget.transform);

                showInsertFor = false;
                contextTarget = null;
                showSpawnWindow = false;
                break;
            }
        }

        ImGui.Separator();

        if (ImGui.Button("Cancel", new Vector2(120, 0)))
        {
            showInsertFor = false;
            contextTarget = null;
            showSpawnWindow = false;
        }

        ImGui.End();
    }

    void UpdateSelectedObjectColor()
    {
        if (selectedObject == null) return;
        var rend = selectedObject.GetComponent<Renderer>();
        if (rend == null) return;
        rend.material.color = new Color(colorR, colorG, colorB);
    }

    Color GetObjectColor(GameObject go)
    {
        var rend = go.GetComponent<Renderer>();
        if (rend != null) return rend.material.color;
        return Color.white;
    }

    bool IsChildOf(Transform child, Transform parent)
    {
        if (child == null || parent == null) return false;
        while (child.parent != null)
        {
            if (child.parent == parent) return true;
            child = child.parent;
        }
        return false;
    }
}
