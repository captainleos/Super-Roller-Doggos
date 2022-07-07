using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushObjectsInsideTowards : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float power;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        // other.transform.position = Vector3.MoveTowards(other.transform.position, target.position, power);
        Vector3 boostDirection = (target.transform.position - other.transform.position).normalized;
        Rigidbody otherRb = other.gameObject.GetComponent<Rigidbody>();
        otherRb.AddForce(boostDirection * power);
    }
}
