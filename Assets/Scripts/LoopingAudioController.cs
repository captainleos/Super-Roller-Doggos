using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingAudioController : MonoBehaviour
{
    [SerializeField] AudioClip rollSound;
    private PlayerController playerController;
    private AudioSource loopingAudio;
    private bool playingRollSound = false;

    // Start is called before the first frame update
    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        loopingAudio = GetComponent<AudioSource>();
        loopingAudio.clip = rollSound;
        loopingAudio.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.onGround && !playingRollSound)
        {
            loopingAudio.Play();
            playingRollSound = true;
        }

        if (!playerController.onGround && playingRollSound)
        {
            loopingAudio.Pause();
            playingRollSound = false;
        }
    }
}
