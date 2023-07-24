using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Chasing state of Leaper AI. Handles Leaper movement and attacks through calculating jump target and launch direciton/velocity
 * for each jump
 */
public class LChasing : FSMState
{
    private LChaseAIProperties chaseAIProperties;
    private LeaperAI ai;
    public LeaperAI getAI() { return ai; }

    private float elapsedTime;
    private float elapsedJumpTime;
    private float activationHeight = float.MinValue;
    private float originalHeight;
    private bool reachedTarget = true;
    public bool hasReachedTarget() { return reachedTarget; }

    private float t;
    Vector3 jumpTarget;
    Color debugColor;

    public LChasing(LeaperAI controller, LChaseAIProperties properties)
    {
        stateID = StateIDs.Chasing;
        ai = controller;
        chaseAIProperties = properties;
    }

    public override void EnterStateInit(Transform player, Transform npc)
    {
        elapsedTime = 0.0f;
        ai.agent.enabled = true;
        if (!ai.agent.isOnNavMesh)
        {
            ai.agent.enabled = false;
            ai.agent.enabled = true;
        }
        base.EnterStateInit(player, npc);

        ai.rb.AddForce(Vector3.up*chaseAIProperties.activationForce, ForceMode.VelocityChange);
        originalHeight = ai.rb.transform.position.y;

        //anim activation
        ai.animController.activation();

        ai.leaperCoroutines.StartCoroutine(ai.leaperCoroutines.waitForApex(ai.rb, Mathf.Abs(chaseAIProperties.activationForce/Physics.gravity.y)));
        
        jumpTarget = getRandomNavmeshPoint(chaseAIProperties.jumpRange);
        debugColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
    }

    public override void Act(Transform player, Transform npc)
    {
        //Store the activation height when entering this stage (wait for jump to be complete)
        if (activationHeight == float.MinValue && chaseAIProperties.activated)
        {
            activationHeight = 0.5f;
        }
        if (chaseAIProperties.activated)
        {
            /*if (Vector3.Distance(jumpTarget, ai.rb.transform.position) < ai.agent.radius + activationHeight && reachedTarget == false)
            {
                ai.agent.isStopped = true;
                ai.agent.velocity = Vector3.zero;
                ai.rb.isKinematic = true;
                reachedTarget = true;
            }*/
            //if it hasn't reached the target yet and jumptime is over 0.35 seconds
            if(reachedTarget == false && elapsedJumpTime >= 0.1f)
            {
                RaycastHit hit;
                //Raycasts are origin, direction, outputhit, length, layer
                /*if(Physics.Raycast(ai.rb.transform.position, Vector3.down, out hit, activationHeight+0.5f))
                {
                    if (hit.collider.gameObject.CompareTag("Terrain"))
                    {
                        ai.agent.isStopped = true;
                        ai.agent.velocity = Vector3.zero;
                        ai.rb.isKinematic = true;
                        reachedTarget = true;
                    }
                }*/
                Collider[] hits = Physics.OverlapSphere(ai.rb.transform.position, 0.5f + activationHeight);
                if(hits.Length > 0)
                {
                    foreach(Collider collider in hits)
                    {
                        if (collider.gameObject.CompareTag("Terrain"))
                        {
                            
                            if (checkProx(collider))
                            {
                                break;
                            }

                        }
                    }
                }
            }
            //Timer for activating jump stopping for raycasts
            else if(reachedTarget == false && elapsedJumpTime <= 0.1f)
            {
                elapsedJumpTime += Time.fixedDeltaTime;
            }
            //Timer for time between jumps
            else if (reachedTarget == true)
            {
                //if the time between jumps is up
                if (elapsedTime >= chaseAIProperties.jumpIntervals)
                {
                    //anim jump
                    ai.animController.jumping();

                    elapsedTime = 0.0f;

                    //disable navmesh before starting jump
                    ai.agent.updatePosition = false;
                    ai.agent.updateRotation = false;
                    ai.agent.isStopped = true;

                    ai.rb.isKinematic = false;

                    jumpTarget = getRandomNavmeshPoint(chaseAIProperties.jumpRange);

                    //jumps to next point
                    Vector3 forceApplied = calcForce(ai.rb.transform.position, jumpTarget);
                    ai.rb.AddForce(forceApplied, ForceMode.VelocityChange);
                    //ai.rb.AddTorque(calcTorque(ai.rb.transform, jumpTarget, t), ForceMode.VelocityChange);
                    reachedTarget = false;
                    calcTorque(ai.rb.transform, jumpTarget, t);
                }
                else
                {
                    elapsedTime += Time.fixedDeltaTime;
                }
            }

            Debug.DrawLine(ai.rb.position, jumpTarget, debugColor);
        }
    }

