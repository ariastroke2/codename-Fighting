using System;
using UnityEngine;

public interface IAttackHoldable
{
    void EndAttack() { }
    void EndCooldown();
    void ReleaseAttack() { }
    void SetTarget(GameObject target) { }
    void SetOffset(Vector3 offset) { }
}
