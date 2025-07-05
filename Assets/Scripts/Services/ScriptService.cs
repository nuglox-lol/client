using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using MoonSharp.Interpreter;
using UnityEngine;
using System.Linq;

public class ScriptService : NetworkBehaviour
{
    public string scriptResult;
    public Script script;
    private UnityEngine.Coroutine luaCoroutine;
    private DynValue luaMainCoroutine;
    private Dictionary<string, Table> trackedTables = new Dictionary<string, Table>();
    private Table workspaceTable;
    private Table playersTable;
    private LuaEvent playersAddedEvent;
    private LuaEvent playersRemovedEvent;
    private int lastWorkspaceCount;
    private bool workspaceInitialized = false;
    private Dictionary<string, InstanceDatamodel> trackedInstances = new();
    private HashSet<string> currentFrameKeys = new();

    [Command]
    public void CmdRunCodeOnServer(string luaCode)
    {
        Debug.Log("Received Lua code from client, scheduling to run on server when ready.");

        StartCoroutine(RunScriptWhenReady(luaCode));
    }

    public void Init(Dictionary<string, object> globals = null)
    {
        script = new Script();
        UserData.RegisterAssembly();
        UserData.RegisterType<GameObject>();
        UserData.RegisterType<Transform>();
        UserData.RegisterType<Vector4>();
        UserData.RegisterType<Vector3>();
        UserData.RegisterType<Vector2>();
        UserData.RegisterType<Color>();
        UserData.RegisterType<ColorSync>();
        UserData.RegisterType<LuaVector2>();
        UserData.RegisterType<LuaVector3>();
        UserData.RegisterType<LuaVector4>();
        UserData.RegisterType<LuaQuaternion>();
        UserData.RegisterType<LuaThumbnailGenerator>();
        UserData.RegisterType<LuaModelService>();
        UserData.RegisterType<LuaDataService>();
        UserData.RegisterType<LuaColor3>();
        UserData.RegisterType<LuaEvent>();
        UserData.RegisterType<InstanceDatamodel>();
        UserData.RegisterType<InstanceRigidbody>();
        UserData.RegisterType<InstancePlayer>();
        UserData.RegisterType<TouchedHandler>();
        UserData.RegisterType<LuaUIService>();
        UserData.RegisterType<LuaImGui>();
        UserData.RegisterType<LuaTweenService>();

        DynValue instanceNewFunc = DynValue.NewCallback((context, args) =>
        {
            string className = args.Count > 0 && args[0].Type == DataType.String ? args[0].String : "Part";
            GameObject go = DataModel.SpawnClass(className);
            InstanceDatamodel instance = new InstanceDatamodel(go, context.GetScript());
            return UserData.Create(instance);
        });

        Table instanceTable = new Table(script);
        instanceTable["New"] = instanceNewFunc;
        script.Globals["Instance"] = instanceTable;

        script.Globals["Vector2"] = typeof(LuaVector2);
        script.Globals["Vector3"] = typeof(LuaVector3);
        script.Globals["Vector4"] = typeof(LuaVector4);
        script.Globals["Quaternion"] = typeof(LuaQuaternion);
        script.Globals["ThumbnailGenerator"] = typeof(LuaThumbnailGenerator);
        script.Globals["ModelService"] = typeof(LuaModelService);
        script.Globals["UIService"] = typeof(LuaUIService);
        script.Globals["DataService"] = typeof(LuaDataService);
        script.Globals["Color"] = typeof(LuaColor3);
        script.Globals["print"] = new Action<object>(PrintToConsole);
        script.Globals["ImGui"] = typeof(LuaImGui);
        script.Globals["TweenService"] = typeof(LuaTweenService);
        script.Globals["wait"] = (Func<DynValue, DynValue>)((seconds) =>
        {
            double time = seconds.Number;
            return DynValue.NewYieldReq(new DynValue[]
            {
                DynValue.NewNumber(time),
                DynValue.NewString("wait")
            });
        });

        GameObject gameObject = base.gameObject;
        InstanceDatamodel o = null;
        bool flag = false;

        try
        {
            if (gameObject != null)
            {
                ObjectClass oc = gameObject.GetComponent<ObjectClass>();
                if (oc != null && !string.IsNullOrEmpty(oc.className))
                {
                    switch (oc.className)
                    {
                        case "Tool":
                            o = new InstanceTool(gameObject, script);
                            break;
                        case "Player":
                            o = new InstancePlayer(gameObject);
                            break;
                        default:
                            o = new InstanceDatamodel(gameObject);
                            break;
                    }
                }
                else
                {
                    o = new InstanceDatamodel(gameObject);
                }

                if (o != null)
                {
                    script.Globals["script"] = UserData.Create(o);
                    flag = true;
                }
            }
        }
        catch { }

        if (!flag)
        {
            script.Globals["script"] = DynValue.Nil;
        }

        GameObject gameObject2 = base.gameObject.transform.parent?.gameObject;
        InstanceDatamodel o2 = null;
        bool flag2 = false;

        try
        {
            if (gameObject2 != null)
            {
                ObjectClass oc2 = gameObject2.GetComponent<ObjectClass>();
                if (oc2 != null && !string.IsNullOrEmpty(oc2.className))
                {
                    switch (oc2.className)
                    {
                        case "Tool":
                            o2 = new InstanceTool(gameObject2, script);
                            break;
                        case "Player":
                            o2 = new InstancePlayer(gameObject2);
                            break;
                        default:
                            o2 = new InstanceDatamodel(gameObject2);
                            break;
                    }
                }
                else
                {
                    o2 = new InstanceDatamodel(gameObject2);
                }

                if (o2 != null)
                {
                    script.Globals["scriptParent"] = UserData.Create(o2);
                    flag2 = true;
                }
            }
        }
        catch { }

        if (!flag2)
        {
            script.Globals["scriptParent"] = DynValue.Nil;
        }

        Table gameTable = new Table(script);
        workspaceTable = new Table(script);
        gameTable.Set("Workspace", DynValue.NewTable(workspaceTable));
        playersTable = new Table(script);
        gameTable.Set("Players", DynValue.NewTable(playersTable));

        playersAddedEvent = new LuaEvent(base.gameObject, script);
        playersRemovedEvent = new LuaEvent(base.gameObject, script);
        playersTable.Set("Added", UserData.Create(playersAddedEvent));
        playersTable.Set("Removed", UserData.Create(playersRemovedEvent));

        playersTable.Set("GetByUsername", DynValue.NewCallback((c, args) =>
        {
            string name = args[0].String;
            foreach (var kv in playersTable.Pairs)
            {
                InstancePlayer p = kv.Value.ToObject<InstancePlayer>();
                if (p.Username == name)
                    return kv.Value;
            }
            return DynValue.Nil;
        }));

        playersTable.Set("GetByID", DynValue.NewCallback((c, args) =>
        {
            string id = args[0].String;
            if (int.TryParse(id, out int userId))
            {
                foreach (var kv in playersTable.Pairs)
                {
                    InstancePlayer p = kv.Value.ToObject<InstancePlayer>();
                    if (p.UserId == userId)
                        return kv.Value;
                }
            }
            return DynValue.Nil;
        }));

        playersTable.Set("GetByNetID", DynValue.NewCallback((c, args) =>
        {
            string netid = args[0].String;
            if (playersTable.Get(netid).IsNotNil())
                return playersTable.Get(netid);
            return DynValue.Nil;
        }));

        gameTable.Set("AlertAll", DynValue.NewCallback((c, args) =>
        {
            string msg = args[0].CastToString();
            float duration = (float)(args.Count > 1 && args[1].Type == DataType.Number ? args[1].Number : 3f);

            Debug.Log($"AlertAll called on server by '{gameObject.name}' sending message: \"{msg}\" for {duration} seconds");

            if (netIdentity != null && netIdentity.observers != null)
            {
                foreach (var conn in netIdentity.observers.Values)
                {
                    Debug.Log($"Observer connection: {conn.connectionId}");
                }

            }
            else
            {
                Debug.LogWarning("NetIdentity or its observers list is null!");
            }
            
            if (isServer)
            {
                RpcShowAlert(msg, duration);
            }
            return DynValue.Nil;
        }));

        gameTable.Set("Alert", DynValue.NewCallback((c, args) =>
        {
            string msg = args[0].CastToString();
            float duration = (float)(args.Count > 1 && args[1].Type == DataType.Number ? args[1].Number : 3f);
            string netId = args.Count > 2 ? args[2].CastToString() : null;

            if (playersTable != null && netId != null && playersTable.Get(netId).Type == DataType.UserData)
            {
                var userDataObj = playersTable.Get(netId).UserData.Object;
                var gameObjProp = userDataObj.GetType().GetProperty("gameObject");

                if (gameObjProp != null)
                {
                    var gameObj = gameObjProp.GetValue(userDataObj) as GameObject;
                    if (gameObj != null)
                    {
                        var identity = gameObj.GetComponent<NetworkIdentity>();
                        if (identity != null && identity.connectionToClient != null)
                        {
                            TargetShowAlert(identity.connectionToClient, msg, duration);
                        }
                    }
                }
            }

            return DynValue.Nil;
        }));

        script.Globals["Game"] = DynValue.NewTable(gameTable);
        trackedTables["Game"] = gameTable;

        if (globals != null)
        {
            foreach (var global in globals)
            {
                DynValue dynValue = ToDynValue(script, global.Value);
                script.Globals[global.Key] = dynValue;
                if (dynValue.Type == DataType.Table)
                {
                    trackedTables[global.Key] = dynValue.Table;
                }
            }
        }

        script.Globals["SendCodeToServer"] = (Action<string>)((code) =>
        {
            if (!isLocalPlayer)
            {
                Debug.LogWarning("SendCodeToServer can only be called by the local player.");
                return;
            }

            CmdRunCodeOnServer(code);
        });
    }

