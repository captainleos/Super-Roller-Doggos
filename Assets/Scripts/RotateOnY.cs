using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateOnY : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 60;

    void FixedUpdate()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
    }
}