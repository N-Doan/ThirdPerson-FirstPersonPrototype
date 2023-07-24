using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : BaseBullet
{
    private int instanceID;
    [SerializeField]
    private float launchForce;

    [SerializeField]
    private GameObject explosion;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        rb = GetComponentInChildren<Rigidbody>();
        EventManager.instance.EOnBulletCollision += onTerrainCollision;
        instanceID = transform.gameObject.GetInstanceID();
        rb.AddForce(getTravelDirection() * launchForce, ForceMode.Impulse);
    }

    public override void applyBulletEffects()
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void onTerrainCollision(int id, Collision collision)
    {
        //Start detonation timer
        if(id == instanceID)
        {
            StartCoroutine(DetonationTimer());
        }
        
    }

    public override IEnumerator lifeTimeCountdown()
    {
        yield return new WaitForSeconds(getLifeTime());
        //INSTANTIATE AN EXPLOSION HERE
        Destroy(gameObject);
    }

    private IEnumerator DetonationTimer()
    {
        StopCoroutine(lifeTimeCountdown());
        yield return new WaitForSeconds(2);
        //Instantiate Explosion
        GameObject.Instantiate(explosion, rb.transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
