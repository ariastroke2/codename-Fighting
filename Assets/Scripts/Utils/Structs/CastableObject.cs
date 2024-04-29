using UnityEngine;

// Object data that can be casted and its transforms ; for use in lists
[System.Serializable]
public struct CastableObject
{
    public GameObject obj;
    public Vector3 offset;
    public Vector3 scale;
    public Vector3 rotation;
    public float delay;

    public CastableObject WithDelay(float newDelay) => new CastableObject
    {
        obj = this.obj,
        offset = this.offset,
        scale = this.scale,
        rotation = this.rotation,  
        delay = this.delay + newDelay
    };
}