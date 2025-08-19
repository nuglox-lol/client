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
                case "Sound":
                    return new InstanceSound(go);
                case "Team":
                    return new InstanceTeam(go, luaScript);
                case "Mesh":
                    return new InstanceMesh(go);
                case "Light":
                    return new InstanceLight(go);
                case "Sky":
                    return new InstanceSky(go);
                case "IntValue":
                    return new InstanceIntValue(go);
                case "StringValue":
                    return new InstanceStringValue(go);
                case "BoolValue":
                    return new InstanceBoolValue(go);
                case "FloatValue":
                    return new InstanceFloatValue(go);
                case "ClickDetector":
                    return new InstanceClickDetector(go, luaScript);
                case "Decal":
                    return new InstanceDecal(go);
                default:
                    return new InstanceDatamodel(go, luaScript);
            }
        }

        return new InstanceDatamodel(go, luaScript);
    }
}
