using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentHitbox : MonoBehaviour, IAttackHoldable, IHitbox
{
    private int _damage;
    private float _knockback;
    private int _team;
    private Vector2 _forceDirection;

    private AttackMessage _attackMessage;

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
                    target.GetComponent<IDamageable>()?.TakeDamage(_attackMessage);
                    DamagedTargets[target] = Timer;
                }
            }
            else
            {
                target.GetComponent<IDamageable>()?.TakeDamage(_attackMessage);
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

    void IHitbox.SetTransform(Vector3 offset, Vector3 scale, Quaternion rotation)
    {
        transform.position += offset;
        transform.localScale = scale;
        transform.rotation = rotation;
    }

    void IHitbox.SetAttackMessage(AttackMessage attackMessage)
    {
        _attackMessage = attackMessage;
    }

    void IAttackHoldable.EndCooldown()
    {
        Destroy(gameObject);
    }
}