    private void Start()
    {
        InvokeRepeating("UpdateWorkspaceTable", 0f, 1f);
        Debug.Log($"{gameObject.name} netId: {netIdentity.netId}");
    }

    public IEnumerator RunScriptWhenReady(string luaCode)
    {
        while (!workspaceInitialized)
            yield return null;

        while (workspaceTable == null || !workspaceTable.Keys.Any() || workspaceTable.Keys.Count() < lastWorkspaceCount)
            yield return null;

        RunScript(luaCode);
    }

    public void RunScript(string luaCode, Action<string> onComplete = null)
    {
        if (luaCoroutine != null)
            StopCoroutine(luaCoroutine);

        string wrappedCode = $"return coroutine.create(function()\n{luaCode}\nend)";
        luaMainCoroutine = script.DoString(wrappedCode);
        luaCoroutine = StartCoroutine(LuaCoroutineRunner(onComplete));
    }

    [TargetRpc]
    public void TargetShowAlert(NetworkConnection target, string message, float duration)
    {
        ShowAlert(message, duration);
    }

    [ClientRpc]
    public void RpcShowAlert(string message, float duration)
    {
        Debug.Log("RAZIEAE");
        ShowAlert(message, duration);
    }

    private void ShowAlert(string message, float duration)
    {
        var coreGui = GameObject.Find("CoreGui");
        if (coreGui == null) return;

        var alertPanel = coreGui.transform.Find("AlertPanel");
        var alertText = alertPanel?.Find("AlertMessage")?.GetComponent<TMPro.TextMeshProUGUI>();

        if (alertPanel == null || alertText == null) return;

        alertText.text = message;
        alertPanel.gameObject.SetActive(true);

        StartCoroutine(HideAlertAfterDelay(alertPanel.gameObject, duration));
    }

