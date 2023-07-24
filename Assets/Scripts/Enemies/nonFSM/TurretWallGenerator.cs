using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretWallGenerator : MonoBehaviour
{
    enum fireMode
    {
        Instant, columns, rows
    }

    [SerializeField]
    private int wallHeight = 5;
    [SerializeField]
    private int wallWidth = 5;
    [SerializeField]
    private float spaceBetweenSpawners = 1.5f;
    [SerializeField]
    private GameObject bulletPrefab;
    [SerializeField]
    private float shotDelay = 2.5f;
    [SerializeField]
    private float bulletTravelSpeed;
    [SerializeField]
    private fireMode bulletFireMode;

    private Transform[][] fireLocations;

    // Start is called before the first frame update
    void Start()
    {
        fireLocations = new Transform[wallHeight][];
        for(int i = 0; i < wallHeight; i++)
        {
            fireLocations[i] = new Transform[wallWidth];
        }

        for(int i = 0; i < wallHeight; i++)
        {
            for(int j = 0; j < wallWidth; j++)
            {
                GameObject fireLoc = new GameObject();
                fireLoc.transform.position = gameObject.transform.position;
                fireLoc.transform.parent = gameObject.transform;

                fireLoc.transform.position += new Vector3(j * spaceBetweenSpawners, i * spaceBetweenSpawners, 0);
                fireLocations[i][j] = fireLoc.transform;
            }
        }

        StartCoroutine(fire());
    }

    private IEnumerator fire()
    {
        for( ; ; )
        {
            yield return new WaitForSeconds(shotDelay);
            switch (bulletFireMode)
            {
                case fireMode.Instant:
                    for (int i = 0; i < wallHeight; i++)
                    {
                        for (int j = 0; j < wallWidth; j++)
                        {
                            GameObject bulletGO = GlobalVariableStorage.instance.bulletPool.activateBullet();
                            bulletGO.transform.position = fireLocations[i][j].position;
                            bulletGO.GetComponent<BaseBullet>().setTravelDirection(new Vector3(0, 0, -1.0f));
                        }
                    }
                    break;
                case fireMode.columns:
                    //forwards
                    for (int i = 0; i < wallWidth; i++)
                    {
                        for (int j = 0; j < wallHeight; j++)
                        {
                            GameObject bulletGO = GlobalVariableStorage.instance.bulletPool.activateBullet();
                            bulletGO.transform.position = fireLocations[j][i].position;
                            bulletGO.GetComponent<BaseBullet>().setTravelDirection(new Vector3(0, 0, -1.0f));
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                    //backwards
                    for (int i = wallWidth-1; i >= 0; i--)
                    {
                        for (int j = wallHeight-1; j >= 0; j--)
                        {
                            GameObject bulletGO = GlobalVariableStorage.instance.bulletPool.activateBullet();
                            bulletGO.transform.position = fireLocations[j][i].position;
                            bulletGO.GetComponent<BaseBullet>().setTravelDirection(new Vector3(0, 0, -1.0f));
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                    break;

                case fireMode.rows:
                    for (int i = 0; i < wallHeight; i++)
                    {
                        for (int j = 0; j < wallWidth; j++)
                        {
                            GameObject bulletGO = GlobalVariableStorage.instance.bulletPool.activateBullet();
                            bulletGO.transform.position = fireLocations[i][j].position;
                            bulletGO.GetComponent<BaseBullet>().setTravelDirection(new Vector3(0, 0, -1.0f));
                        }
                        yield return new WaitForSeconds(0.1f);
                    }
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
