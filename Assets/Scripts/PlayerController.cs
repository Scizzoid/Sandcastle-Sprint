using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 0.0f;
    public float jumpHeight = 200.0f;
    public int curJumps = 1;

    private bool started = false;
    private bool ducking = false;
    private int defJumps;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        // Store ammount of jumps
        defJumps = curJumps;

        rb = GetComponent<Rigidbody>();
    }

    void OnJump()
    {
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
        }
    }

    void FixedUpdate()
    {
        // Left is technically forward on the track
        transform.Translate(Vector3.left * Time.deltaTime * speed);

        if (ducking && transform.localScale.y > 0.2f)
        {
            transform.localScale += new Vector3(0.0f, -0.2f, 0.0f);
            jumpHeight = 100.0f;
        }

        else if (!ducking && transform.localScale.y < 0.4f)
        {
            transform.localScale += new Vector3(0.0f, 0.2f, 0.0f);
            jumpHeight = 200.0f;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Runway")
        {
            // Reset number of jumps when colliding the ground
            curJumps = defJumps;
        }
    }
}
