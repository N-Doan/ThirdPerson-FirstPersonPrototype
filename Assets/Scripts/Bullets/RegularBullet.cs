using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegularBullet : BaseBullet
{
    int instanceID;

    protected override void Start()
    {
        base.Start();
        rb = GetComponentInChildren<Rigidbody>();
        EventManager.instance.EOnBulletCollision += onTerrainCollision;
        instanceID = transform.gameObject.GetInstanceID();

    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void Update()
    {
        //rb.AddForce(getTravelDirection() * getBulletSpeed() * Time.deltaTime);
        rb.position = rb.position + getTravelDirection() * getBulletSpeed() * Time.deltaTime;
    
    }

    public override void applyBulletEffects()
    {
        //No special effect
    }

    private void onTerrainCollision(int id, Collision collision)
    {
        if(id == instanceID)
        {
            if (collision.gameObject.CompareTag("Terrain"))
            {
                Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal);
                Vector3 newDir = collision.GetContact(0).normal;
                newDir = newDir.normalized;
                newDir = addVariation(newDir);
                //newDir = addVariation(newDir);
                //setTravelDirection(newDir);
                setTravelDirection(Vector3.Reflect(getTravelDirection(), collision.GetContact(0).normal));
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponentInParent<PlayerCombatManager>().playerHit(gameObject.GetComponent<BaseBullet>(), collision.contacts[0].normal);
                GlobalVariableStorage.instance.bulletPool.deactivateBullet(gameObject);
            }
            else if (collision.gameObject.CompareTag("Enemy"))
            {
                collision.gameObject.GetComponentInParent<BaseEnemyCombatManager>().EnemyHit(gameObject.GetComponent<BaseBullet>(), collision.contacts[0].normal);
                GlobalVariableStorage.instance.bulletPool.deactivateBullet(gameObject);
            }
        }
    }

    private Vector3 addVariation(Vector3 dir)
    {
        dir = new Vector3(dir.x + Random.Range(-0.05f, 0.05f), dir.y + Random.Range(-0.05f, 0.05f), dir.z + Random.Range(-0.05f, 0.05f));
        dir = dir.normalized;
        return dir;
    }

    public override IEnumerator lifeTimeCountdown()
    {
        yield return new WaitForSeconds(getLifeTime());
        GlobalVariableStorage.instance.bulletPool.deactivateBullet(gameObject);
    }
}
