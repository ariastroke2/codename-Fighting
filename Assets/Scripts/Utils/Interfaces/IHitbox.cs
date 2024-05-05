using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHitbox
{
    void SetTransform(Vector3 offset, Vector3 scale, Quaternion rotation);
    void SetAttackMessage(AttackMessage attackMessage);
}
