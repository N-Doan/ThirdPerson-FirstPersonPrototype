using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletCollisionHandler : MonoBehaviour
{
    BaseBullet parent;
    int instanceID;

    private void Start()
    {
        parent = gameObject.GetComponentInParent<BaseBullet>();
        instanceID = parent.transform.gameObject.GetInstanceID();
    }

    private void OnCollisionEnter(Collision collision)
    {
            EventManager.instance.OnBulletCollision(instanceID, collision);
    }
}
