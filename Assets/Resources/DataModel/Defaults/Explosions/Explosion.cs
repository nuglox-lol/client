using Mirror;
using UnityEngine;

public class Explosion : NetworkBehaviour
{
    private ParticleSystem _particles;
    private AudioSource _audioSource;
    private AudioClip _explosionClip;

    public float radius = 3f;
    public float explosionForce = 700f;
    public float upwardsModifier = 1f;
    public float massThreshold = 100f;

    private void Awake()
    {
        _particles = GetComponentInChildren<ParticleSystem>();
        _audioSource = GetComponent<AudioSource>();

        _explosionClip = Resources.Load<AudioClip>("Audio/Explosion");
        if (_audioSource != null && _explosionClip != null)
        {
            _audioSource.clip = _explosionClip;
            _audioSource.playOnAwake = false;

            _audioSource.spatialBlend = 1f;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.minDistance = radius * 0.2f;
            _audioSource.maxDistance = radius * 2f;
        }
    }

    [Server]
    public void Trigger()
    {
        RpcTrigger();

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Rigidbody rb = hit.attachedRigidbody;

            if (rb == null)
            {
                rb = hit.gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = false;
            }
            else if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }

            if (rb.mass <= massThreshold)
            {
                rb.AddExplosionForce(explosionForce, transform.position, radius, upwardsModifier, ForceMode.Impulse);
            }
        }
    }

    [ClientRpc]
    void RpcTrigger()
    {
        if (_particles != null)
            _particles.Play();

        _audioSource.clip = _explosionClip;
        _audioSource.playOnAwake = false;

        _audioSource.spatialBlend = 1f;
        _audioSource.rolloffMode = AudioRolloffMode.Linear;
        _audioSource.minDistance = radius * 0.2f;
        _audioSource.maxDistance = radius * 2f;

        if (_audioSource != null)
            _audioSource.Play();
    }
}