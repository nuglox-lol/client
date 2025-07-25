using System.Collections;
using UnityEngine;
using MoonSharp.Interpreter;

public static class LuaInstance
{
    public static InstanceDatamodel GetCorrectInstance(GameObject go, Script luaScript = null)
    {
        if (go == null) return null;

        ObjectClass oc = go.GetComponent<ObjectClass>();
        if (oc != null && !string.IsNullOrEmpty(oc.className))
        {
            switch (oc.className)
            {
                case "Tool":
                    return new InstanceTool(go, luaScript);
                case "Player":
                    return new InstancePlayer(go);
                case "Explosion":
                    return new InstanceExplosion(go);
                case "Text3D":
                    return new InstanceText3D(go);
                case "PlayerDefaults":
                    return new InstancePlayerDefaults(go);
                default:
                    return new InstanceDatamodel(go, luaScript);
            }
        }

        return new InstanceDatamodel(go, luaScript);
    }
}
