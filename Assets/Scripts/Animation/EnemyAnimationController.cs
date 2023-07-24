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
    private GameObject[] legTargets;

    private bool IKState = true;

    private void Start()
    {
        if(enemyRigBuilder == null)
        {
            enemyRigBuilder = gameObject.GetComponent<RigBuilder>();
        }
        enemyRig = enemyRigBuilder.layers[0].rig;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (IKState)
            {
                disableIK();
            }
            else
            {
                enableIK();
            }
        }
    }

    public void enableIK()
    {
        enemyRig.weight = 1.0f;
        IKState = true;
    }
    public void disableIK()
    {
        enemyRig.weight = 0.0f;
        IKState = false;
    }
}
