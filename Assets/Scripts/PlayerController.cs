using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Notes
    // Better scoresystem, higher value food item burger that gives 50 bonus points
    // Score bonus for airspins?
    // Create out of bounds barriers and a respawn system, maybe move character back to last checkpoint?

    // Debug
    public float currentSpeed;

    // Movement
    [SerializeField] public bool onGround = false;
    [SerializeField] bool executingOnGroundFalse = false;
    [SerializeField] float delayTimeOnGroundFalse = 0.5f;
    [SerializeField] GameObject vCamera;
    [SerializeField] float maxSpeed = 45f;
    [SerializeField] float moveSpeedZ = 1f;
    [SerializeField] float moveSpeedX = 1f;
    [SerializeField] float upwardsForce = 1f;
    [SerializeField] float forwardTorque = 1f;
    [SerializeField] float sidewaysTorque = 1f;
    [SerializeField] int offGroundMovementSpeedDivider = 1;
    
    public Rigidbody rb;

    // Jumping variables
    public float jumpSpeed = 3f;
    public float jumpDelay = 1f;
    private bool canjump;
    private bool isjumping;
    private float jumpCountDown;

    [Header("Audio")]
    [SerializeField] AudioClip jumpSound;
    [SerializeField] AudioClip chompSound;
    [SerializeField] AudioClip wind;
    private AudioSource playerAudio;

    [Header("Effects")]
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private ParticleSystem explosionParticle2;
    [SerializeField] private ParticleSystem explosionParticle3;

    // Game Manager
    private GameManager gameManager;

    [Header("Multiplayer settings")]
    public string inputID;

    void Start()
    {
        canjump = true;
        jumpCountDown = jumpDelay;
        rb = GetComponent<Rigidbody>();
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        playerAudio = GetComponent<AudioSource>();
        playerAudio.PlayOneShot(wind, 0.8f);
        playerAudio.loop = true;
    }

    private void Update()
    {
        if (gameManager.gameStarted && !rb.useGravity)
        {
            rb.useGravity = true;
        }

        JumpDelay();
    }


    void FixedUpdate()
    {
        if (gameManager.gameStarted && !gameManager.gameOver)
        {
            MovePlayer();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            if (!onGround)
            {
                gameManager.UpdateScore(1);
            }
        }
    }

    void MovePlayer()
    {
        float z = Input.GetAxis("Vertical" + inputID);
        float x = Input.GetAxis("Horizontal" + inputID);

        if (onGround)
        {
            rb.AddForce(vCamera.transform.forward * z * Time.deltaTime * moveSpeedZ);
            rb.AddForce(vCamera.transform.up * z * Time.deltaTime * upwardsForce);
            rb.AddTorque(vCamera.transform.forward * z * Time.deltaTime * forwardTorque);

            rb.AddForce(vCamera.transform.right * x * Time.deltaTime * moveSpeedX);
            rb.AddForce(vCamera.transform.up * z * Time.deltaTime * (upwardsForce / 3));
            rb.AddTorque(vCamera.transform.right * z * Time.deltaTime * sidewaysTorque);
        }
        if (!onGround)
        {
            rb.AddForce(vCamera.transform.forward * z * Time.deltaTime * (moveSpeedZ / offGroundMovementSpeedDivider));
            rb.AddForce(vCamera.transform.up * z * Time.deltaTime * (upwardsForce / offGroundMovementSpeedDivider));
            rb.AddTorque(vCamera.transform.forward * z * Time.deltaTime * (forwardTorque / offGroundMovementSpeedDivider));

            rb.AddForce(vCamera.transform.right * x * Time.deltaTime * (moveSpeedX / offGroundMovementSpeedDivider));
            rb.AddTorque(vCamera.transform.right * z * Time.deltaTime * (sidewaysTorque / offGroundMovementSpeedDivider));
        }

        currentSpeed = rb.velocity.magnitude;
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }

    void Jump()
    {
        if (canjump && onGround)
        {
            playerAudio.PlayOneShot(jumpSound, 0.4f);
            canjump = false;
            isjumping = true;
            rb.AddForce(0, jumpSpeed, 0, ForceMode.Impulse);
        }
    }

    void JumpDelay()
    {
        if (isjumping && jumpCountDown > 0)
            jumpCountDown -= Time.deltaTime;
        else
        {
            canjump = true;
            isjumping = false;
            jumpCountDown = jumpDelay;
        }
    }

    IEnumerator OnGroundFalse(float time)
    {
        if (executingOnGroundFalse) yield break;
        executingOnGroundFalse = true;

        yield return new WaitForSeconds(time);

        onGround = false;
        executingOnGroundFalse = false;
    }

private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collided with type of " + other.gameObject.tag);
        if (other.gameObject.tag == "Food")
        {
            gameManager.UpdateScore(100);
            Destroy(other.gameObject);
            playerAudio.PlayOneShot(chompSound, 0.4f);
            explosionParticle.Play();
            explosionParticle2.Play();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Hit " + collision.gameObject.name);
        //Debug.Log("Hit " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Ground")
        {
            explosionParticle3.Play();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        //Debug.Log("Touching " + collision.gameObject.name);
        //Debug.Log("Touching " + collision.gameObject.tag);

        if (collision.gameObject.tag == "Ground")
        {
            onGround = true;
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            StartCoroutine(OnGroundFalse(delayTimeOnGroundFalse));
        }
    }
}
