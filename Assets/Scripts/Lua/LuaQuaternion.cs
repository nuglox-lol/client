using UnityEngine;

public class LuaQuaternion
{
    public static Quaternion Identity()
    {
        return Quaternion.identity;
    }

    public static Quaternion New(float x, float y, float z, float w)
    {
        return new Quaternion(x, y, z, w);
    }

    public static Quaternion Euler(float x, float y, float z)
    {
        return Quaternion.Euler(x, y, z);
    }

    public static Quaternion AngleAxis(float angle, Vector3 axis)
    {
        return Quaternion.AngleAxis(angle, axis);
    }
}
