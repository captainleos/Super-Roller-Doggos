using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    public string p1LastCheckpointName;
    public GameObject destinationCheckpoint;

    // Game Manager
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody playerRb = other.GetComponent<Rigidbody>();
        PlayerController playerController = other.GetComponent<PlayerController>();
        string inputID = playerController.inputID;

        if (inputID == "1")
        {
            p1LastCheckpointName = "Checkpoint " + gameManager.p1LastCheckpoint;
        }

        destinationCheckpoint = GameObject.Find(p1LastCheckpointName);

        Vector3 modifiedTransform = destinationCheckpoint.transform.position + new Vector3(0, -10, 0);

        other.gameObject.transform.SetPositionAndRotation(modifiedTransform, destinationCheckpoint.transform.rotation);

        playerRb.velocity = Vector3.zero;
        playerRb.angularVelocity = Vector3.zero;
    }
}