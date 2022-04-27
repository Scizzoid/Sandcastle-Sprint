using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 0.0f;
    public float jumpHeight = 200.0f;
    public int curJumps = 1;
    public GameObject startTextObj;
    public GameObject restartTextObj;
    public GameObject LoseTextObj;
    public GameObject WinTextObj;

    private bool started = false;
    private bool finished = false;
    private bool ducking = false;
    private float fallTimer = 0.0f;
    private float finishedTimer;
    private float loseRotation = -25.0f;
    private int defJumps;
    private Rigidbody rb;
    private TextMeshProUGUI restartText;

    void Start()
    {
        WinTextObj.SetActive(false);
        LoseTextObj.SetActive(false);
        restartTextObj.SetActive(false);
        startTextObj.SetActive(true);

        // Store ammount of jumps
        defJumps = curJumps;

        rb = GetComponent<Rigidbody>();
        restartText = restartTextObj.GetComponent<TMPro.TextMeshProUGUI>();
    }

    void OnJump()
    {
        // Press jump to start
        if (!started)
        {
            started = true;
            startTextObj.SetActive(false);
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

        if (finished)
        {
            restartTextObj.SetActive(true);
            finishedTimer -= Time.deltaTime;
            restartText.text = "Restart in " + Mathf.Round(finishedTimer).ToString();
            if (finishedTimer <= 0.0f)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Goal Trigger")
        {
            WinTextObj.SetActive(true);

            // Stop player
            speed = 0.0f;
            jumpHeight = 0.0f;

            finishedTimer = 5.0f;
            finished = true;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.name == "Runway")
        {
            // Reset number of jumps when colliding the ground
            curJumps = defJumps;
        }  

        if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log("Collision");
            LoseTextObj.SetActive(true);
            speed = 0.0f;
            jumpHeight = 0.0f;

            rb.freezeRotation = false;
            if (other.gameObject.name.Contains("Floating"))
            {
                // Player will rotate backwards and fall over
                Quaternion target = Quaternion.Euler(0.0f, 0.0f, loseRotation);
                
                while(fallTimer < 5.0f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * 0.04f);
                    fallTimer += Time.deltaTime;
                }
                    
            }

            else
            {
                // Player will be pushed forward and fall over
                rb.velocity = new Vector3(-2, 0, 0);
            }
            finishedTimer = 3.0f;
            finished = true;
        }
    }
}
