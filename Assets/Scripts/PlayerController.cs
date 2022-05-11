using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 0.0f;
    public float duckSpeed = 0.005f;
    public float jumpHeight = 200.0f;
    public int curJumps = 1;
    public GameObject playerModel;
    public GameObject startTextObj;
    public GameObject directionsTextObj;
    public GameObject restartTextObj;
    public GameObject LoseTextObj;
    public GameObject WinTextObj;

    private bool started = false;
    private bool finished = false;
    private bool attacking = false;
    private bool ducking = false;
    private bool grounded = false;
    private bool CRRunning = false;
    private float fallTimer = 0.0f;
    private float restartTimer;
    private float loseRotation = -25.0f;
    private int defJumps;
    private Rigidbody rb;
    private TextMeshProUGUI restartText;
    private Animator playerAnimator;

    void Start()
    {
        // Set UI object states
        WinTextObj.SetActive(false);
        LoseTextObj.SetActive(false);
        restartTextObj.SetActive(false);
        directionsTextObj.SetActive(true);
        startTextObj.SetActive(true);

        // Store ammount of jumps
        defJumps = curJumps;

        // Accessed components
        rb = GetComponent<Rigidbody>();
        playerAnimator = playerModel.GetComponent<Animator>();
        restartText = restartTextObj.GetComponent<TMPro.TextMeshProUGUI>();

    }

//----------------------------------JUMP----------------------------------

    void OnJump()
    {
        // Press jump to start
        if (!started)
        {
            started = true;
            playerAnimator.SetBool("GameStarted", started);
            startTextObj.SetActive(false);
            directionsTextObj.SetActive(false);
            speed = 3.6f;
        }

        else
        {
            if (curJumps > 0)
            {
                if (curJumps == defJumps) // First Jump
                {
                    playerAnimator.SetBool("Jumped", true);
                    Vector3 jump = new Vector3(0.0f, jumpHeight, 0.0f);
                    rb.AddForce(jump);
                }

                else // All Jumps after
                {
                    playerAnimator.SetBool("MultiJump", true);
                    Vector3 jump = new Vector3(0.0f, jumpHeight * 0.5f, 0.0f);
                    rb.AddForce(jump);
                }
                curJumps--;
            }
        }
    }

//------------------------------------------------------------------------

//---------------------------------ATTACK---------------------------------

    void OnAttack()
    {
        if (curJumps >= defJumps-1 && !finished) // No Attacking on double jump or when finished
        {
            playerAnimator.SetBool("Attacking", true);
            attacking = true;
            StartCoroutine(attackCooldown());
        }
        
    }

    private IEnumerator attackCooldown()
    {
        yield return new WaitForSeconds(0.6f);
        playerAnimator.SetBool("Attacking", false);
        attacking = false;
    }

//------------------------------------------------------------------------

//----------------------------------DUCK----------------------------------

    public void OnDuck()
    {
        if (started)
        {
            // Toggle Ducking when duck button is pressed or released
            ducking = !ducking;

            if (ducking && !finished && !CRRunning)
            {
                // Lower jump height so player must un-duck to jump over obstacles
                jumpHeight = 150.0f;
                StartCoroutine(duckCoroutine());
            }

            else if (!ducking && !finished && !CRRunning)
            {
                jumpHeight = 200.0f;
                StartCoroutine(unduckCoroutine());
            }
        }
    }

    private IEnumerator duckCoroutine()
    {
        while (true && transform.localScale.y > 0.2f)
        {
            CRRunning = true;

            // Shrink the player
            transform.localScale += new Vector3(-0.5f * duckSpeed, -1.0f * duckSpeed, -0.5f * duckSpeed);
            if (grounded)
            {
                // Move player down to stay on ground
                transform.Translate(Vector3.up * -1.0f * duckSpeed);
            }

            yield return null;
            CRRunning = false;
        }
    }

    private IEnumerator unduckCoroutine()
    {
        while (true && transform.localScale.y < 0.4f)
        {
            CRRunning = true;

            // Grow the player
            transform.localScale += new Vector3(0.5f * duckSpeed, duckSpeed, 0.5f * duckSpeed);

            yield return null;
            CRRunning = false;
        }
    }

//------------------------------------------------------------------------

    void FixedUpdate()
    {
        // Left is technically forward on the track
        transform.Translate(Vector3.left * Time.deltaTime * speed);

        if (finished)
        {
            restartTextObj.SetActive(true);
            StartCoroutine(unduckCoroutine());

            restartTimer -= Time.deltaTime;
            restartText.text = "Restarting in " + Mathf.Round(restartTimer).ToString();
            if (restartTimer <= 0.0f)
            {
                // Restart current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

//------------------------COLLISIONS & TRIGGERS---------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Goal Trigger")
        {
            WinTextObj.SetActive(true);

            // Stop player
            speed = 0.0f;
            jumpHeight = 0.0f;
            restartTimer = 5.0f;
            finished = true;
            playerAnimator.SetBool("GameWon", finished);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Ground") && !finished)
        {
            // Reset number of jumps when colliding the ground
            curJumps = defJumps;
            grounded = true;
            playerAnimator.SetBool("Jumped", false);
            playerAnimator.SetBool("MultiJump", false);
        }

        if (other.gameObject.CompareTag("Obstacle") && !finished)
        {
            if (other.gameObject.name.Contains("Breakable") && attacking)
            {
                other.gameObject.SetActive(false); //Sucessfully destroyed obstacle
            }

            else
            {
                playerAnimator.SetBool("GameLost", true);
                LoseTextObj.SetActive(true);
                speed = 0.0f;
                jumpHeight = 0.0f;

                rb.freezeRotation = false;
                if (other.gameObject.name.Contains("Floating") || other.gameObject.name.Contains("Breakable"))
                {
                    // Player will rotate backwards and fall over
                    Quaternion target = Quaternion.Euler(0.0f, 0.0f, loseRotation);

                    while (fallTimer < 5.0f)
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
                restartTimer = 4.0f;
                finished = true;
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            grounded = false;
        }
    }
}

//------------------------------------------------------------------------
