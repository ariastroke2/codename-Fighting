using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RegisteredInput
{
    public Direction Direction;
    public float Timestamp;

    public RegisteredInput(Direction Direction, float Timestamp)
    {
        this.Direction = Direction;
        this.Timestamp = Timestamp;
    }
}
