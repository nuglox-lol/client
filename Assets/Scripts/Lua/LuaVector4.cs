using UnityEngine;

public class LuaVector4
{
	public static Vector4 New()
	{
		return new Vector4(0f, 0f, 0f, 0f);
	}

	public static Vector3 New(float d)
	{
		return new Vector4(d, d, d, d);
	}

	public static Vector4 New(float x, float y)
	{
		return new Vector4(x, y, 0f, 0f);
	}

	public static Vector4 New(float x, float y, float z)
	{
		return new Vector4(x, y, z, 0f);
	}

	public static Vector4 New(float x, float y, float z, float w)
	{
		return new Vector4(x, y, z, w);
	}
}
