using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetPlayer : NetworkBehaviour
{

    private NetworkVariable<NetworkPlayerData> _netData = new(writePerm: NetworkVariableWritePermission.Owner);

    void Start()
    {
        
    }

    void Update()
    {
        // if (IsOwner)
        // {
        //     _netData.Value = new NetworkPlayerData(transform.position);
        // }
        // else
        // {
        //     transform.position = _netData.Value._position;
        // }
    }
}