    private IEnumerator HideAlertAfterDelay(GameObject panel, float delay)
    {
        yield return new WaitForSeconds(delay);
        panel.SetActive(false);
    }

    private IEnumerator LuaCoroutineRunner(Action<string> onComplete)
    {
        DynValue coroutine = luaMainCoroutine;
        DynValue lastResult = null;

        while (coroutine.Coroutine.State != CoroutineState.Dead)
        {
            DynValue result = coroutine.Coroutine.Resume();
            lastResult = result;

            if (result.Type == DataType.Tuple && result.Tuple.Length >= 2)
            {
                DynValue t = result.Tuple[0];
                if (t.Type == DataType.Number)
                    yield return new WaitForSeconds((float)t.Number);
                else
                    yield return null;
            }
            else
            {
                yield return null;
            }
        }

        string resultString = (lastResult != null && lastResult.Type != DataType.Void && lastResult.Type != DataType.Nil)
            ? lastResult.ToPrintString()
            : "nil";

        onComplete?.Invoke(resultString);
        luaCoroutine = null;
    }

    public void SetGlobal(string path, object value)
    {
        string[] array = path.Split('.');
        if (array.Length < 2) return;
        string text = array[0];
        if (!trackedTables.TryGetValue(text, out var value2)) return;
        for (int i = 1; i < array.Length - 1; i++)
        {
            value2 = value2.Get(array[i]).Table;
            if (value2 == null) return;
        }
        string key = array[^1];
        value2[key] = ToDynValue(script, value);
    }

