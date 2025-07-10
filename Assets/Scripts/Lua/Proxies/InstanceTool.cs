using System;
using MoonSharp.Interpreter;
using UnityEngine;
using System.Collections;

[MoonSharpUserData]
public class InstanceTool : InstanceDatamodel
{
    private GameObject toolObject;
    private Script luaScript;

    private LuaEvent equippedEvent;
    private LuaEvent unequippedEvent;
    private LuaEvent activatedEvent;

    private ToolComponent toolComponent;
    private MonoBehaviour coroutineRunner;

    public InstanceTool(GameObject go, Script lua) : base(go, lua)
    {
        if (go == null) throw new ArgumentNullException(nameof(go));

        toolObject = go;
        luaScript = lua;

        equippedEvent = new LuaEvent(toolObject, luaScript);
        unequippedEvent = new LuaEvent(toolObject, luaScript);
        activatedEvent = new LuaEvent(toolObject, luaScript);

        toolComponent = go.GetComponent<ToolComponent>();

        coroutineRunner = go.GetComponent<MonoBehaviour>();
        if (coroutineRunner == null)
            coroutineRunner = toolObject.AddComponent<HelperMonoBehaviour>();

        CheckAndSubscribe();
    }

    void CheckAndSubscribe()
    {
        if (toolComponent == null) return;

        if (toolObject.activeInHierarchy && toolComponent.enabled)
        {
            SubscribeEvents();
        }
        else
        {
            coroutineRunner.StartCoroutine(WaitForEnable());
        }
    }

    System.Collections.IEnumerator WaitForEnable()
    {
        while (!toolObject.activeInHierarchy || !toolComponent.enabled)
            yield return null;

        SubscribeEvents();
    }

    void SubscribeEvents()
    {
        toolComponent.OnEquipped -= OnEquippedHandler;
        toolComponent.OnActivated -= OnActivatedHandler;
        toolComponent.OnUnequipped -= OnUnequippedHandler;

        toolComponent.OnEquipped += OnEquippedHandler;
        toolComponent.OnActivated += OnActivatedHandler;
        toolComponent.OnUnequipped += OnUnequippedHandler;
    }

    void OnEquippedHandler(GameObject playerObj)
    {
        FireEquipped(new InstanceDatamodel(playerObj));
    }

    void OnActivatedHandler(GameObject userObj)
    {
        FireActivated(new InstanceDatamodel(userObj));
    }

    void OnUnequippedHandler(GameObject playerObj)
    {
        FireUnequipped(new InstanceDatamodel(playerObj));
    }

    public LuaEvent Equipped => equippedEvent;
    public LuaEvent Unequipped => unequippedEvent;
    public LuaEvent Activated => activatedEvent;

    public void FireEquipped(InstanceDatamodel whoEquipped)
    {
        equippedEvent.Fire(DynValue.FromObject(luaScript, whoEquipped));
    }

    public void FireUnequipped(InstanceDatamodel whoUnequipped)
    {
        unequippedEvent.Fire(DynValue.FromObject(luaScript, whoUnequipped));
    }

    public void FireActivated(InstanceDatamodel user)
    {
        activatedEvent.Fire(DynValue.FromObject(luaScript, user));
    }

    public void Play(string animName)
    {
        Transform target = toolObject.transform;
        for (int i = 0; i < 3; i++)
        {
            if (target.parent != null)
                target = target.parent;
            else
                break;
        }

        var animator = target.GetComponent<Animator>();
        if (animator != null)
            animator.Play(animName);
    }

    class HelperMonoBehaviour : MonoBehaviour { }
}