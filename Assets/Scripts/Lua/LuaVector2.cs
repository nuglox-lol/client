using UnityEngine;

public class LuaVector2
{
	public static Vector2 New()
	{
		return new Vector2(0f, 0f);
	}

	public static Vector2 New(float d)
	{
		return new Vector2(d, d);
	}

	public static Vector2 New(float x, float y)
	{
		return new Vector2(x, y);
	}

	public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
	{
		return Vector2.Lerp(a, b, t);
	}
}
