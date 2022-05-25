using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public bool attackUnlocked = false;
    public float speed = 0.0f;
    public float scaleSpeed = 0.03f;
    public float jumpHeight = 4.0f;
    public int curJumps = 1;
    public GameObject playerModel;
    public GameObject startTextObj;
    public GameObject directionsTextObj;
    public GameObject unlockedTextObj;
    public GameObject FinishedTextObj;
    public GameObject LoseTextObj;
    public GameObject WinTextObj;
    public AudioSource footsteps;
    public AudioSource kick;

    private bool started = false;
    private bool finished = false;
    private bool won = false;
    private bool attacking = false;
    private bool shrinking = false;
    private bool grounded = true;
    private bool shrinkCR = false;
    private bool growCR = false;
    private bool footstepsPlaying = false;
    private float fallTimer = 0.0f;
    private float finishedTimer;
    private float loseRotation = -25.0f;
    private float defJumpHeight;
    private int defJumps;
    private Rigidbody rb;
    private TextMeshProUGUI FinishedText;
    private Animator playerAnimator;
    private Coroutine shrink;
    private Coroutine grow;

    void Start()
    {
        // Set UI object states
        WinTextObj.SetActive(false);
        LoseTextObj.SetActive(false);
        FinishedTextObj.SetActive(false);
        directionsTextObj.SetActive(true);
        startTextObj.SetActive(true);
        if (!(SceneManager.GetActiveScene().name == "Beta Level 1"))
        {
            unlockedTextObj.SetActive(true);
        }

        // Store ammount of jumps and jumpHeight
        defJumps = curJumps;
        defJumpHeight = jumpHeight;

        // Accessed components
        rb = GetComponent<Rigidbody>();
        playerAnimator = playerModel.GetComponent<Animator>();
        FinishedText = FinishedTextObj.GetComponent<TMPro.TextMeshProUGUI>();

    }

    void FixedUpdate()
    {
        // Left is technically forward on the track
        transform.Translate(Vector3.left * Time.deltaTime * speed);

        // Play footstep SFX
        if (grounded && !finished && started && footstepsPlaying == false)
        {
            footsteps.Play();
            footstepsPlaying = true;
        }

        // Stop footstep SFX
        else if (!grounded || finished)
        {
            footsteps.Stop();
            footstepsPlaying = false;
        }

        // Display finish text
        if (finished)
        {
            FinishedTextObj.SetActive(true);
            finishedTimer -= Time.deltaTime;

            if (won && !(SceneManager.GetActiveScene().name == "Beta Level 3"))
            {
                FinishedText.text = "Next Level in " + Mathf.Round(finishedTimer).ToString();
            }

            else
            {
                FinishedText.text = "Restarting in " + Mathf.Round(finishedTimer).ToString();
            }

            if (finishedTimer <= 0.0f)
            {
                // Next level
                if (won && !(SceneManager.GetActiveScene().name == "Beta Level 3"))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }

                // Restart game
                else if (won && SceneManager.GetActiveScene().name == "Beta Level 3")
                {
                    SceneManager.LoadScene(0);
                }
                // Restart scene
                else
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }
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
            if (!(SceneManager.GetActiveScene().name == "Beta Level 1"))
            {
                unlockedTextObj.SetActive(false);
            }
            speed = 3.6f;
        }

        else
        {
            if (curJumps > 0)
            {
                // First Jump
                if (curJumps == defJumps) 
                {
                    playerAnimator.SetBool("Jumped", true);
                    Vector3 jump = new Vector3(0.0f, jumpHeight, 0.0f);
                    rb.velocity = jump;
                }

                // All Jumps after
                else
                {
                    playerAnimator.SetBool("MultiJump", true);
                    Vector3 jump = new Vector3(0.0f, jumpHeight * 0.75f, 0.0f);
                    rb.velocity = jump;
                }
                curJumps--;
            }
        }
    }

//------------------------------------------------------------------------

//---------------------------------ATTACK---------------------------------

    void OnAttack()
    {
        // No Attacking on double jump or when finished
        if (attackUnlocked && curJumps >= defJumps-1 && !finished)
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

    // For rotating surfboards when attacked
    private IEnumerator rotateCoroutine(Collision other, Vector3 rotateoffset)
    {
        while (true && other.gameObject.transform.eulerAngles.z > 175.0f)
        {
            other.gameObject.transform.RotateAround(rotateoffset, Vector3.forward, 250.0f * Time.deltaTime);

            yield return new WaitForFixedUpdate();
        }
        other.gameObject.SetActive(false);
    }

//------------------------------------------------------------------------

//---------------------------------SHRINK---------------------------------

    public void OnShrink()
    {
        if (started)
        {
            // Toggle shrinking when shrink button is pressed or released
            shrinking = !shrinking;

            if (shrinking && !finished)
            {
                // Lower jump height so player must grow to jump over obstacles
                jumpHeight = defJumpHeight * 0.5f;
                // If growing, stop growing
                if (growCR) { StopCoroutine(grow); }
                shrink = StartCoroutine(shrinkCoroutine());
            }

            else if (!shrinking && !finished)
            {
                // Reset jump height
                jumpHeight = defJumpHeight;
                // If shrinking, stop shrinking
                if (shrinkCR) { StopCoroutine(shrink); }
                grow = StartCoroutine(growCoroutine());
            }
        }
    }

    private IEnumerator shrinkCoroutine()
    {
        while (transform.localScale.y > 0.2f)
        {
            shrinkCR = true;
            // Shrink the player
            transform.localScale += new Vector3(-0.5f * scaleSpeed,
                                                -1.0f * scaleSpeed,
                                                -0.5f * scaleSpeed);
            // If touching ground, move player down to stay on ground
            if (grounded) { transform.Translate(Vector3.up * -1.0f * scaleSpeed); }
            footsteps.volume += -3.0f * scaleSpeed;
            yield return new WaitForFixedUpdate();
        }
        shrinkCR = false;
    }

    private IEnumerator growCoroutine()
    {
        while (transform.localScale.y < 0.4f)
        {
            growCR = true;
            // Grow the player
            transform.localScale += new Vector3(0.5f * scaleSpeed,
                                                       scaleSpeed,
                                                0.5f * scaleSpeed);
            footsteps.volume += 3.0f * scaleSpeed;
            yield return new WaitForFixedUpdate();
        }
        growCR = false;
    }

//------------------------------------------------------------------------

//----------------------------LEVEL SHORTCUTS-----------------------------

    void OnLevel1()
    {
        SceneManager.LoadScene(0);
    }

    void OnLevel2()
    {
        SceneManager.LoadScene(1);
    }

    void OnLevel3()
    {
        SceneManager.LoadScene(2);
    }

//------------------------------------------------------------------------

//------------------------COLLISIONS & TRIGGERS---------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Goal Trigger")
        {
            WinTextObj.SetActive(true);

            // Stop player
            speed = 0.0f;
            jumpHeight = 0.0f;
            finishedTimer = 5.0f;
            finished = won = true;
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
                Vector3 rotateoffset = new Vector3(other.gameObject.transform.position.x + 0.25f,
                                                   other.gameObject.transform.position.y - 0.75f,
                                                   other.gameObject.transform.position.z);
                
                StartCoroutine(rotateCoroutine(other, rotateoffset));
                kick.Play();

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
                finishedTimer = 3.0f;
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
