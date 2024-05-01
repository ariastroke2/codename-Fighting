using System;
using UnityEngine;

public interface IDamageable 
{
    void TakeDamage(int dmg, float knockback, Vector2 forceDirection, int team);
}
