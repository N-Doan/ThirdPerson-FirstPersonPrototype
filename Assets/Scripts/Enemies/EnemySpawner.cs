using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    Collider col;

    private void Start()
    {
        col = gameObject.GetComponent<Collider>();
    }
    public void spawnEnemy()
    {
        //get random point inside collider
        Vector3 randomPoint = new Vector3(Random.Range(-col.bounds.extents.x, col.bounds.extents.x), col.bounds.center.y, 
        Random.Range(-col.bounds.extents.z, col.bounds.extents.z));

        randomPoint = col.bounds.center + transform.InverseTransformDirection(randomPoint);
        GameObject enemySpawned = GlobalVariableStorage.instance.enemyPool.activateEnemy(randomPoint);
    }
}
