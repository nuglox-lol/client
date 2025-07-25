using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    public string currentBehavior = "None";
    public float speed = 5f;
    public float stoppingDistance = 1.5f;
    public float jumpForce = 8f;
    public float jumpCooldown = 1.0f;

    private Transform target;
    private Rigidbody rb;
    private Animator animator;
    private bool isGrounded = true;
    private float jumpTimer = 0f;

    private void Start()
    {
        if (string.IsNullOrEmpty(currentBehavior))
            currentBehavior = "None";
        else if (currentBehavior != "Enemy" && currentBehavior != "Follower" && currentBehavior != "None")
            currentBehavior = "None";

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        jumpTimer -= Time.deltaTime;

        if (target == null)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag("Object");
            foreach (GameObject obj in objects)
            {
                ObjectClass oc = obj.GetComponent<ObjectClass>();
                if (oc != null && oc.className == "Player")
                {
                    target = obj.transform;
                    break;
                }
            }
        }

        if (currentBehavior == "None" || target == null)
        {
            animator?.SetBool("IsWalking", false);
            animator?.SetBool("IsJumping", false);
            return;
        }

        Vector3 direction = target.position - transform.position;
        float yDiff = target.position.y - transform.position.y;
        direction.y = 0;
        float distance = direction.magnitude;

        if (distance > stoppingDistance)
        {
            Vector3 moveDir = direction.normalized;
            Vector3 movePosition = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            rb.MovePosition(movePosition);

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);

            animator?.SetBool("IsWalking", true);
        }
        else
        {
            animator?.SetBool("IsWalking", false);
            if (currentBehavior == "Enemy")
            {
                Debug.Log("Hit!");
            }
        }

        if (yDiff > 0.5f && isGrounded && jumpTimer <= 0f)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            isGrounded = false;
            jumpTimer = jumpCooldown;
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
}