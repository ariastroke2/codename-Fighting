using UnityEngine;

// Hitbox data that can be casted
[System.Serializable]
public struct CastableHitbox
{
    public int dmg;
    public float knockback;
    public Vector2 forceDirection;
    public GameObject hitbox;
    public Vector3 offset;
    public Vector3 scale;
    public Vector3 rotation;

    public CastableHitbox(GameObject hitbox, Vector3 offset, Vector3 scale, Vector3 rotation, int dmg, float knockback, Vector2 forceDirection)
    {
        this.forceDirection = forceDirection;
        this.hitbox = hitbox;
        this.offset = offset;
        this.scale = scale;
        this.rotation = rotation;
        this.dmg = dmg;
        this.knockback = knockback;
    }
}