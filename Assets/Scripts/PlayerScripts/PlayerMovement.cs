using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
	// be sure to have networkidentity or else it'll cause random bs errors.

	public bool isStudio;
	public float speed = 5f;
	public float jumpForce = 5f;

	private Transform cam;
	private Animator animator;
	private Rigidbody rb;
	private bool isGrounded;

	private void Start()
	{
		if(!isLocalPlayer && !isStudio) return;

		animator = GetComponent<Animator>();
		rb = GetComponent<Rigidbody>();

		if (rb == null)
		{
			rb = gameObject.AddComponent<Rigidbody>();
			rb.constraints = RigidbodyConstraints.FreezeRotation;
		}

		if (!isLocalPlayer && !isStudio)
		{
			enabled = false;
			return;
		}

		SpawnPlayerCamera();
	}

	private void SpawnPlayerCamera()
	{
		if(!isLocalPlayer && !isStudio) return;

		GameObject prefab = Resources.Load<GameObject>("MainCameraPrefab");
		if (prefab == null)
		{
			Debug.LogError("MainCameraPrefab not found in Resources.");
			return;
		}

		GameObject cameraInstance = Instantiate(prefab);
		cameraInstance.name = "PlayerCamera";
		cam = cameraInstance.transform;

		Camera camComponent = cam.GetComponent<Camera>();
		if (camComponent == null)
		{
			Debug.LogWarning("Camera component missing from MainCameraPrefab.");
		}
		else if (isStudio)
		{
			camComponent.fieldOfView = 95f;
		}

		CameraController controller = cam.GetComponent<CameraController>();
		if (controller != null)
		{
			controller.SetTarget(transform);
		}
		else
		{
			Debug.LogWarning("CameraController missing on PlayerCamera.");
		}
	}

	private void Update()
	{
		if(!isLocalPlayer && !isStudio) return;
		
		if (!(isLocalPlayer || isStudio) || cam == null) return;

		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");
		Vector3 input = new Vector3(horizontal, 0f, vertical);

		if (input.sqrMagnitude > 0.01f)
		{
			Vector3 forward = cam.forward;
			forward.y = 0f;
			forward.Normalize();

			Vector3 right = cam.right;
			right.y = 0f;
			right.Normalize();

			Vector3 moveDir = (forward * vertical + right * horizontal).normalized;
			Vector3 velocity = moveDir * speed;
			rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

			Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

			animator?.SetBool("IsWalking", true);
		}
		else
		{
			rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
			animator?.SetBool("IsWalking", false);
		}

		if (Input.GetButtonDown("Jump") && isGrounded)
		{
			rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
			isGrounded = false;
		}

		animator?.SetBool("IsJumping", !isGrounded);
	}

	private void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			if (contact.normal.y > 0.5f)
			{
				isGrounded = true;
				break;
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		isGrounded = false;
	}

	public override bool Weaved()
	{
		return true;
	}
}
