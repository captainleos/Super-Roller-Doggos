using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mover : MonoBehaviour
{
    // Movement
    [SerializeField] GameObject vCamera;
    [SerializeField] float moveSpeed = 1f;
    public Rigidbody rb;

    // Jumping variables
    public float jumpSpeed = 3f;
    public float jumpDelay = 2f;
    private bool canjump;
    private bool isjumping;
    private float countDown;

    // Multiplayer
    public string inputID;

    // Debug
    public int foodScore;

    void Start()
    {
        canjump = true;
        countDown = jumpDelay;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isjumping && countDown > 0)
            countDown -= Time.deltaTime;
        else
        {
            canjump = true;
            isjumping = false;
            countDown = jumpDelay;
        }

        Input.GetAxis("Horizontal" + inputID);
        Input.GetAxis("Vertical" + inputID);
    }


    void FixedUpdate()
    {
        MovePlayer();

        if (Input.GetAxis("Jump" + inputID) > 0)
        {
            Jump();
        }
    }

    void MovePlayer()
    {
        float x = Input.GetAxis("Horizontal" + inputID);
        float z = Input.GetAxis("Vertical" + inputID);

        rb.AddRelativeForce(vCamera.transform.right * x * Time.deltaTime * moveSpeed);
        rb.AddRelativeForce(vCamera.transform.forward * z * Time.deltaTime * moveSpeed);
    }

    void Jump()
    {
        if (canjump)
        {
            canjump = false;
            isjumping = true;
            rb.AddForce(0, jumpSpeed, 0, ForceMode.Impulse);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with type of " + other.gameObject.tag);
        if (other.gameObject.tag == "Food")
        {
            foodScore++;
            Debug.Log("Eaten " + foodScore + " items!");
            Destroy(other.gameObject);
        }
    }
}
