using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField]
    private RigBuilder enemyRigBuilder;
    private Rig enemyRig;

    [SerializeField]
    private LayerMask terrainMask;


    [SerializeField]
    private GameObject[] legTargets;

    private bool IKState = true;
    private Vector3[] origins;

    private void Start()
    {
        origins = new Vector3[legTargets.Length];
        if(enemyRigBuilder == null)
        {
            enemyRigBuilder = gameObject.GetComponent<RigBuilder>();
        }
        enemyRig = enemyRigBuilder.layers[0].rig;
        for(int i = 0; i < legTargets.Length; i++)
        {
            origins[i] = legTargets[i].transform.localPosition;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            calculateArmPositions();
        }
    }

    public void enableIK()
    {
        enemyRig.weight = 1.0f;
        IKState = true;
    }
    public void disableIK()
    {
        int inc = 0;
        //set IK position back to original position
        foreach (GameObject target in legTargets)
        {
            target.transform.localPosition = origins[inc];
            inc++;
        }
        enemyRig.weight = 0.0f;
        IKState = false;
    }

    public void calculateArmPositions()
    {
        //for each IK-Controller
        //Find nearby terrain
        //Move IK-Target to terrain
        int inc = 0;
        foreach(GameObject target in legTargets)
        {
            target.transform.position = getNearestPointOnTerrain(target);
            inc++;
        }
    }

    //Use same process used in LChasing to find where the foot should go
    private Vector3 getNearestPointOnTerrain(GameObject target)
    {
        Vector3 output = Vector3.zero;
        //Make list of nearest points
        Collider[] hits = Physics.OverlapSphere(target.transform.position, 5.5f, terrainMask);
        float[] dists = new float[hits.Length];
        if (hits.Length > 0)
        {
            float closest = float.MaxValue;
            for(int i = 0; i < dists.Length; i++)
            {
                Vector3 closestPoint;
                closestPoint = hits[i].ClosestPoint(target.transform.position);
                float dist = Vector3.Distance(closestPoint, target.transform.position);
                dists[i] = dist;
                if(closest >= dist)
                {
                    closest = dist;
                    output = closestPoint;
                    Debug.Log(dist);
                }
            }
        }
        else
        {
            Debug.Log("Failed to find nearest point");
        }
        return output;
    }
}
