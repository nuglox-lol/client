using System;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class InstanceTool : InstanceDatamodel
{
    private GameObject toolObject;
    private Script luaScript;

    private LuaEvent equippedEvent;
    private LuaEvent unequippedEvent;
    private LuaEvent activatedEvent;

    public InstanceTool(GameObject go, Script lua) : base(go, lua)
    {
        if (go == null) throw new ArgumentNullException(nameof(go));

        toolObject = go;
        luaScript = lua;

        equippedEvent = new LuaEvent(toolObject, luaScript);
        unequippedEvent = new LuaEvent(toolObject, luaScript);
        activatedEvent = new LuaEvent(toolObject, luaScript);

        var toolComponent = go.GetComponent<ToolComponent>();
        if (toolComponent != null)
        {
            toolComponent.OnEquipped += (playerObj) => FireEquipped(new InstanceDatamodel(playerObj));
            toolComponent.OnActivated += (playerObj) => FireActivated(new InstanceDatamodel(playerObj));
            toolComponent.OnUnequipped += (playerObj) => FireUnequipped(new InstanceDatamodel(playerObj));
        }
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
            if (target.parent != null)
                target = target.parent;
            else
                break;

        var animator = target.GetComponent<Animator>();
        if (animator != null)
            animator.Play(animName);
    }
}