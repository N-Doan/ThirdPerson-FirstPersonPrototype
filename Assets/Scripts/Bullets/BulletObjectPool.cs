using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletObjectPool : MonoBehaviour
{
    [SerializeField]
    private int bulletPoolSize = 2500;
    [SerializeField]
    private GameObject bulletPref;

    private Queue<GameObject> inactiveBullets;
    private Queue<GameObject> activeBullets;

    // Start is called before the first frame update
    void Start()
    {
        inactiveBullets = new Queue<GameObject>();

        for(int i = 0; i < bulletPoolSize; i++)
        {
            GameObject bullet = GameObject.Instantiate(bulletPref);
            bullet.SetActive(false);
            inactiveBullets.Enqueue(bullet);
        }
    }

    public GameObject activateBullet()
    {
        GameObject output = inactiveBullets.Dequeue();
        output.SetActive(true);
        return output;
    }

    public void deactivateBullet(GameObject bullet)
    {
        if (bullet.GetComponent<RegularBullet>())
        {
            inactiveBullets.Enqueue(bullet);
            RegularBullet script = bullet.GetComponent<RegularBullet>();
            script.setTravelDirection(new Vector3(0, 0, 0));
            script.rb.gameObject.transform.localPosition = new Vector3(0, 0, 0);
            bullet.SetActive(false);
        }
        else
        {
            Debug.Log("Not a Bullet!");
        }
    }
}