    private Vector3 calcForce(Vector3 source, Vector3 target)
    {
        //if target is higher than start (or close enough)
        if(target.y - source.y >= -2.5f)
        {
            float angle = 0.0f;
            //get velocity along y
            float vy = Mathf.Sqrt(-2 * Physics.gravity.y * (Vector3.Distance(source, target)));
            //get time at apex of launch
            t = vy / Mathf.Abs(Physics.gravity.y);
            //get velocity along x. since we know x position would be 1/2 of the range we can solve this
            float vx = (target.x - source.x) / 2 / t;
            //get velocity along z. Same story.
            float vz = (target.z - source.z) / 2 / t;

            //get magnitude of vector
            float mag = Mathf.Sqrt(vx * vx + vy * vy + vz * vz);

            //if there's more than a 2.5 height difference, don't use default angle
            if (Mathf.Abs(target.y - source.y) <= 2.5f)
            {
                angle = Mathf.Deg2Rad * 45.0f;
            }
            else
            {
                //get angle
                angle = Mathf.Asin(vy / mag);
            }


            //get direct path of travel
            Vector3 direction = target - source;
            //Debug.Log(direction);

            //store height component of direct path
            float height = direction.y;
            direction.y = 0;
            float distanceBetween = direction.magnitude; //hypotneuse of vector without y component (because y component needs to change in order to launch)
                                                         //convert angle to radians (Don't have to anymore)
                                                         //float d2g = angle * Mathf.Deg2Rad;
            float d2g = angle;
            //Debug.Log(Mathf.Rad2Deg * angle);

            direction.y = distanceBetween * Mathf.Tan(d2g); // multiply hypotneuse with tan of missing y component to get proper y component
            distanceBetween += height / Mathf.Tan(angle); //re-adding the old y component to the distance between the target and current locations

            //ai.rb.AddTorque(calcTorque(ai.rb.transform, jumpTarget, t), ForceMode.VelocityChange);

            //velocity
            float velocity = Mathf.Sqrt(distanceBetween * Physics.gravity.magnitude / Mathf.Sin(2 * angle));
            direction = velocity * direction.normalized;
            if (!float.IsNaN(direction.x) && !float.IsNaN(direction.y) && !float.IsNaN(direction.z))
            {
                return direction;
            }
            else
            {
                Debug.Log("Invalid return force!");
                return Vector3.one;
            }
        }
        //if start is higher than target
        else
        {
            //Y init will be zero, so get T
            float t = Mathf.Sqrt(Mathf.Abs((target.y - source.y) / (-0.5f * Physics.gravity.y)));
            //get velocity for x direction
            float vx = (target.x - source.x) / t;
            //z direction
            float vz = (target.z - source.z) / t;

            Vector3 force = new Vector3(vx, 0.0f, vz);
            return force;

        }
    }

    private void calcTorque(Transform start, Vector3 target, float t)
    {
        //OverlapSphere to find terrain
        Quaternion targetTerrain = Quaternion.identity;

        RaycastHit hit;
        if(Physics.Raycast(start.position, target - start.position, out hit, chaseAIProperties.terrainLayerMask))
        {
            //gets the rotation for torque calculation
            if (hit.collider.CompareTag("Terrain"))
            {
                targetTerrain = Quaternion.FromToRotation(ai.transform.up, hit.normal);
            }
        }

        ai.leaperCoroutines.StartCoroutine(ai.leaperCoroutines.LerpRotation(this, ai.rb.transform, targetTerrain, t));

    }

