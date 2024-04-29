using UnityEngine;

public class InstantHitbox : MonoBehaviour, IHitbox
{

    private float Timer;
    private int _damage;
    private float _knockback;
    private int _team;
    private Vector2 _forceDirection;

    private void LateUpdate()
    {
        Timer += Time.deltaTime;
        if(Timer > 0.05f)
        {
            Destroy(gameObject);
        }
    }


    void OnTriggerEnter(Collider col)
    {
        col.GetComponent<IDamageable>()?.TakeDamage(_damage, _knockback, _forceDirection, _team);
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
