using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnAttackEnd : MonoBehaviour, IAttackHoldable
{
    public void EndCooldown()
    {
        Destroy(gameObject);
    }
}