    private Vector3 forTorque(Vector3 input)
    {
        Vector3 output = new Vector3();
        if(input.x > 180.0f)
        {
            output.x = input.x - 180;
        }
        if (input.y > 180.0f)
        {
            output.y = input.y - 180;
        }
        if (input.z > 180.0f)
        {
            output.z = input.z - 180;
        }
        return output;
    }
    /*
     Gotta add the check for jumping down (Raycast from rb to player that's jumplength long then raycast straight down to check for terrain (DONE)
     */
    private Vector3 getRandomNavmeshPoint(float range)
    {
        if (!ai.agent.isOnNavMesh)
        {
            ai.agent.enabled = false;
            ai.agent.enabled = true;
        }

        //Return player's location if they're in range
        if(Vector3.Distance(ai.rb.position, GlobalVariableStorage.instance.playerLocation.position) <= range)
        {
            return GlobalVariableStorage.instance.playerLocation.position;
        }
        //get random point within unit sphere
        Vector3 randomDirection = Random.insideUnitSphere * range;

        //randomDirection += ai.rb.transform.position;
        randomDirection = Vector3.Project(randomDirection, (ai.rb.transform.position - GlobalVariableStorage.instance.playerLocation.position).normalized);

        //Debug.Log(randomDirection.magnitude);
        if (randomDirection.magnitude <= chaseAIProperties.jumpRange * 0.6f)
        {
            randomDirection = randomDirection + new Vector3(chaseAIProperties.jumpRange * 0.6f, 0, chaseAIProperties.jumpRange * 0.6f);
        }

        randomDirection += Random.insideUnitSphere * 2;

        //use dot product to check if vectors are facing in the same direction (if D-P > 0 they are)
        if (Vector3.Dot(randomDirection, (ai.rb.transform.position - GlobalVariableStorage.instance.playerLocation.position).normalized) > 0)
        {
            randomDirection *= -1;
        }

        Vector3 downwardsOutput = Vector3.negativeInfinity;
        randomDirection += ai.rb.transform.position;
        //if there's a segnificant height difference between the ai and player
        if(Mathf.Abs(GlobalVariableStorage.instance.playerLocation.position.y - ai.rb.transform.position.y) > 1.5f)
        {
            //check if there's a navmesh surface below us that the player's on
            RaycastHit rayHit;
            if (Physics.Raycast(ai.rb.position, GlobalVariableStorage.instance.playerLocation.position - ai.rb.position, out rayHit, range))
            {
                downwardsOutput = downwardsOutput = rayHit.point;
            }
            else
            {
                if (Physics.Raycast(Vector3.Lerp(ai.rb.position, GlobalVariableStorage.instance.playerLocation.position,
                    range / Mathf.Abs(Vector3.Distance(ai.rb.position, GlobalVariableStorage.instance.playerLocation.position))), Vector3.down, out rayHit, chaseAIProperties.terrainLayerMask))
                {
                    downwardsOutput = rayHit.point;
                }
                {

                }
            }

            //Check if there's a wall blocking the path?
        }

        NavMeshHit hit;
        Vector3 output = ai.rb.position;
        if(NavMesh.SamplePosition(randomDirection, out hit, range, 1))
        {
            NavMeshPath path = new NavMeshPath();
            if (ai.agent.CalculatePath(hit.position, path))
            {
                output = hit.position;
                debugColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
            }
        }
        //if we bothered to get a downwards output
        if(!downwardsOutput.Equals(Vector3.negativeInfinity))
        {
            //Debug.DrawRay(ai.rb.position, downwardsOutput - ai.rb.position, Color.red, 5.0f);
            //if the downwards output is closer to the player choose it
            if (Vector3.Distance(output, GlobalVariableStorage.instance.playerLocation.position) < Vector3.Distance(downwardsOutput, GlobalVariableStorage.instance.playerLocation.position))
            {
                return output;
            }
            else
            {
                //add a little influence towards the player's location
                downwardsOutput += Random.insideUnitSphere * 2;
                return downwardsOutput;
            }
        }
        else
        {
            return output;
        }

    }

