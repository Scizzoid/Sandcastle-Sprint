using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{

    public float floatSpeed = 1.0f;

    private float diff = 0.0f;

    void Update()
    {
        transform.Translate(Vector3.up * Time.deltaTime * (floatSpeed * 0.1f));
        diff += floatSpeed;
        if (diff > 150.0f || diff < 0.0f)
            floatSpeed *= -1.0f;
    }
}
