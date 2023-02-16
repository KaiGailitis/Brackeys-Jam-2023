using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float minXClamp;
    public float maxXClamp;

    public Transform player;

    void LateUpdate()
    {
        Vector3 cameraPosition;

        cameraPosition = transform.position;
        cameraPosition.x = Mathf.Clamp(player.transform.position.x, minXClamp, maxXClamp);

        transform.position = cameraPosition;
    }
}
