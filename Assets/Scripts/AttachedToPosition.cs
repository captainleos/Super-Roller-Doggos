using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttachedToPosition : MonoBehaviour
{
    [SerializeField] private float xPos = 0;
    [SerializeField] private float yPos = 0;
    [SerializeField] private float zPos = 0;
    [SerializeField] private GameObject attachedTo;

    void FixedUpdate()
    {
        transform.position = attachedTo.transform.position + new Vector3(xPos, yPos, zPos);
    }
}
