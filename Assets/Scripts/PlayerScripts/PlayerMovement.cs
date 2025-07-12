using Mirror;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public bool isStudio;
    [SyncVar] public float speed = 8f;
    [SyncVar] public float jumpForce = 8f;

    private Transform cam;
    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded;
    private Seat currentSeat;
    private bool isSeated => currentSeat != null;
    private Vector3 input;

    private void Start()
    {
        if (!isLocalPlayer && !isStudio) return;

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
        if (!isLocalPlayer && !isStudio) return;

        GameObject prefab = Resources.Load<GameObject>("MainCameraPrefab");
        if (prefab == null) return;

        GameObject cameraInstance = Instantiate(prefab);
        cameraInstance.name = "PlayerCamera";
        cam = cameraInstance.transform;

        Camera camComponent = cam.GetComponent<Camera>();
        if (camComponent == null) return;
        if (isStudio) camComponent.fieldOfView = 95f;

        CameraController controller = cam.GetComponent<CameraController>();
        if (controller != null) controller.SetTarget(transform);
    }

    private void Update()
    {
        if (!isLocalPlayer && !isStudio) return;
        if (!(isLocalPlayer || isStudio) || cam == null) return;

        if (isSeated)
        {
            input = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CmdStandUp();
            }
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        input = new Vector3(horizontal, 0f, vertical);
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer && !isStudio) return;

        if (isSeated)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = currentSeat.transform.position;
            animator?.SetBool("IsWalking", false);
            return;
        }

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 forward = cam.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 right = cam.right;
            right.y = 0f;
            right.Normalize();

            Vector3 moveDir = (forward * input.z + right * input.x).normalized;

            CmdMove(moveDir);

            animator?.SetBool("IsWalking", true);
            Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            CmdStop();
            animator?.SetBool("IsWalking", false);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            CmdJump(jumpForce);
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

    [Command]
    private void CmdMove(Vector3 moveDir)
    {
        if (rb == null) return;
        Vector3 velocity = moveDir * speed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    [Command]
    private void CmdStop()
    {
        if (rb == null) return;
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    [Command]
    private void CmdJump(float force)
    {
        if (rb == null) return;
        rb.velocity = new Vector3(rb.velocity.x, force, rb.velocity.z);
    }

    [Command]
    private void CmdStandUp()
    {
        if (currentSeat != null)
        {
            currentSeat.CmdRequestStand(netIdentity);
        }
    }

    public void SitOn(Seat seat)
    {
        currentSeat = seat;
    }

    public void StandUp()
    {
        currentSeat = null;
    }

    public override bool Weaved()
    {
        return true;
    }
}