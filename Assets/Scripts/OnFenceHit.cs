using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnFenceHit : MonoBehaviour
{
    public Material blue;

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("Hit a fence");
        GetComponent<MeshRenderer>().sharedMaterial = blue;
    }
}
