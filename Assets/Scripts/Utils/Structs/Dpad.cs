using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dpad
{
    public bool Up;
    public bool Down;
    public bool Left;
    public bool Right;
    public Direction xAxis;
    public Direction yAxis;
    public Vector2 Digitalized;

    List<KeyValuePair<Direction, Action>> Listeners;

    public Dpad()
    {
        this.Up = false;
        this.Down = false;
        this.Left = false;
        this.Right = false;
        this.Digitalized = Vector2.zero;
        xAxis = Direction.None;
        yAxis = Direction.None;
        Listeners = new();
    }

    public void Subscribe(Action action, Direction dir)
    {
        Listeners.Add(new KeyValuePair<Direction, Action>(dir, action));
    }

    public void Press(Direction dir)
    {
        // Register input
        switch (dir)
        {
            case Direction.Left: Left = true; break;
            case Direction.Right: Right = true; break;
            case Direction.Up: Up = true; break;
            case Direction.Down: Down = true; break;
            default: break;
        }
        UpdateAxis();
        foreach(KeyValuePair<Direction, Action> item in Listeners)
        {
            if(item.Key == dir)
                item.Value();
        }
        Digitalize();
    }

    public void Release(Direction dir)
    {
        // Release input
        switch (dir)
        {
            case Direction.Left: Left = false; if (xAxis == Direction.Left) xAxis = Direction.None;  break;
            case Direction.Right: Right = false; if (xAxis == Direction.Right) xAxis = Direction.None; break;
            case Direction.Up: Up = false; if (yAxis == Direction.Up) yAxis = Direction.None; break;
            case Direction.Down: Down = false; if (yAxis == Direction.Down) yAxis = Direction.None; break;
            default: break;
        }
        UpdateAxis();
        Digitalize();
    }

    void UpdateAxis()
    {
        if (xAxis == Direction.None && (Right || Left))
        {
            if(Right)
                xAxis = Direction.Right;
            if (Left)
                xAxis = Direction.Left;
        }
        if (yAxis == Direction.None && (Up || Down))
        {
            if (Up)
                yAxis = Direction.Up;
            if (Down)
                yAxis = Direction.Down;
        }
    }

    void Digitalize()
    {
        if (yAxis == Direction.Up) Digitalized = new Vector2(Digitalized.x, 1);
        if (yAxis == Direction.Down) Digitalized = new Vector2(Digitalized.x, -1);
        if (yAxis == Direction.None) Digitalized = new Vector2(Digitalized.x, 0);
        if (xAxis == Direction.Right) Digitalized = new Vector2(1, Digitalized.y);
        if (xAxis == Direction.Left) Digitalized = new Vector2(-1, Digitalized.y);
        if (xAxis == Direction.None) Digitalized = new Vector2(0, Digitalized.y);
    }

}
