using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking.Types;

public class PlayerSlot : NetworkBehaviour
{
    // Data transferred through network
    private NetworkVariable<NetworkPlayerData> _netData = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<NetworkString> _netUsername = new(writePerm: NetworkVariableWritePermission.Owner);

    // Connect slot to player
    private PlayerActions _actionsScript;
    private Dpad _dpad;

    // Connect player to slot
    private VirtualPlayer _virtualPlayer;

    [SerializeField] private LocalPlayerInfo _playerInfo;
    [SerializeField] private TMPro.TextMeshProUGUI _usernameLabel;
    [SerializeField] private UnityEngine.UI.Image _LifeGauge;

    void Start()
    {
        _actionsScript = GetComponent<PlayerActions>();
        _virtualPlayer = GetComponent<VirtualPlayer>();

        if (IsOwner)
        {
            Destroy(_virtualPlayer);

            _dpad = new Dpad();
            _actionsScript.AssignDpad(ref _dpad);

            _actionsScript.SetNetworkId(NetworkObjectId);
            _actionsScript.Team = (int)NetworkObjectId;
        }
        else
        {
            Destroy(_actionsScript);

            _virtualPlayer.SetNetworkId(NetworkObjectId);
            _virtualPlayer.SetOwnership(IsOwner);
            _virtualPlayer.SetDamageRelayCallback(RelayDamage_ServerRpc);
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            _netData.Value = _actionsScript.GetPlayerState();
            _netUsername.Value = _playerInfo.GetUsername();

            // Player slot should not be the one to modify these ui elements but waghh
            _LifeGauge.fillAmount = (_actionsScript.GetPlayerState().HP / 100f);
            _usernameLabel.text = _playerInfo.GetUsername();
        }
        else
        {
            _virtualPlayer.UpdatePlayerState(_netData.Value);

            _LifeGauge.fillAmount = (_netData.Value.HP / 100f);
            _usernameLabel.text = _netUsername.Value;
        }

        // frantic color effect
        Vector3 HSVColor = new();
        Color.RGBToHSV(_usernameLabel.color, out HSVColor.x, out HSVColor.y, out HSVColor.z);
        HSVColor.x += 0.2f * Time.deltaTime;
        _usernameLabel.color = Color.HSVToRGB(HSVColor.x, HSVColor.y, HSVColor.z);
    }

    public void DirectionalInput(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            Direction button = Enum.Parse<Direction>(ctx.action.name);
            if (ctx.started)
                _dpad.Press(button);
            if (ctx.canceled)
                _dpad.Release(button);
        }
    }

    public void ExecuteAttack(InputAction.CallbackContext ctx)
    {
        if (IsOwner)
        {
            if (ctx.performed)
                _actionsScript.ExecuteInput(ctx.action.name);
            if (ctx.canceled)
                _actionsScript.ReleaseInput(ctx.action.name);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RelayDamage_ServerRpc(ulong target, AttackMessage attackMessage)
    {
        CatchRelayedDamage_ClientRpc(target, attackMessage);
    }

    [ClientRpc]
    private void CatchRelayedDamage_ClientRpc(ulong target, AttackMessage attackMessage)
    {
        if (target == NetworkObjectId && IsOwner)
        {
            _actionsScript.TakeDamage(attackMessage);
        }
    }
}
