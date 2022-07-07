using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDrop : MonoBehaviour
{
    [SerializeField] private float timer = 3f;

    void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

    void Update()
    {
        if (Time.time > timer)
        {
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
