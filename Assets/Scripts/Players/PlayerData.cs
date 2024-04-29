using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour, IDamageable
{
    public int _team;
    public int _hp;

    private LayerMask _terrainMask;

    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;

    public void RecoverHealth(int dmg)
    {
        Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAA");
    }

    public void TakeDamage(int dmg, float knockback, Vector2 forceDirection, int team)
    {
        if(_team != team)
        {
            _hp -= dmg;
            _rigidbody.AddForce(forceDirection * knockback, ForceMode.VelocityChange);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_terrainMask.value != collision.gameObject.layer)
            Physics.IgnoreCollision(_boxCollider, collision.collider);
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();

        Entity.DisableCollision(_boxCollider);

        _terrainMask = LayerMask.NameToLayer("Terrain");
    }

    void Update()
    {
        
    }
}
