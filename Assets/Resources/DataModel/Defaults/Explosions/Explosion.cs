using UnityEngine;

public class Explosion : MonoBehaviour
{
    private ParticleSystem _particles;
    private float _radius = 5f;

    private void Awake()
    {
        _particles = GetComponentInChildren<ParticleSystem>();
    }

    public void Trigger()
    {
        if (_particles != null)
        {
            _particles.Play();
        }
        
        Collider[] hits = Physics.OverlapSphere(transform.position, _radius);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Rigidbody rb = hit.attachedRigidbody;

            if (rb != null && rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }
    }
}