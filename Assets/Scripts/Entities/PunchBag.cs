using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PunchBag : MonoBehaviour, IDamageable
{
    public int _team;

    private int _hp;

    private Rigidbody _rigidbody;
    private Animator _animator;
    public TextMeshPro _text;

    void IDamageable.TakeDamage(int dmg, float knockback, Vector2 forceDirection, int team)
    {
        if (_team != team)
        {
            _hp -= dmg;
            if (_hp > 0)
                _rigidbody.AddForce(forceDirection * knockback, ForceMode.VelocityChange);
            else
                Respawn();
        }
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        Respawn();
    }

    void Update()
    {
        _text.text = $"Vida: {_hp}";
        _animator.SetFloat("SpeedY", _rigidbody.velocity.y);
    }

    void Respawn()
    {
        _hp = 100;
        transform.position = new Vector3(10, 6, -2);
    }
}
