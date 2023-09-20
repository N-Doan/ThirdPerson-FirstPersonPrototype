using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField]
    private float moveForce = 100.0f;
    [SerializeField]
    private float strafeForce = 50.0f;
    [SerializeField]
    private float walkerMoveSpeed = 1.2f;
    [SerializeField]
    private float maxVelocity = 15.0f;
    [SerializeField]
    private float dragCoefficient = 0.47f;

    [Header("Jumping")]
    [SerializeField]
    private float jumpForce = 51.22f;
    [SerializeField]
    public int maxJumps = 2;

    [Header("Dashing")]
    [SerializeField]
    private float dashForce = 14.6f;
    [SerializeField]
    public int maxDashes = 2;
    [SerializeField]
    private float dashWaitTime = 1.05f;

    [SerializeField]
    private Transform cameraT;

    private Rigidbody rb;
    private float maxStandardVelocity;
    private float maxDashVelocity;
    private float maxLaunchVelocity;
    [HideInInspector]
    public int jumpsRemaining;
    [HideInInspector]
    public int dashesRemaining;

    private PlayerCombatManager combatManager;
    GroundCheck g;
    CameraManager cam;

    private float playerRadius;

    enum Mode {Ball, Walker};
    Mode activeMode;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponentInChildren<Rigidbody>();
        playerRadius = rb.gameObject.GetComponent<SphereCollider>().radius;
        g = gameObject.GetComponentInChildren<GroundCheck>();
        activeMode = Mode.Ball;
        dashesRemaining = maxDashes;
        maxStandardVelocity = maxVelocity;
        maxDashVelocity = maxVelocity * 3;
        maxLaunchVelocity = maxLaunchVelocity * 5;
        cam = GetComponent<CameraManager>();
        combatManager = GetComponent<PlayerCombatManager>();
    }

    private void Update()
    {
        //Swap between walker mode and BALL mode
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (dashesRemaining > 0)
            {
                string Dir = null;
                if (Input.GetKey(KeyCode.W))
                {
                    Dir = Dir + 'W';
                }
                if (Input.GetKey(KeyCode.S))
                {
                    Dir = Dir + 'S';
                }
                if (Input.GetKey(KeyCode.A))
                {
                    Dir = Dir + 'A';
                }
                if (Input.GetKey(KeyCode.D))
                {
                    Dir = Dir + 'D';
                }
                if (Dir != null)
                {
                    StartCoroutine(performDash(Dir));
                }
            }
        }
        if (activeMode == Mode.Ball && g.grounded && Input.GetKeyDown(KeyCode.Space))
        {
             StartCoroutine(performJump());
            
        }

        else if (!g.grounded && jumpsRemaining == maxJumps && Input.GetKeyDown(KeyCode.Space))
        {
            string playerInputJumpDir = null;

            if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
            {
                playerInputJumpDir = playerInputJumpDir + 'W';
            }
            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
            {
                playerInputJumpDir = playerInputJumpDir + 'A';
            }
            if (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
            {
                playerInputJumpDir = playerInputJumpDir + 'S';
            }
            if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
            {
                playerInputJumpDir = playerInputJumpDir + 'D';
            }
            StartCoroutine(performDoubleJump(playerInputJumpDir));
            jumpsRemaining = 0;
        }

        else if(activeMode == Mode.Ball && Input.GetKeyDown(KeyCode.Space) && jumpsRemaining < maxJumps && jumpsRemaining > 0)
        {
            string playerInputJumpDir = null;

            if (Input.GetKey(KeyCode.W))
            {
                playerInputJumpDir = playerInputJumpDir + 'W';
            }
            if (Input.GetKey(KeyCode.A))
            {
                playerInputJumpDir = playerInputJumpDir + 'A';
            }
            if (Input.GetKey(KeyCode.S))
            {
                playerInputJumpDir = playerInputJumpDir + 'S';
            }
            if (Input.GetKey(KeyCode.D))
            {
                playerInputJumpDir = playerInputJumpDir + 'D';
            }
            StartCoroutine(performDoubleJump(playerInputJumpDir));
        }
        //Scoping and Firing Bullets
        if (Input.GetMouseButtonDown(0))
        {
            if (cam.scoped)
            {
                combatManager.spawnBullet(cameraT);
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (cam.scoped)
            {
                combatManager.stopLaser();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (cam.scoped)
            {
                combatManager.stopLaser();
            }
            cam.swapCamera();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            GlobalVariableStorage.instance.checkpointManager.takePlayerToActiveCheckpoint();
            //zero out velocity
            rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
            rb.angularVelocity = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //cameraT.forward = new Vector3(cameraT.forward.x, 0, cameraT.forward.z);
        //cameraT.right = new Vector3(cameraT.right.x, 0, cameraT.right.z);
        //BALL MOVEMENTS
        if (activeMode == Mode.Ball)
        {
            Vector3 unclampedMoveForce = Vector3.zero;
            if (g.grounded)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    //rb.AddForce(cameraT.transform.forward * moveForce, ForceMode.Acceleration);
                    unclampedMoveForce += (cameraT.transform.forward);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    //rb.AddForce(-cameraT.transform.right * moveForce, ForceMode.Acceleration);
                    unclampedMoveForce += (-cameraT.transform.right);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    //rb.AddForce(-cameraT.transform.forward * moveForce, ForceMode.Acceleration);
                    unclampedMoveForce += (-cameraT.transform.forward);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    //rb.AddForce(cameraT.transform.right * moveForce, ForceMode.Acceleration);
                    unclampedMoveForce += (cameraT.transform.right);
                }
                unclampedMoveForce = unclampedMoveForce.normalized * moveForce;
            }
            else
            {
                if (Input.GetKey(KeyCode.W))
                {
                    //rb.AddForce(cameraT.transform.forward * strafeForce, ForceMode.Acceleration);
                    unclampedMoveForce += (cameraT.transform.forward);
                }
                if (Input.GetKey(KeyCode.A))
                {
                    //rb.AddForce(-cameraT.transform.right * strafeForce, ForceMode.Acceleration);
                    unclampedMoveForce += (-cameraT.transform.right);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    //rb.AddForce(-cameraT.transform.forward * strafeForce, ForceMode.Acceleration);
                    unclampedMoveForce += (-cameraT.transform.forward);
                }
                if (Input.GetKey(KeyCode.D))
                {
                    //rb.AddForce(cameraT.transform.right * strafeForce, ForceMode.Acceleration);
                    unclampedMoveForce += (cameraT.transform.right);
                }
                unclampedMoveForce = unclampedMoveForce.normalized * strafeForce;
            }
            StartCoroutine(clampVelocity(unclampedMoveForce));
        }
        //WALKER MOVEMENTS
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(new Vector3(cameraT.transform.forward.x, 0, cameraT.transform.forward.z) * walkerMoveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(-new Vector3(cameraT.transform.right.x, 0, cameraT.transform.right.z) * walkerMoveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(-new Vector3(cameraT.transform.forward.x, 0, cameraT.transform.forward.z) * walkerMoveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(new Vector3(cameraT.transform.right.x, 0, cameraT.transform.right.z) * walkerMoveSpeed * Time.deltaTime);
            }
        }

        if(!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D))
        {
            applyDrag();
        }
    }

    private void applyDrag()
    {
        float p = 1.225f;
        float area = Mathf.PI * playerRadius * playerRadius;
        float v = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z).magnitude;

        Vector3 direction = new Vector3(-rb.velocity.x, 0.0f, -rb.velocity.z).normalized;
        float force = (p * v * v * dragCoefficient * area) / 2;
        rb.AddForce(direction * force);
    }

    private void swapToWalker()
    {
        rb.velocity = new Vector3(0,0,0);
        rb.angularVelocity = new Vector3(0, 0, 0);
        rb.AddForce(Vector3.up * 8, ForceMode.VelocityChange);
        StartCoroutine(waitForApex());
    }

    private IEnumerator waitForApex()
    {
        Quaternion target = Quaternion.Euler(0, 0, 0);

        while (rb.velocity.y >= 0 && rb.transform.rotation != Quaternion.Euler(0,0,0))
        {
            yield return new WaitForEndOfFrame();
            rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, target, Time.deltaTime * 5.0f);
            
        }
        rb.transform.rotation = Quaternion.Euler(0, 0, 0);
        rb.isKinematic = true;
        activeMode = Mode.Walker;
        yield return null;
    }

    private void swapToBall()
    {
        activeMode = Mode.Ball;
        if (cam.scoped)
        {
            cam.swapCamera();
        }
        rb.isKinematic = false;
    }

    private IEnumerator performDash(string Dir)
    {
        yield return new WaitForEndOfFrame();
        cameraT.forward = new Vector3(cameraT.forward.x, 0, cameraT.forward.z);
        cameraT.right = new Vector3(cameraT.right.x, 0, cameraT.right.z);
        bool dashed = false;
        //Store velocity before dash
        Vector3 preDash = rb.velocity;
        rb.velocity = new Vector3(0.0f, rb.velocity.y, 0.0f);
        Vector3 totalDash = Vector3.zero;
        if(Dir != null)
        {
            while (Dir.Length > 0)
            {
                if (Dir.StartsWith("W"))
                {
                    totalDash += (cameraT.transform.forward.normalized * dashForce);
                    dashed = true;
                    if (Dir.Length > 1)
                    {
                        Dir = Dir.Substring(1);
                    }
                    else
                    {
                        Dir = string.Empty;
                    }
                }
                if (Dir.StartsWith("A"))
                {
                    totalDash += (-cameraT.transform.right.normalized * dashForce);
                    dashed = true;
                    if (Dir.Length > 1)
                    {
                        Dir = Dir.Substring(1);
                    }
                    else
                    {
                        Dir = string.Empty;
                    }
                }
                if (Dir.StartsWith("S"))
                {
                    totalDash += (-cameraT.transform.forward.normalized * dashForce);
                    dashed = true;
                    if (Dir.Length > 1)
                    {
                        Dir = Dir.Substring(1);
                    }
                    else
                    {
                        Dir = string.Empty;
                    }
                }
                if (Dir.StartsWith("D"))
                {
                    totalDash += (cameraT.transform.right.normalized * dashForce);
                    dashed = true;
                    if (Dir.Length > 1)
                    {
                        Dir = Dir.Substring(1);
                    }
                    else
                    {
                        Dir = string.Empty;
                    }
                }
            }

        }

            if (dashed)
            {
                EventManager.instance.OnPlayerDash(gameObject.transform.GetInstanceID(), true);
                totalDash = new Vector3(totalDash.x, 0, totalDash.z);
                dashesRemaining--;
                StartCoroutine(dashCooldown());
                rb.AddForce(totalDash, ForceMode.VelocityChange);
                StartCoroutine(setDashMaxVelocity(preDash));
            }
            
        yield return null;
    }

    private IEnumerator performJump()
    {
        yield return new WaitForFixedUpdate();
        jumpsRemaining--;
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        yield return null;
    }

    private IEnumerator performDoubleJump(string key)
    {

        /*Debug.Log("WORLD CAMERA");
        Debug.Log("Z:"+cameraT.transform.forward.z);
        Debug.Log("X:" + cameraT.transform.right.x);

        Debug.Log("LOCAL CAMERA");
        Debug.Log("Z:" + cameraT.InverseTransformDirection(cameraT.transform.forward).z);
        Debug.Log("X:" + cameraT.InverseTransformDirection(cameraT.transform.right).x);*/

        yield return new WaitForFixedUpdate();
        jumpsRemaining--;
        Vector3 jumpDir = Vector3.up;

        cameraT.forward = new Vector3(cameraT.forward.x, 0, cameraT.forward.z);
        cameraT.right = new Vector3(cameraT.right.x, 0, cameraT.right.z);

        if(key != null)
        {
            //Different Directions depending on which direction the camera is facing
            if(Mathf.Abs(cameraT.transform.forward.z) > Mathf.Abs(cameraT.transform.forward.x))
            {
                while (key.Length != 0)
                {
                    if (key.StartsWith("W"))
                    {
                        //check if signs match
                        if ((rb.velocity.z * cameraT.transform.forward.z) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(0,0,rb.velocity.z * -1);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + cameraT.transform.forward;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                    if (key.StartsWith("A"))
                    {
                        if ((rb.velocity.x * -cameraT.transform.right.x) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(rb.velocity.x * -1, 0, 0);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + -cameraT.transform.right;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                    if (key.StartsWith("S"))
                    {
                        if ((rb.velocity.z * -cameraT.transform.forward.z) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(0, 0, rb.velocity.z * -1);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + -cameraT.transform.forward;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                    if (key.StartsWith("D"))
                    {
                        if ((rb.velocity.x * cameraT.transform.right.x) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(rb.velocity.x * -1, 0, 0);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + cameraT.transform.right;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                }
            }
            else
            {
                while (key.Length != 0)
                {
                    if (key.StartsWith("W"))
                    {
                        //check if signs match
                        if ((rb.velocity.x * cameraT.transform.forward.x) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(0,0,rb.velocity.z * -1);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + cameraT.transform.forward;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                    if (key.StartsWith("A"))
                    {
                        if ((rb.velocity.z * -cameraT.transform.right.z) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(rb.velocity.x * -1, 0, 0);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + -cameraT.transform.right;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                    if (key.StartsWith("S"))
                    {
                        if ((rb.velocity.x * -cameraT.transform.forward.x) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(0, 0, rb.velocity.z * -1);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + -cameraT.transform.forward;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                    if (key.StartsWith("D"))
                    {
                        if ((rb.velocity.z * cameraT.transform.right.z) < 0)
                        {
                            //jumpDir = jumpDir + new Vector3(rb.velocity.x * -1, 0, 0);
                            //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                            rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
                        }
                        jumpDir = jumpDir + cameraT.transform.right;

                        if (key.Length > 1)
                        {
                            key = key.Substring(1);
                        }
                        else
                        {
                            key = string.Empty;
                        }
                    }
                }
            }
            
        }

        /*if (key == 'W')
        {
            if (rb.velocity.z < 0)
            {
                //jumpDir = jumpDir + new Vector3(0,0,rb.velocity.z * -1);
                //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
            }
            jumpDir = jumpDir + cameraT.transform.forward;
        }
        if (key == 'A')
        {
            if (rb.velocity.x > 0)
            {
                //jumpDir = jumpDir + new Vector3(rb.velocity.x * -1, 0, 0);
                //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
            }
            jumpDir = jumpDir + -cameraT.transform.right;
        }
        if (key == 'S')
        {
            if (rb.velocity.z > 0)
            {
                //jumpDir = jumpDir + new Vector3(0, 0, rb.velocity.z * -1);
                //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
            }
            jumpDir = jumpDir + -cameraT.transform.forward;
        }
        if (key == 'D')
        {
            if (rb.velocity.x < 0)
            {
                //jumpDir = jumpDir + new Vector3(rb.velocity.x * -1, 0, 0);
                //rb.velocity = new Vector3(0, rb.velocity.y, 0);
                rb.AddForce(new Vector3(-rb.velocity.x, 0, -rb.velocity.z), ForceMode.VelocityChange);
            }
            jumpDir = jumpDir + cameraT.transform.right;
        }
        rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);*/
        rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        //rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
        StartCoroutine(clampJumpVelocity(jumpDir * jumpForce));
        yield return null;
    }

    private IEnumerator clampVelocity(Vector3 unclampedMoveForce)
    {
        yield return new WaitForFixedUpdate();

        //Vector3 clamped = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), maxVelocity);
        //rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
        Vector3 clamped = Vector3.ClampMagnitude(new Vector3(unclampedMoveForce.x, 0, unclampedMoveForce.z), maxVelocity);

        float maxSquaredVelocity = maxVelocity * maxVelocity;
        Vector3 velocityNoY = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
        /*if(velocityNoY.sqrMagnitude > maxSquaredVelocity)
        {
            Vector3 forward = Vector3.Project(clamped, velocityNoY);
            clamped -= forward;
        }*/

        //if travelling in same x direction
        if (Mathf.Abs(velocityNoY.x) > maxVelocity && velocityNoY.x * clamped.x > velocityNoY.x)
        {
            Vector3 forward = Vector3.Project(clamped, velocityNoY);
            clamped -= forward;
        }

        //if travelling in same z direction
        if (Mathf.Abs(velocityNoY.z) > maxVelocity && velocityNoY.z * clamped.z > velocityNoY.z)
        {
            Vector3 forward = Vector3.Project(clamped, velocityNoY);
            clamped -= forward;
        }

        rb.AddForce(clamped, ForceMode.Impulse);
        //if(rb.velocity.magnitude <= maxVelocity)
        //{

        //}
        //else
        //{
        //Debug.Log("FAIL");
        //}
        yield return null;
    }

    private IEnumerator clampJumpVelocity(Vector3 unclampedMoveForce)
    {
        yield return new WaitForFixedUpdate();
        //Vector3 clamped = Vector3.ClampMagnitude(new Vector3(rb.velocity.x, 0, rb.velocity.z), maxVelocity);
        //rb.velocity = new Vector3(clamped.x, rb.velocity.y, clamped.z);
        Vector3 clamped = Vector3.ClampMagnitude(new Vector3(unclampedMoveForce.x, 0, unclampedMoveForce.z), maxVelocity);
        rb.AddForce(new Vector3(clamped.x, unclampedMoveForce.y, clamped.z), ForceMode.Impulse);
        yield return null;
    }

    private IEnumerator setDashMaxVelocity(Vector3 preDashVelocity)
    {
        //maxVelocity = maxDashVelocity;
        yield return new WaitForSeconds(0.25f);
        EventManager.instance.OnPlayerDash(gameObject.transform.GetInstanceID(), false);
        //rb.velocity = preDashVelocity;
        StartCoroutine(lerpVelocity(preDashVelocity));
        yield return null;
    }

    private IEnumerator lerpVelocity(Vector3 preDashVelocity)
    {
        float lerpTime = 2.0f;
        float elaplsedTime = 0;
        while(rb.velocity.magnitude > preDashVelocity.magnitude)
        {
            elaplsedTime += Time.deltaTime;
            Vector3 lerped = Vector3.Lerp(rb.velocity, preDashVelocity, elaplsedTime/lerpTime);
            rb.velocity = new Vector3(lerped.x, rb.velocity.y, lerped.z);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z);
        yield return null;
    }

    /*public IEnumerator useLaunchedMaxVelocity()
    {
        maxVelocity = maxLaunchVelocity;
        while(rb.velocity.magnitude > 2)
        {
            yield return null;
            //Debug.Log(rb.velocity.magnitude);
        }
        maxVelocity = maxStandardVelocity;
        yield return null;
    }*/

    public void resetPlayer()
    {
        //zero out velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        //reset position
        rb.transform.localPosition = Vector3.zero;

        //back to TP
        if (cam.scoped)
        {
            cam.swapCamera();
        }

        StopAllCoroutines();

        //call reset on combat manager
        combatManager.resetPlayer();
    }

    private IEnumerator dashCooldown()
    {
        yield return new WaitForSeconds(dashWaitTime);
        dashesRemaining++;
    }
}
