using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrapnelPiece : BaseBullet
{
    int instanceID;
    private Vector3 rotation;

    protected override void Start()
    {
        base.Start();
        rb = GetComponentInChildren<Rigidbody>();
        EventManager.instance.EOnBulletCollision += onTerrainCollision;
        instanceID = transform.gameObject.GetInstanceID();
        rotation = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
    }

    private void Update()
    {
        //rb.AddForce(getTravelDirection() * getBulletSpeed() * Time.deltaTime);
        rb.position = rb.position + getTravelDirection() * getBulletSpeed() * Time.fixedDeltaTime;
        rb.transform.Rotate(rotation);

    }

    public override void applyBulletEffects()
    {
        //No special effect
    }

    private void onTerrainCollision(int id, Collision collision)
    {
        if (id == instanceID)
        {
            if (collision.gameObject.CompareTag("Terrain"))
            {
                Vector3 newDir = collision.contacts[0].normal - rb.transform.position;
                newDir = newDir.normalized;
                newDir = addVariation(newDir);
                setTravelDirection(newDir);
            }
            else if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponentInParent<PlayerCombatManager>().playerHit(gameObject.GetComponent<BaseBullet>(), collision.contacts[0].normal);
                Destroy(gameObject);
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
        Destroy(gameObject);
    }
}
