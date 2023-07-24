using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserBeamController : BaseLaser
{
    public void setLaserTraverDir(Vector3 newDir)
    {
        laserTravelDir = newDir;
    }
    public Vector3 getLaserTravelDir()
    {
        return laserTravelDir;
    }
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    void Update()
    {
        if(line.GetPosition(1).magnitude < currentMaxRange)
        {
            if (!isHittingObject)
            {
                elapsedTime += Time.deltaTime;
            }
            line.SetPosition(1, Vector3.Lerp(line.GetPosition(0), laserTravelDir * currentMaxRange, elapsedTime / timeUntilMaxRange));
        }
        performRaycast();
        //Debug.Log("POINT 0: " + line.GetPosition(0));
        //Debug.Log("POINT 1: " + line.GetPosition(1));
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void performRaycast()
    {
        RaycastHit hit;
        Debug.DrawRay(gameObject.transform.position, transform.forward, Color.green);
        if (Physics.Raycast(gameObject.transform.position, transform.forward, out hit, line.GetPosition(1).magnitude, laserLayerMask))
        {

            if(hit.distance != currentMaxRange)
            {
                currentMaxRange = hit.distance;
                line.SetPosition(1, Vector3.Lerp(line.GetPosition(0), laserTravelDir * currentMaxRange, 1));
            }
            isHittingObject = true;

            if (hit.collider.CompareTag("Enemy"))
            {
                Debug.Log("HIT");
                hit.collider.gameObject.GetComponentInParent<BaseEnemyCombatManager>().EnemyHit(this, -gameObject.transform.forward);
            }
        }
        else if(isHittingObject)
        {
            //no longer hitting object, so reset lazer end position to match previous collision point
            isHittingObject = false;
            recalculateElapsedTime();
        }
    }
    private void recalculateElapsedTime()
    {
        elapsedTime = (currentMaxRange / maxLaserRange) * timeUntilMaxRange;
        currentMaxRange = maxLaserRange;
    }

}
