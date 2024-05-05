using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct AttackMessage : INetworkSerializable
{
    public int Damage;
    public float Knockback;
    public Vector2 ForceDirection;
    public int AttackingTeam;

    public AttackMessage(int damage, float knockback, Vector2 direction, int attackingTeam)
    {
        Damage = damage;
        Knockback = knockback;
        ForceDirection = direction;
        AttackingTeam = attackingTeam;
    }

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref Damage);
        serializer.SerializeValue(ref Knockback);
        serializer.SerializeValue(ref ForceDirection);
        serializer.SerializeValue(ref AttackingTeam);
    }
}
