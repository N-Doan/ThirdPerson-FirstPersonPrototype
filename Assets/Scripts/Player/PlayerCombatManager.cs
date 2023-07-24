using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombatManager : MonoBehaviour
{
    [SerializeField]
    private float playerDamaged = 0.0f;

    [SerializeField]
    private GameObject activeBullet;
    public void setActiveBullet(GameObject newBullet)
    {
        activeBullet = newBullet;
    }

    [SerializeField]
    private Transform fireLocation;
    [SerializeField]
    private GameObject laserGO;
    [SerializeField]
    private Transform cameraT;

    private PlayerController controller;
    private bool canFire = true;

    private bool firingLaser = false;

    private LaserBeamController laserController;

    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponentInChildren<Rigidbody>();
        controller = gameObject.GetComponent<PlayerController>();
        laserController = laserGO.GetComponent<LaserBeamController>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void playerHit(BaseBullet bullet, Vector3 collisionNormal)
    {
        playerDamaged += bullet.getBulletDamage();
        playerDamaged = Mathf.Clamp(playerDamaged, 0.0f, 10.0f);
        //Debug.Log(playerDamaged);
        bullet.applyBulletEffects();
        EventManager.instance.OnPlayerDamageTaken(gameObject.transform.GetInstanceID(), playerDamaged);

        launchPlayer(collisionNormal);
    }

    public void playerHit(float damage, Vector3 collisionNormal)
    {
        playerDamaged += damage;
        playerDamaged = Mathf.Clamp(playerDamaged, 0.0f, 10.0f);

        EventManager.instance.OnPlayerDamageTaken(gameObject.transform.GetInstanceID(), playerDamaged);

        launchPlayer(collisionNormal);
    }

    public void spawnBullet(Transform lookDirection)
    {
        if (activeBullet.GetComponent<BaseBullet>())
        {
            if (activeBullet.GetComponent<BaseBullet>().getShotCooldown() > 0 && !activeBullet.GetComponent<RaycastBullet>())
            {
                GameObject bullet = GameObject.Instantiate(activeBullet, fireLocation.position, Quaternion.identity);
                bullet.GetComponent<BaseBullet>().setTravelDirection(lookDirection.forward);
                canFire = false;
                StartCoroutine(fireCooldown(bullet.GetComponent<BaseBullet>().getShotCooldown()));
            }
            else if (activeBullet.GetComponent<RaycastBullet>())
            {
                Ray ray = cameraT.GetComponent<Camera>().ScreenPointToRay(new Vector3(cameraT.GetComponent<Camera>().scaledPixelWidth/2, cameraT.GetComponent<Camera>().scaledPixelHeight / 2, 0.0f));
                activeBullet.GetComponent<RaycastBullet>().fire(ray);
            }
        }
        else if (activeBullet.GetComponent<LaserBeamController>())
        {
            laserGO.SetActive(true);
            firingLaser = true;
            //laserController.setLaserTraverDir(lookDirection.forward);
        }
    }

    public void stopLaser()
    {
        if (activeBullet.GetComponent<LaserBeamController>())
        {
            laserGO.SetActive(false);
            firingLaser = false;
        }
    }

    private IEnumerator fireCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        canFire = true;
        yield return null;
    }

    private void launchPlayer(Vector3 collisionNormal)
    {
        //controller.StartCoroutine("useLaunchedMaxVelocity");
        if(collisionNormal.y > 0)
        {
            collisionNormal.y = Random.Range(-0.2f, -0.01f);
        }
        collisionNormal = new Vector3(collisionNormal.x, Mathf.Clamp(collisionNormal.y, -0.2f, -0.01f), collisionNormal.z);
        rb.AddForce(-collisionNormal * 25.0f * playerDamaged, ForceMode.Impulse);
    }

    public void resetPlayer()
    {
        playerDamaged = 0.0f;
    }
}
