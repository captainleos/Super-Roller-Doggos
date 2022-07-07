using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private bool used = false;
    [SerializeField] public float timeBonus;

    // Game Manager
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        string inputID = playerController.inputID;

        if (inputID == "1")
        {
            gameManager.p1LastCheckpoint = id;
        }

        if (id == "Start")
        {
            gameManager.timerIsRunning = true;
        }
        if (id == "Finish")
        {
            gameManager.Finish();
        }
        if (!used)
        {
            gameManager.AddTime(timeBonus);
            used = true;
        }
        
    }
}