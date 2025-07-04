using Mirror;
using MoonSharp.Interpreter;
using UnityEngine;

[MoonSharpUserData]
public class InstanceRigidbody : NetworkBehaviour
{
	private Rigidbody rb;

	public Vector3 Velocity
	{
		get
		{
			return rb.velocity;
		}
		set
		{
			rb.velocity = value;
		}
	}

	public bool IsKinematic
	{
		get
		{
			return rb.isKinematic;
		}
		set
		{
			rb.isKinematic = value;
		}
	}

	public bool UseGravity
	{
		get
		{
			return rb.useGravity;
		}
		set
		{
			rb.useGravity = value;
		}
	}

	public float Drag
	{
		get
		{
			return rb.drag;
		}
		set
		{
			rb.drag = value;
		}
	}

	public float AngularDrag
	{
		get
		{
			return rb.angularDrag;
		}
		set
		{
			rb.angularDrag = value;
		}
	}

	public RigidbodyInterpolation Interpolation
	{
		get
		{
			return rb.interpolation;
		}
		set
		{
			rb.interpolation = value;
		}
	}

	public CollisionDetectionMode CollisionDetection
	{
		get
		{
			return rb.collisionDetectionMode;
		}
		set
		{
			rb.collisionDetectionMode = value;
		}
	}

	public InstanceRigidbody(Rigidbody rigidbody)
	{
		rb = rigidbody;
	}

	public void AddForce(Vector3 force)
	{
		rb.AddForce(force);
	}

	public void ResetVelocity()
	{
		rb.velocity = Vector3.zero;
	}

	public void ResetAngularVelocity()
	{
		rb.angularVelocity = Vector3.zero;
	}

	public override bool Weaved()
	{
		return true;
	}
}
