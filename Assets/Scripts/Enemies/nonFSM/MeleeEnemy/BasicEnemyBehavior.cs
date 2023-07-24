using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyBehavior : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private GameObject investigateTransform;

    [Header("Movement")]
    [SerializeField]
    private float baseMoveSpeed;
    [SerializeField]
    private float chaseMoveSpeed;
    [SerializeField]
    private List<Transform> patrolPoints;

    [Header("Combat")]
    [SerializeField]
    private float attackDamage;
    [SerializeField]
    private float maxHealth;
    [SerializeField]
    private float dashAttackSpeed;

    private float currentHealth;

    private bool playerSpotted;
    private bool isInvestigating;
    private bool dashed;

    private Transform travelTarget;

    private float activeMoveSpeed;

    public float getcurrentHealth()
    {
        return currentHealth;
    }

    public void applyDamage(float damage)
    {
        currentHealth -= damage;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        if(patrolPoints.Count != 0)
        {
            travelTarget = patrolPoints[0];
        }
        activeMoveSpeed = baseMoveSpeed;
        dashed = false;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Move towards the current destination
        rb.AddForce((travelTarget.position - rb.position).normalized * activeMoveSpeed, ForceMode.Force);

        //Check if its reached the destination if its a waypoint
        if (travelTarget != GlobalVariableStorage.instance.playerLocation)
        {
            if((travelTarget.position - rb.position).magnitude < 1.0f)
            {
                if(patrolPoints.Count > patrolPoints.IndexOf(travelTarget) + 1)
                {
                    travelTarget = patrolPoints[patrolPoints.IndexOf(travelTarget) + 1];
                }
                else
                {
                    travelTarget = patrolPoints[0];
                }
            }
        }
        else if (isInvestigating)
        {
            if((travelTarget.position - rb.position).magnitude < 1.0f)
            {
                StartCoroutine(investigateWait());
            }
        }

        //Cast a ray to the player to check if they're within spotting range
        RaycastHit hit;
        if(Physics.Raycast(rb.position, GlobalVariableStorage.instance.playerLocation.position - rb.position, out hit, 15))
        {
            Debug.DrawRay(rb.position, hit.transform.position - rb.position, Color.red);
            if (hit.collider.CompareTag("Player"))
            {
                travelTarget = GlobalVariableStorage.instance.playerLocation;
                activeMoveSpeed = chaseMoveSpeed;
                playerSpotted = true;
            }
            else
            {
                if (playerSpotted)
                {
                    playerLost();
                }
            }

        }
        else
        {
            if (playerSpotted)
            {
                playerLost();
            }
        }

        if (playerSpotted)
        {
            //Cast a ray to the player checking if the enemy should dash into them
            if (Physics.Raycast(rb.position, GlobalVariableStorage.instance.playerLocation.position - rb.position, out hit, 5) && ! dashed)
            {
                rb.AddForce((GlobalVariableStorage.instance.playerLocation.position - rb.transform.position).normalized * dashAttackSpeed, ForceMode.VelocityChange);
                StartCoroutine(dashAttackWait());
            }
        }

        Debug.DrawRay(rb.position, travelTarget.position - rb.position, Color.green);
    }

    private void playerLost()
    {
        playerSpotted = false;
        isInvestigating = true;
        //travelTarget = GameObject.Instantiate(empty, GlobalVariableStorage.instance.playerLocation.position, Quaternion.identity).transform;
        investigateTransform.transform.position = new Vector3(GlobalVariableStorage.instance.playerLocation.position.x, GlobalVariableStorage.instance.playerLocation.position.y, GlobalVariableStorage.instance.playerLocation.position.z);
        travelTarget = investigateTransform.transform;
    }

    private IEnumerator investigateWait()
    {
        yield return new WaitForSeconds(2.0f);
        if (!playerSpotted)
        {
            isInvestigating = false;
            //should grab nearest patrol point instead
            travelTarget = patrolPoints[0];
            activeMoveSpeed = baseMoveSpeed;
            yield return null;
        }
        else
        {
            yield return null;
        }
    }

    private IEnumerator dashAttackWait()
    {
        dashed = true;
        yield return new WaitForSeconds(1.5f);
        dashed = false;
        yield return null;
    }

}
