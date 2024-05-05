using UnityEngine;

public class InstantHitbox : MonoBehaviour, IHitbox
{

    private float Timer;
    private AttackMessage _attackMessage;

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
        col.GetComponent<IDamageable>()?.TakeDamage(_attackMessage);
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
}
