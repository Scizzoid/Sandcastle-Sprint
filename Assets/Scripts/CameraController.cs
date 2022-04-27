using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    private float offset;

    void Start()
    {
        offset = transform.position.x - player.transform.position.x;
    }

    void LateUpdate()
    {
        
        Vector3 temp = transform.position;

        // Only follow the player's x position
        temp.x = player.transform.position.x + offset;
        transform.position = temp;
    }
}