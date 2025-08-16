using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : NetworkBehaviour
{
    public bool isStudio;
    private bool isMobile = false;
    [SyncVar] public float speed = 8f;
    [SyncVar] public float jumpForce = 8f;
    [SyncVar] public bool CanMove = true;

    private Transform cam;
    private Animator animator;
    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 input;

    private GameObject joystick;
    private GameObject jumpButton;
    private bool jumpPressed;

    [SerializeField] private float stepHeight = 2f;
    [SerializeField] private float stepSmooth = 0.1f;
    [SerializeField] private float stepCheckDistance = 0.5f;

    private void Awake()
    {
        isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
        if (!isMobile)
        {
            var js = GameObject.Find("CoreGui/Joystick");
            if (js != null) js.SetActive(false);
            var jb = GameObject.Find("CoreGui/Jump");
            if (jb != null) jb.SetActive(false);
        }
    }

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

        if (isMobile)
        {
            joystick = GameObject.Find("CoreGui/Joystick");
            jumpButton = GameObject.Find("CoreGui/Jump");
            if (jumpButton != null)
            {
                var eventTrigger = jumpButton.GetComponent<EventTrigger>();
                if (eventTrigger == null) eventTrigger = jumpButton.AddComponent<EventTrigger>();
                var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                entryDown.callback.AddListener(e => { jumpPressed = true; });
                var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                entryUp.callback.AddListener(e => { jumpPressed = false; });
                eventTrigger.triggers.Add(entryDown);
                eventTrigger.triggers.Add(entryUp);
            }
        }
    }

    public void Move(Vector3 moveDir)
    {
        if (rb == null || !CanMove) return;
        Vector3 velocity = moveDir * speed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
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

        if (isMobile && joystick != null)
        {
            var handle = joystick.transform.Find("Handle");
            if (handle != null)
            {
                Vector3 localPos = handle.localPosition;
                input.x = Mathf.Clamp(localPos.x / 100f, -1f, 1f);
                input.z = Mathf.Clamp(localPos.y / 100f, -1f, 1f);
            }
        }
        else
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            input = new Vector3(horizontal, 0f, vertical);
        }
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer && !isStudio) return;
        if (!CanMove) return;

        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 forward = cam.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = cam.right;
            right.y = 0f;
            right.Normalize();
            Vector3 moveDir = (forward * input.z + right * input.x).normalized;

            HandleStepClimb(moveDir);

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

        if (isMobile)
        {
            if (jumpPressed && isGrounded)
            {
                CmdJump(jumpForce);
                isGrounded = false;
                jumpPressed = false;
            }
        }
        else
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                CmdJump(jumpForce);
                isGrounded = false;
            }
        }

        animator?.SetBool("IsJumping", !isGrounded);
    }

    private void HandleStepClimb(Vector3 moveDir)
    {
        if (rb == null) return;

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        RaycastHit hit;

        if (Physics.Raycast(origin, moveDir, out hit, stepCheckDistance))
        {
            Vector3 stepUpOrigin = new Vector3(hit.point.x, hit.collider.bounds.max.y + 0.1f, hit.point.z);
            if (!Physics.Raycast(stepUpOrigin, moveDir, stepCheckDistance))
            {
                Vector3 targetPos = rb.position + Vector3.up * Mathf.Min(stepHeight, hit.collider.bounds.max.y - transform.position.y);
                rb.MovePosition(Vector3.Lerp(rb.position, targetPos, stepSmooth));
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.01f)
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
        if (rb == null || !CanMove) return;
        Vector3 velocity = moveDir * speed;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    [Command]
    private void CmdStop()
    {
        if (rb == null || !CanMove) return;
        rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
    }

    [Command]
    private void CmdJump(float force)
    {
        if (rb == null || !CanMove) return;
        rb.velocity = new Vector3(rb.velocity.x, force, rb.velocity.z);
    }

    public override bool Weaved()
    {
        return true;
    }
}
