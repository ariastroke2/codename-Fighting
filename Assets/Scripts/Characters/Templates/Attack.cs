using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Character/Attack", fileName = "NewAttack")]
public class Attack : ScriptableObject
{
    [Header("Attack Class Type")]
    public string AttackClassType;

    [Header("Attack properties")]
    public CastableHitbox Hitbox;
    public AttackInputType Type;
    public bool LockMovement;

    [Header("Attack duration")]
    public float Delay;
    public AttackDurationType DurationType;
    public float Duration;
    public float EndLag;

    [Header("Attack impulse")]
    public SpeedType SpeedType;
    public Vector3 ImpulseVector;

    [Header("Attack effects")]
    public CastableObject[] Effects;

    [Header("Sequence")]
    public AttackSequenceType SequenceType;
    public Attack Next;
}
