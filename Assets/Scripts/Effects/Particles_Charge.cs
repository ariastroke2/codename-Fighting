using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles_Charge : MonoBehaviour, IAttackHoldable
{
    [SerializeField] private float PersistenceTime;

    private GameObject _target;
    private float _deathTimer;
    private bool _done;

    private Vector3 _offset;
    private ParticleSystem _particles;

    void IAttackHoldable.SetOffset(Vector3 offset)
    {
        _offset = offset;
    }

    void IAttackHoldable.SetTarget(GameObject target)
    {
        _target = target;
    }

    void IAttackHoldable.ReleaseAttack()
    {
        _particles.Stop();
        _target = null;
        _done = true;
    }

    void Start()
    {
        _particles = GetComponent<ParticleSystem>();
        _deathTimer = PersistenceTime;
        _done = false;
    }

    void Update()
    {
        if (_target != null)
            transform.position = _target.transform.position + _offset;
        if (_done)
        {
            _deathTimer -= Time.deltaTime;
            if (_deathTimer < 0)
                Destroy(gameObject);
        }
    }

    void IAttackHoldable.EndCooldown()
    {
        // Destroy(gameObject);
    }
}
