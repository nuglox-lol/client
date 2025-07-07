using UnityEngine;

public class TrussMovement : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    private bool isClimbing = false;
    public float speed = 5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        var objClass = collision.gameObject.GetComponent<ObjectClass>();
        if (objClass != null && objClass.className == "Truss")
        {
            isClimbing = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;

            if (animator != null)
                animator.SetBool("IsClimbing", true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var objClass = collision.gameObject.GetComponent<ObjectClass>();
        if (objClass != null && objClass.className == "Truss")
        {
            isClimbing = false;
            rb.useGravity = true;

            if (animator != null)
                animator.SetBool("IsClimbing", false);
        }
    }

    private void FixedUpdate()
    {
        if (!isClimbing) return;

        float verticalInput = Input.GetAxis("Vertical");
        Vector3 climbVelocity = transform.up * verticalInput * speed;
        rb.velocity = new Vector3(rb.velocity.x, climbVelocity.y, rb.velocity.z);
    }
}
