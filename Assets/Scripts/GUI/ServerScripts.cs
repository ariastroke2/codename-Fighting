using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class ServerScripts : NetworkBehaviour
{
    InputField server;
    InputField port;

    private string _roomCode;

    UnityTransport _transport;

    [SerializeField] private GameObject _activeServerMenu;
    [SerializeField] private GameObject _inactiveServerMenu;

    [SerializeField] private TextMeshProUGUI _roomCodeLabel;
    [SerializeField] public TMP_InputField _roomCodeField;

    [SerializeField] public TMP_InputField _usernameField;
    [SerializeField] public LocalPlayerInfo _playerInfo;

    async void Start()
    {
        _transport = GameObject.Find("NetworkManager").GetComponent<UnityTransport>();

        _activeServerMenu.SetActive(false);
        _inactiveServerMenu.SetActive(true);

        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void StartHost()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(4);
        _roomCodeLabel.text = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        Debug.Log(_transport);
        Debug.Log(a);

        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        NetworkManager.Singleton.StartHost();

        _activeServerMenu.SetActive(true);
        _inactiveServerMenu.SetActive(false);
    }

    public async void StartClient()
    {
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(joinCode: _roomCodeField.text);

        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);

        NetworkManager.Singleton.StartClient();

        _activeServerMenu.SetActive(true);
        _inactiveServerMenu.SetActive(false);
    }

    public void StopConnection()
    {
        NetworkManager.Singleton.Shutdown();

        _activeServerMenu.SetActive(false);
        _inactiveServerMenu.SetActive(true);
    }

    public void UpdateRoomCode(string newCode)
    {
        _roomCode = newCode;
    }

    public void OnUsernameFieldUpdate()
    {
        _playerInfo.SetUsername(_usernameField.text);
    }

    void Update()
    {
        Vector3 HSVColor = new();
        Color.RGBToHSV(_roomCodeLabel.color, out HSVColor.x, out HSVColor.y, out HSVColor.z);
        HSVColor.x += 0.2f * Time.deltaTime;
        _roomCodeLabel.color = Color.HSVToRGB(HSVColor.x, HSVColor.y, HSVColor.z);
    }
}
