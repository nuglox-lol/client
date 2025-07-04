using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class LuaTweenService
{
    private class TweenInstance
    {
        public InstanceDatamodel Target;
        public string Property;
        public object StartValue;
        public object EndValue;
        public float Duration;
        public float Elapsed;
        public Func<float, float> Easing;
        public Action<InstanceDatamodel, object> Setter;
        public bool Finished;
    }

    private static List<TweenInstance> activeTweens = new List<TweenInstance>();

    public static DynValue Tween(InstanceDatamodel target, string property, DynValue to, float duration, string easing = "linear")
    {
        if (target == null)
            return DynValue.Nil;

        Func<float, float> easingFunc = GetEasing(easing);

        object start = null, end = null;
        Action<InstanceDatamodel, object> setter = null;

        switch (property)
        {
            case "Position":
                start = target.Position;
                end = to.ToObject<Vector3>();
                setter = (targ, val) => targ.Position = (Vector3)val;
                break;
            case "Rotation":
                start = target.Rotation;
                end = to.ToObject<Vector3>();
                setter = (targ, val) => targ.Rotation = (Vector3)val;
                break;
            case "LocalScale":
                start = target.LocalScale;
                end = to.ToObject<Vector3>();
                setter = (targ, val) => targ.LocalScale = (Vector3)val;
                break;
            case "Color":
                start = target.Color;
                end = to.ToObject<Color>();
                setter = (targ, val) => targ.Color = (Color)val;
                break;
            default:
                return DynValue.Nil;
        }

        float dt = duration > 0 ? duration : 0.0001f;

        activeTweens.Add(new TweenInstance
        {
            Target = target,
            Property = property,
            StartValue = start,
            EndValue = end,
            Duration = dt,
            Elapsed = 0f,
            Easing = easingFunc,
            Setter = setter,
            Finished = false
        });

        return DynValue.NewBoolean(true);
    }

    public static void Update(float deltaTime)
    {
        for (int i = activeTweens.Count - 1; i >= 0; i--)
        {
            var tween = activeTweens[i];
            if (tween.Finished)
                continue;

            tween.Elapsed += deltaTime;
            float t = Mathf.Clamp01(tween.Elapsed / tween.Duration);
            float eased = tween.Easing(t);

            object value = null;
            switch (tween.Property)
            {
                case "Position":
                case "Rotation":
                case "LocalScale":
                    value = Vector3.LerpUnclamped((Vector3)tween.StartValue, (Vector3)tween.EndValue, eased);
                    break;
                case "Color":
                    value = Color.LerpUnclamped((Color)tween.StartValue, (Color)tween.EndValue, eased);
                    break;
            }

            tween.Setter(tween.Target, value);

            if (t >= 1f)
                tween.Finished = true;
        }

        activeTweens.RemoveAll(t => t.Finished);
    }

    private static Func<float, float> GetEasing(string name)
    {
        switch (name)
        {
            case "linear": return t => t;
            case "easeIn": return t => t * t;
            case "easeOut": return t => t * (2 - t);
            case "easeInOut": return t => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
            default: return t => t;
        }
    }
}
