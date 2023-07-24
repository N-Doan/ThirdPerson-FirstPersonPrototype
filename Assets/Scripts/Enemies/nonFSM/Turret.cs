using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField]
    private float shotDelay;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private GameObject fireLocation;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(turretShoot());
    }

    private IEnumerator turretShoot()
    {
        for( ; ; )
        {
            GameObject bullet = Instantiate(bulletPrefab, fireLocation.transform.position, Quaternion.identity);
            bullet.GetComponent<BaseBullet>().setTravelDirection(gameObject.transform.forward);

            yield return new WaitForSeconds(shotDelay);
        }

    }
}
