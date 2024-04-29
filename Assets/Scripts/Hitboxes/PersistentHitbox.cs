using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentHitbox : MonoBehaviour, IHoldable, IHitbox
{
    private int _damage;
    private float _knockback;
    private int _team;
    private Vector2 _forceDirection;

    public float Delay = 0.25f;

    private List<GameObject> Targets;
    private Dictionary<GameObject, float> DamagedTargets = new();
    private float Timer;

    void Start()
    {
        Timer = 0f;
        Targets = new List<GameObject>();
    }

    void Update()
    {
        Timer += Time.deltaTime;
        foreach(GameObject target in Targets)
        {
            if (DamagedTargets.ContainsKey(target))
            {
                if (DamagedTargets[target] + Delay < Timer)
                {
                    target.GetComponent<IDamageable>()?.TakeDamage(_damage, _knockback, _forceDirection, _team);
                    DamagedTargets[target] = Timer;
                }
            }
            else
            {
                target.GetComponent<IDamageable>()?.TakeDamage(_damage, _knockback, _forceDirection, _team);
                DamagedTargets.Add(target, Timer);
            }
        }
    }

    void OnTriggerEnter(Collider col)
    {
        Targets.Add(col.gameObject);
    }

    void OnTriggerExit(Collider col)
    {
        if(Targets.Contains(col.gameObject))
            Targets.Remove(col.gameObject);
    }
    
    public void EndAttack()
    {
        Destroy(gameObject);
    }

    void IHitbox.SetDamage(int damage)
    {
        _damage = damage;
    }

    void IHitbox.SetKnockback(float knockback)
    {
        _knockback = knockback;
    }

    void IHitbox.SetForceDirection(Vector2 forceDirection)
    {
        _forceDirection = forceDirection;
    }

    void IHitbox.SetTransform(Vector3 offset, Vector3 scale, Quaternion rotation)
    {
        transform.position += offset;
        transform.localScale = scale;
        transform.rotation = rotation;
    }

    void IHitbox.SetTeam(int team)
    {
        throw new System.NotImplementedException();
    }
}
