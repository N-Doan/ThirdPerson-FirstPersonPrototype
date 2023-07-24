using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBullet : MonoBehaviour
{
    [SerializeField]
    private float bulletDamage;

    public float getBulletDamage()
    {
        return bulletDamage;
    }

    [SerializeField]
    private float bulletKnockback;

    public float getBulletKnockback()
    {
        return bulletKnockback;
    }

    [SerializeField]
    private float bulletSpeed;

    public float getBulletSpeed()
    {
        return bulletSpeed;
    }

    [SerializeField]
    private float lifeTime;

    public float getLifeTime()
    {
        return lifeTime;
    }

    [HideInInspector]
    public Rigidbody rb;

    [SerializeField]
    private float shotCooldown = 0;

    public float getShotCooldown()
    {
        return shotCooldown;
    }

    private Vector3 travelDirection;

    public Vector3 getTravelDirection()
    {
        return travelDirection;
    }

    public void setTravelDirection(Vector3 newDir)
    {
        travelDirection = newDir;
    }

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(lifeTimeCountdown());
    }


    public abstract void applyBulletEffects();

    public abstract IEnumerator lifeTimeCountdown();
}
