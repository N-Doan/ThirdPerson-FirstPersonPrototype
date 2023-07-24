using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private EnemyObjectPool[] enemyObjectPools;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            resetLevel();
        }
    }

    public void resetLevel()
    {
        resetEnemies();
        GlobalVariableStorage.instance.playerLocation.GetComponentInParent<PlayerController>().resetPlayer();
    }

    public void resetEnemies()
    {
        foreach(EnemyObjectPool pool in enemyObjectPools)
        {
            pool.deactivateAllEnemies();
        }
    }
}
