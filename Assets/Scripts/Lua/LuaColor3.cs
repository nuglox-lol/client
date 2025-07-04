using UnityEngine;

public class LuaColor3
{
	public static Color New()
	{
		return new Color(0f, 0f, 0f);
	}

	public static Color New(float d)
	{
		return new Color(d, d, d);
	}

	public static Color New(float r, float g, float b)
	{
		return new Color(r, g, b);
	}

	public static Color New(float r, float g, float b, float a)
	{
		return new Color(r, g, b, a);
	}

	public static Color Random()
	{
		return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
	}

	public static Color FromHex(string hex)
	{
		ColorUtility.TryParseHtmlString(hex, out var color);
		return color;
	}

	public static Color Lerp(Color a, Color b, float t)
	{
		return Color.Lerp(a, b, t);
	}
}
