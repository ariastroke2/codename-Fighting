using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles_Line : MonoBehaviour, IAttackHoldable
{

    // [SerializeField] private float SpeedDecreaseRate;
    [SerializeField] private float StartSpeed;
    [SerializeField] private Vector3 Direction;

    private float _speed;
    private Quaternion _directionQuat;

    void IAttackHoldable.EndCooldown()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        _speed = StartSpeed;
        _directionQuat = Quaternion.Euler(Direction);
    }

    void Update()
    {
        transform.position += _directionQuat * transform.rotation * Vector3.right * _speed;
    }
}
