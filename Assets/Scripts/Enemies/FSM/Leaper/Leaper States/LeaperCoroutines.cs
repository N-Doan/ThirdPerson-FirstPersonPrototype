using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*Helper class for containing all coroutines used by states. Attach to the same gameobject the ai is on*/

public class LeaperCoroutines : MonoBehaviour
{

    LeaperAI leaper;
    // Start is called before the first frame update
    void Start()
    {
        leaper = gameObject.GetComponent<LeaperAI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator waitForApex(Rigidbody rb, float t)
    {
        bool hitGround = false;
        /*while(rb.velocity.y >= 0)
        {
            yield return new WaitForFixedUpdate();
        }
        rb.isKinematic = true;
        leaper.lChaseAIProperties.activated = true;
        */
        Quaternion start = rb.rotation;
        Quaternion finish = Quaternion.Euler(0, 0, 0);
        float elapsedTime = 0.0f;

        //rotate into place
        while(elapsedTime <= 0.1) 
        {
            rb.MoveRotation(Quaternion.Euler(Vector3.Lerp(start.eulerAngles, finish.eulerAngles, elapsedTime / (t/0.1f))));
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        //wait until we get close to the ground
        while(elapsedTime <= 20.0f && !hitGround)
        {
            Collider[] col = Physics.OverlapSphere(rb.position, 1.5f);
            foreach(Collider collider in col)
            {
                if (collider.gameObject.CompareTag("Terrain"))
                {
                    hitGround = true;
                    
                }
            }
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        /*while(elapsedTime <= t)
        {
            rb.MoveRotation(Quaternion.Euler(Vector3.Lerp(start.eulerAngles, finish.eulerAngles, elapsedTime/t)));
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }*/

        rb.isKinematic = true;
        leaper.lChaseAIProperties.activated = true;
        yield return null;
    }

    public IEnumerator LerpRotation(LChasing chase, Transform start, Quaternion finishRot, float t)
    {
        Quaternion original = start.rotation;

        //Direction vector used for calculating rotation between player and ai. Pass this into Slerp to interpolate rotation
        Vector3 dir = GlobalVariableStorage.instance.playerLocation.position - start.position;

        float elapsedTime = 0.0f;

        //combine both desired rotations to create our FINAL TARGET ROTATION
        Quaternion combinedRots = finishRot * Quaternion.LookRotation(new Vector3(dir.x, dir.y, dir.z));

        while (elapsedTime <= t/2 && chase.hasReachedTarget() != true)
        {
            elapsedTime += Time.fixedDeltaTime;
            //Quaternion toNorm = Quaternion.Slerp(original, finish.rotation, elapsedTime / t);//Quaternion.Euler(Vector3.Lerp(original.eulerAngles, finish.eulerAngles, elapsedTime / (t)));
            //Quaternion toLook = Quaternion.Slerp(start.rotation, Quaternion.LookRotation(new Vector3(dir.x, dir.y, dir.z)), elapsedTime / (t));
            //chase.getAI().rb.MoveRotation(Quaternion.Euler(Vector3.Lerp(original.eulerAngles, finish.eulerAngles, elapsedTime/(t/2))));

            //lookrotation which faces the player
            //chase.getAI().rb.MoveRotation(Quaternion.Slerp(start.rotation, Quaternion.LookRotation(new Vector3(dir.x,0.0f, dir.z)), elapsedTime / (t)));
            //chase.getAI().rb.MoveRotation(toNorm * toLook);

            chase.getAI().rb.MoveRotation(Quaternion.Slerp(start.rotation, combinedRots, elapsedTime / (t/2)));
            yield return new WaitForSeconds(Time.fixedDeltaTime);
        }
        yield return null;
    }

    public IEnumerator hitGroundApplyRotation(Quaternion rot)
    {
        StopCoroutine("LerpRotation");
        yield return new WaitForEndOfFrame();
        leaper.rb.MoveRotation(rot);
        yield return null;
    }

}
