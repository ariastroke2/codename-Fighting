using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VirtualPlayer : MonoBehaviour, IDamageable
{
    public int Hp;
    public int Team;

    private ulong _netID;
    private bool _isVirtualObject;  // Since script is virtual player it's assumed by default that this is a virtual object
    private Action<ulong, AttackMessage> _relayCallback;

    private BoxCollider _boxCollider;

    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();

        Entity.DisableCollision(_boxCollider);
    }

    public void UpdatePlayerState(NetworkPlayerData playerData)
    {
        transform.position = playerData.Position;
        Hp = playerData.HP;
        Team = playerData.Team;
    }

    public void SetNetworkId(ulong id)
    {
        _netID = id;
    }

    public void SetOwnership(bool isOwner)
    {
        _isVirtualObject = !isOwner;
    }

    public void SetDamageRelayCallback(Action<ulong, AttackMessage> relayCallback)
    {
        _relayCallback = relayCallback;
    }

    public void TakeDamage(AttackMessage attackMessage)
    {
        if (Team != attackMessage.AttackingTeam)
        {
            _relayCallback(_netID, attackMessage);
        }
    }
}
