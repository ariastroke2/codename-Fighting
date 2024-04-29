using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnAttackEnd : MonoBehaviour, IHoldable
{
    public void EndAttack()
    {
        Destroy(gameObject);
    }
}
