using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField]
    private PhysicMaterial playerMat;

    public bool grounded = false;
    public bool collisionHappened = false;
    private int maxJumps;
    private float playerRadius;
    float initFriction;
    private void Start()
    {
        maxJumps = GetComponentInParent<PlayerController>().maxJumps;
        playerRadius = gameObject.GetComponent<SphereCollider>().radius;
        initFriction = playerMat.dynamicFriction;
    }
    private void OnCollisionStay(Collision collision)
    {
        collisionHappened = true;
        if (collision.gameObject.CompareTag("Terrain"))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, new Vector3(0.0f, -1.0f, 0.0f), out hit, 1.0f))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    grounded = true;
                    playerMat.dynamicFriction = initFriction;
                    playerMat.staticFriction = initFriction;
                }
            }
            else
            {
                grounded = false;
                playerMat.dynamicFriction = 0;
                playerMat.staticFriction = 0;
            }
        }
        else
        {
            grounded = false;
            playerMat.dynamicFriction = 0;
            playerMat.staticFriction = 0;
            //Debug.Log("Not Grounded!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        RaycastHit hit;
        //if(Physics.Raycast(transform.position, new Vector3(0.0f,-1.0f,0.0f), out hit, 1.0f))
        if(Physics.BoxCast(transform.position, new Vector3(playerRadius, 0.1f, playerRadius), Vector3.down, out hit, Quaternion.identity, playerRadius))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                GetComponentInParent<PlayerController>().jumpsRemaining = maxJumps;
            }
        }
    }

    private void Update()
    {
        RaycastHit hit;
        //if (Physics.Raycast(transform.position, new Vector3(0.0f, -1.0f, 0.0f), out hit, 1.0f))
        if (Physics.BoxCast(transform.position, new Vector3(playerRadius, 0.1f, playerRadius), Vector3.down, out hit, Quaternion.identity, playerRadius))
        {
            if (hit.collider.CompareTag("Terrain"))
            {
                GetComponentInParent<PlayerController>().jumpsRemaining = maxJumps;
            }
        }
        else
        {
            grounded = false;
            playerMat.dynamicFriction = 0;
            playerMat.staticFriction = 0;
        }
        //the ray used to confirm we're grounded;
        Debug.DrawRay(transform.position, new Vector3(0.0f,-1.0f,0.0f));
    }

    private void FixedUpdate()
    {

    }
}
