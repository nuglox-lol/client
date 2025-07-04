using UnityEngine;

public class LuaVector3
{
	public static Vector3 New()
	{
		return new Vector3(0f, 0f, 0f);
	}

	public static Vector3 New(float d)
	{
		return new Vector3(d, d, d);
	}

	public static Vector3 New(float x, float y)
	{
		return new Vector3(x, y, 0f);
	}

	public static Vector3 New(float x, float y, float z)
	{
		return new Vector3(x, y, z);
	}

	public static float Angle(Vector3 from, Vector3 to)
	{
		return Vector3.Angle(from, to);
	}

	public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
	{
		return Vector3.ClampMagnitude(vector, maxLength);
	}

	public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
	{
		return Vector3.Cross(lhs, rhs);
	}

	public static float Distance(Vector3 a, Vector3 b)
	{
		return Vector3.Distance(a, b);
	}

	public static float Dot(Vector3 lhs, Vector3 rhs)
	{
		return Vector3.Dot(lhs, rhs);
	}

	public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
	{
		return Vector3.Lerp(a, b, t);
	}

	public static Vector3 Max(Vector3 lhs, Vector3 rhs)
	{
		return Vector3.Max(lhs, rhs);
	}

	public static Vector3 Min(Vector3 lhs, Vector3 rhs)
	{
		return Vector3.Min(lhs, rhs);
	}

	public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
	{
		return Vector3.MoveTowards(current, target, maxDistanceDelta);
	}

	public static Vector3 Normalize(Vector3 value)
	{
		return Vector3.Normalize(value);
	}

	public static Vector3 Project(Vector3 vector, Vector3 onNormal)
	{
		return Vector3.Project(vector, onNormal);
	}

	public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
	{
		return Vector3.ProjectOnPlane(vector, planeNormal);
	}

	public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
	{
		return Vector3.Reflect(inDirection, inNormal);
	}

	public static Vector3 RotateTowards(Vector3 current, Vector3 target, float maxRadiansDelta, float maxMagnitudeDelta)
	{
		return Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
	}

	public static Vector3 Scale(Vector3 a, Vector3 b)
	{
		return Vector3.Scale(a, b);
	}

	public static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis)
	{
		return Vector3.SignedAngle(from, to, axis);
	}

	public static Vector3 Slerp(Vector3 a, Vector3 b, float t)
	{
		return Vector3.Slerp(a, b, t);
	}

	public static Vector3 SlerpUnclamped(Vector3 a, Vector3 b, float t)
	{
		return Vector3.SlerpUnclamped(a, b, t);
	}

	public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
	{
		return Vector3.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
	}
}