    //check each direction surrounding the rigidbody for a terrain collider using raycasts, first checking where it hits following its current velocity
    private bool checkProx(Collider collider)
    {
        RaycastHit colhit;

        /*if(Physics.Raycast(ai.rb.position, ai.rb.velocity.normalized, out colhit, 1.0f + activationHeight, chaseAIProperties.terrainLayerMask))
        {
            //Debug.Log("VELOCITY");
            return hitGround(colhit);
        }*/
        if(Physics.Raycast(ai.rb.position, Vector3.down, out colhit, 1.0f + activationHeight, chaseAIProperties.terrainLayerMask))
        {
            //Debug.Log("DOWN");
            return hitGround(colhit);
        }
        else if (Physics.Raycast(ai.rb.position, Vector3.left, out colhit, 1.0f + activationHeight, chaseAIProperties.terrainLayerMask))
        {
            //Debug.Log("LEFT");
            return hitGround(colhit);
        }
        else if (Physics.Raycast(ai.rb.position, Vector3.up, out colhit, 1.0f + activationHeight, chaseAIProperties.terrainLayerMask))
        {
            //Debug.Log("UP");
            return hitGround(colhit);
        }
        else if (Physics.Raycast(ai.rb.position, Vector3.right, out colhit, 1.0f + activationHeight, chaseAIProperties.terrainLayerMask))
        {
            //Debug.Log("RIGH");
            return hitGround(colhit);
        }
        else if (Physics.Raycast(ai.rb.position, Vector3.forward, out colhit, 1.0f + activationHeight, chaseAIProperties.terrainLayerMask))
        {
            //Debug.Log("FORWARD");
            return hitGround(colhit);
        }
        else if (Physics.Raycast(ai.rb.position, Vector3.back, out colhit, 1.0f + activationHeight, chaseAIProperties.terrainLayerMask))
        {
            //Debug.Log("BACK");
            return hitGround(colhit);
        }
        else
        {
            return false;
        }
    }

    private bool hitGround(RaycastHit colhit)
    {
        //Debug.DrawRay(ai.rb.position, ai.rb.velocity.normalized, Color.red, 5.0f);
        //Debug.DrawRay(colhit.point, colhit.normal, Color.green, 5.0f);
        ai.agent.velocity = Vector3.zero;
        ai.rb.isKinematic = true;
        //ai.agent.isStopped = true;
        reachedTarget = true;
        elapsedJumpTime = 0.0f;

        Vector3 dir = GlobalVariableStorage.instance.playerLocation.position - ai.rb.position;
        Quaternion.FromToRotation(ai.transform.up, Vector3.up);
        Quaternion norm = Quaternion.FromToRotation(ai.transform.up, colhit.normal.normalized);
        Quaternion rotPlayer = Quaternion.LookRotation(new Vector3(dir.x, dir.y, dir.z));
        Quaternion combinedRots;
        if (norm.eulerAngles == new Vector3(0, 0, 0))
        {
            combinedRots = norm * rotPlayer;
        }
        else
        {
            combinedRots = norm;
        }
        //Debug.Log(norm.eulerAngles);
        //Debug.Log("NORMAL: " + colhit.normal.normalized);
        //Debug.Log(colhit.collider.gameObject.name);

        ai.leaperCoroutines.StartCoroutine(ai.leaperCoroutines.hitGroundApplyRotation(combinedRots));

        //anim land
        ai.animController.landing();
        return true;
    }

    //Disable navmeshagent when leaving state!
    public override void Reason(Transform player, Transform npc)
    {
        /*RaycastHit hit;
        if (Physics.Raycast(ai.rb.position, GlobalVariableStorage.instance.playerLocation.position - ai.rb.position, out hit, 15))
        {
            Debug.DrawRay(ai.rb.position, hit.transform.position - ai.rb.position, Color.red);
            if (!hit.collider.CompareTag("Player"))
            {
                ai.PerformTransition(Transitions.PlayerLost);
            }
        }*/
    }
}
