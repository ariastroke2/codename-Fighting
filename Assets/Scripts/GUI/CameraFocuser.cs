using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFocuser : MonoBehaviour
{
    public Transform target;
    public float OffsetX, OffsetY, OffsetZ;

    // Update is called once per frame
    void Update()
    {
        if(target != null)
            transform.position = target.position + new Vector3(OffsetX, OffsetY, OffsetZ);
    }
}
