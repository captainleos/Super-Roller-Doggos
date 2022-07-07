using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    private AudioSource cameraAudio;

    private bool gameStarted = false;

    // Game Manager
    private GameManager gameManager;

    void Start()
    {
        cameraAudio = GetComponent<AudioSource>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(gameManager.gameStarted && !gameStarted)
        {
            cameraAudio.Play();
            gameStarted = true;
        }
    }
}
