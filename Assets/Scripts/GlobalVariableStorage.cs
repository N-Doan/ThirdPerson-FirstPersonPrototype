using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariableStorage : MonoBehaviour
{
    public static GlobalVariableStorage instance;

    public Transform playerLocation;
    public CheckpointManager checkpointManager;
    public BulletObjectPool bulletPool;
    public EnemyObjectPool enemyPool;

    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(obj: this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
