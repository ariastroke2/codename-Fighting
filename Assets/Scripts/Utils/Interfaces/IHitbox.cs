using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitbox
{
    void SetDamage(int damage);
    void SetKnockback(float knockback);
    void SetForceDirection(Vector2 forceDirection);
    void SetTransform(Vector3 offset, Vector3 scale, Quaternion rotation);
    void SetTeam(int team);
}
