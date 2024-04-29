using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character/AttackCollection", fileName = "NewAttackCollection")]
public class AttackCollection : ScriptableObject
{
    [Header("Moveset")]
    [Header("Heavy attacks")]

    public Attack heavyNeutral;
    public Attack heavyUp;
    public Attack heavyDown;
    public Attack heavySlow;
    public Attack heavyDash;
    public Attack heavyAerial;

    [Header("Light attacks")]
    public Attack lightNeutral;
    public Attack lightUp;
    public Attack lightDown;
    public Attack lightDash;
    public Attack lightAerial;

    [Header("Animator")]
    public RuntimeAnimatorController animatorController;
}
