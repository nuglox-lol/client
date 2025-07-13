using System;
using UnityEngine;
using MoonSharp.Interpreter;
using TMPro;

[MoonSharpUserData]
public class InstanceText3D : InstanceDatamodel
{
    private GameObject go;
    private Script luaScript;
    private Text3DComponent textcomponent;

    public InstanceText3D(GameObject gameObject, Script lua = null) : base(gameObject, lua)
    {
        if (gameObject == null) throw new ArgumentNullException(nameof(gameObject));
        go = gameObject;
        luaScript = lua;
        textcomponent = go.GetComponent<Text3DComponent>();
    }

    public string Text
    {
        get => textcomponent != null ? textcomponent.GetText() : "";
        set
        {	
            if (textcomponent != null)
                textcomponent.ChangeText(value);
        }
    }

    public Color Color
    {
        get
        {
            var tmp = go.transform.Find("Text3D")?.GetComponent<TextMeshProUGUI>();
            return tmp != null ? tmp.color : Color.white;
        }
        set
        {
            if (textcomponent != null)
                textcomponent.ChangeTextColor(value);
        }
    }
}
