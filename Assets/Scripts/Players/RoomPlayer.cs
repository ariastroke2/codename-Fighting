using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RoomPlayer : NetworkBehaviour
{
    private NetworkVariable<NetworkString> _username = new(writePerm: NetworkVariableWritePermission.Owner);

    private TextMeshPro _textMeshPro;
    public TMPro.TMP_InputField _usernameInputField;

    [SerializeField] private Renderer _renderer;

    private Color[] PlayerColors = {Color.white, Color.red, Color.yellow, Color.blue};

    void Start()
    {
        Debug.Log("Joined client id >> " + OwnerClientId);
        _textMeshPro = transform.Find("Label").GetComponent<TextMeshPro>();
        _usernameInputField = GameObject.Find("Canvas/UsernameField").GetComponent<TMP_InputField>();

        Debug.Log(_renderer.materials[4].color = PlayerColors[OwnerClientId]);// .SetColor("White", PlayerColors[OwnerClientId]);
        transform.position = new Vector3(-7.24f + (5f * (OwnerClientId)), transform.position.y, 0f); 
    }

    void Update()
    {
        if (IsOwner)
        {
            _textMeshPro.text = _usernameInputField.text;
            _username.Value = _usernameInputField.text;
        }
        else
        {
            _textMeshPro.text = _username.Value;
        }
    }
}
