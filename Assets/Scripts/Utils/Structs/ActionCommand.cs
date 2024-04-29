using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ActionCommand
{
    public string[] Part;

    public string Last(int pieces)
    {
        string str = "";
        for(int i = 0; i < pieces && i < Part.Length; i++)
        {
            str = "/" + Part[^(i + 1)] + str;
        }
        return str;
    }
    public string All()
    {
        string str = "";
        foreach(string part in Part)
        {
            if(part != "")
                str += "/" + part;
        }
        return str;
    }

    public ActionCommand(params Direction[] com)
    {
        int item = 0;
        string[] bld = new string[com.Length];
        bool facingRight = false;
        for (int i = 0; i < com.Length; i++)
        {
            if (com[^(i + 1)] == Direction.Right || com[^(i + 1)] == Direction.Left)
            {
                if (com[^(i + 1)] == Direction.Right)
                    facingRight = true;
                break;
            }
        }
        foreach (Direction d in com)
        {
            if (d == Direction.Up)
                bld[item] = "up";
            else if (d == Direction.Down)
                bld[item] = "down";
            else if (d == Direction.None)
                bld[item] = "";
            else
            {
                if (!(d == Direction.Right ^ facingRight))
                    bld[item] = "forth";
                else
                    bld[item] = "back";
            }
            item++;
        }
        Part = bld;
    }
}
