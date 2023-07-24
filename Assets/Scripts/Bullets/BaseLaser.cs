using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLaser : MonoBehaviour
{
    public float laserDamage;

    public float maxLaserRange;

    public float timeUntilMaxRange;

    protected LineRenderer line;

    public LayerMask laserLayerMask;

    public float elapsedTime;

    public bool isHittingObject = false;

    public float currentMaxRange;

    public Vector3 laserTravelDir;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        line = gameObject.GetComponentInChildren<LineRenderer>();
        laserTravelDir = new Vector3(0, 0, 1.0f);
    }

    protected virtual void Awake()
    {
        line = gameObject.GetComponentInChildren<LineRenderer>();
        line.SetPosition(1, line.GetPosition(0));
        currentMaxRange = maxLaserRange;
        elapsedTime = 0.0f;
        line.SetPosition(1, new Vector3(0.0f, 0.0f, 0.0f));
    }

    protected virtual void OnDisable()
    {
        elapsedTime = 0.0f;
        line.SetPosition(1, new Vector3(0.0f, 0.0f, 0.0f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
