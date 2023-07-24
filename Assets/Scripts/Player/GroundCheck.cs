using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public bool grounded = false;
    public bool collisionHappened = false;
    private int maxJumps;
    private float playerRadius;
    private void Start()
    {
        maxJumps = GetComponentInParent<PlayerController>().maxJumps;
        playerRadius = gameObject.GetComponent<SphereCollider>().radius;
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
                }
            }
            else
            {
                grounded = false;
            }
        }
        else
        {
            grounded = false;
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
        }
        //the ray used to confirm we're grounded;
        Debug.DrawRay(transform.position, new Vector3(0.0f,-1.0f,0.0f));
    }

    private void FixedUpdate()
    {

    }
}
