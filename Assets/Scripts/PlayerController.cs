using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 0.0f;
    public float jumpHeight = 200.0f;
    public int curJumps = 1;
    public GameObject LoseTextObj;
    public GameObject WinTextObj;
    private bool started = false;
    private bool ducking = false;
    private int defJumps;
    private Rigidbody rb;

    void Start()
    {
        WinTextObj.SetActive(false);
        LoseTextObj.SetActive(false);
        // Store ammount of jumps
        defJumps = curJumps;

        rb = GetComponent<Rigidbody>();
    }

    void OnJump()
    {
        // Press jump to start
        if (!started)
        {
            started = true;
            speed = 3.6f;
        }

        else
        {
            if (curJumps > 0)
            {
                Vector3 jump = new Vector3(0.0f, jumpHeight, 0.0f);
                rb.AddForce(jump);
                curJumps--;
            }
        }
    }

    public void OnDuck()
    {
        if (started)
        {
            // Toggle Ducking when duck button is pressed or released
            ducking = !ducking;

            if (ducking && transform.localScale.y > 0.2f)
            {
                transform.localScale += new Vector3(0.0f, -0.2f, 0.0f);

                // Lower jump height so player must un-duck to jump over obstacles
                jumpHeight = 100.0f;
            }

            else if (!ducking && transform.localScale.y < 0.4f)
            {
                transform.localScale += new Vector3(0.0f, 0.2f, 0.0f);
                jumpHeight = 200.0f;
            }
        }
    }

    void FixedUpdate()
    {
        // Left is technically forward on the track
        transform.Translate(Vector3.left * Time.deltaTime * speed);
        
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Runway")
        {
            // Reset number of jumps when colliding the ground
            curJumps = defJumps;
        }
        if (other.gameObject.CompareTag("Goal"))
        {
            WinTextObj.SetActive(true);
        }
        if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Collision");
            LoseTextObj.SetActive(true);
        }
    }
}
