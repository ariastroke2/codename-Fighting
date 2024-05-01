using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particles_Spin : MonoBehaviour, IAttackHoldable
{

    [SerializeField] private float Duration;
    [SerializeField] private float StartSpeed;

    private float _speed;
    private bool _loseSpeed;
    private GameObject _target;
    private List<TrailRenderer> _trailRenderers;

    void IAttackHoldable.SetTarget(GameObject target)
    {
        _target = target;
    }

    void IAttackHoldable.EndCooldown()
    {
        _loseSpeed = true;
        _target = null;
    }

    void Start()
    {
        _speed = StartSpeed;
        _loseSpeed = false;
        _trailRenderers = new();
        foreach(Transform child in transform)
        {
            _trailRenderers.Add(child.gameObject.GetComponent<TrailRenderer>());
        }
    }


    void Update()
    {
        if (_target != null)
            transform.position = _target.transform.position;
        if (_loseSpeed)
        {
            foreach (TrailRenderer tr in _trailRenderers)
            {
                tr.widthMultiplier -= Duration * Time.deltaTime;
            }
                _speed -= StartSpeed * Duration * Time.deltaTime;
        }
        transform.Rotate(0, 0, -_speed);
        if( _speed < 0 )
            Destroy(gameObject);
    }
}
