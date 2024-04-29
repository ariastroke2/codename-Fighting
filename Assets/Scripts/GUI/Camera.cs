using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public Transform target;
    public float OffsetX, OffsetY, OffsetZ;

    // Update is called once per frame
    void Update()
    {
        // transform.position = new Vector3(target.position.x + OffsetX, target.position.y + OffsetY, -5f);
        transform.position = target.position + new Vector3(OffsetX, OffsetY, OffsetZ);
    }
}
