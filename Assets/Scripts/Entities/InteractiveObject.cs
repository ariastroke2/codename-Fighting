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

    void IDamageable.TakeDamage(AttackMessage attackMessage)
    {
        if(_team != attackMessage.AttackingTeam)
            _rigidbody.AddForce(attackMessage.ForceDirection * attackMessage.Knockback, ForceMode.VelocityChange);
    }

}
