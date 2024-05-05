using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct NetworkPlayerData : INetworkSerializable
{
    public int HP;
    public int Team;

    // Animation dependencies
    public Vector3 Position;
    public AttackStatus AttackStatus;

    public NetworkPlayerData(int hP, int team, Vector3 position, AttackStatus attackStatus)
    {
        HP = hP;
        Team = team;
        Position = position;
        AttackStatus = attackStatus;
    }

    void INetworkSerializable.NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        serializer.SerializeValue(ref HP);
        serializer.SerializeValue(ref Team);
        serializer.SerializeValue(ref Position);
        serializer.SerializeValue(ref AttackStatus);
    }
}