    private DynValue ToDynValue(Script script, object obj)
    {
        if (obj is int num) return DynValue.NewNumber(num);
        if (obj is double num2) return DynValue.NewNumber(num2);
        if (obj is float num3) return DynValue.NewNumber(num3);
        if (obj is string str) return DynValue.NewString(str);
        if (obj is bool v) return DynValue.NewBoolean(v);
        if (obj is Dictionary<string, object> dictionary)
        {
            Table table = new Table(script);
            foreach (var item in dictionary)
                table[item.Key] = ToDynValue(script, item.Value);
            return DynValue.NewTable(table);
        }
        if (obj is List<object> list)
        {
            Table table2 = new Table(script);
            for (int i = 0; i < list.Count; i++)
                table2[i + 1] = ToDynValue(script, list[i]);
            return DynValue.NewTable(table2);
        }
        if (obj is GameObject o) return UserData.Create(o);
        return DynValue.Nil;
    }

    public void SetGlobals(Dictionary<string, object> globals)
    {
        foreach (var global in globals)
        {
            DynValue dynValue = ToDynValue(script, global.Value);
            script.Globals[global.Key] = dynValue;
            if (dynValue.Type == DataType.Table)
                trackedTables[global.Key] = dynValue.Table;
        }
    }

    public void UpdateWorkspaceTable()
    {
        if (workspaceTable == null || playersTable == null) return;

        currentFrameKeys.Clear();
        workspaceTable.Clear();
        playersTable.Clear();

        playersTable.Set("Added", UserData.Create(playersAddedEvent));
        playersTable.Set("Removed", UserData.Create(playersRemovedEvent));

        GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag("Object");

        foreach (var go in taggedObjects)
        {
            ObjectClass oc = go.GetComponent<ObjectClass>();
            if (oc == null || string.IsNullOrEmpty(oc.className)) continue;

            string id = go.name;
            currentFrameKeys.Add(id);

            if (!trackedInstances.ContainsKey(id))
            {
                InstanceDatamodel instance = oc.className switch
                {
                    "Player" => new InstancePlayer(go),
                    "Tool" => new InstanceTool(go, script),
                    "Part" => new InstanceDatamodel(go),
                    _ => new InstanceDatamodel(go)
                };

                trackedInstances[id] = instance;

                if (instance is InstancePlayer playerInstance)
                {
                    playersAddedEvent.Fire(UserData.Create(playerInstance));
                }
            }

            var trackedInstance = trackedInstances[id];

            if (trackedInstance is InstancePlayer)
                playersTable[id] = UserData.Create(trackedInstance);
            else
                workspaceTable[id] = UserData.Create(trackedInstance);
        }

        var previousKeys = new List<string>(trackedInstances.Keys);
        foreach (var id in previousKeys)
        {
            if (!currentFrameKeys.Contains(id))
            {
                if (trackedInstances[id] is InstancePlayer playerInstance)
                {
                    playersRemovedEvent.Fire(UserData.Create(playerInstance));
                }

                trackedInstances.Remove(id);
            }
        }

        lastWorkspaceCount = workspaceTable.Keys.Count();
        workspaceInitialized = true;
    }


    private void PrintToConsole(object obj)
    {
        Debug.Log(obj.ToString());
    }

    public override bool Weaved()
    {
        return true;
    }

    private void Update()
    {
        LuaTweenService.Update(Time.deltaTime);
    }
}
