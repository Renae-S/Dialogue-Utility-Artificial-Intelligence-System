using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    [Range(0.0f, 0.2f)]
    public float rotationSpeed = 0.05f;

    // Update is called once per frame
    // Rotates the suns on the x axis to simulate sun movement for day and night cycles
    void Update ()
    {
        transform.Rotate(new Vector3(rotationSpeed, 0, 0));
    }
}
