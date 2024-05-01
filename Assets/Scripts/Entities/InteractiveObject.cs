using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour, IDamageable
{
    public int _team;

    private Rigidbody _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    
    void Update()
    {
        
    }

    void IDamageable.TakeDamage(int dmg, float knockback, Vector2 forceDirection, int team)
    {
        if(_team != team)
            _rigidbody.AddForce(forceDirection * knockback, ForceMode.VelocityChange);
    }

}
