using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "custom/PlayerInfo")]
public class LocalPlayerInfo : ScriptableObject
{
    NetworkString _username;

    public void SetUsername(string newName) {
        _username = newName;
    }

    public string GetUsername()
    {
        return _username;
    }
}
