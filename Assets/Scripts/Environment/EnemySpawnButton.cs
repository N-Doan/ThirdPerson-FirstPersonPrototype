using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnButton : BaseInteractable
{
    EnemySpawner spawner;
    private void Start()
    {
        spawner = gameObject.GetComponentInParent<EnemySpawner>();
    }
    public override void onRaycastHit()
    {
        spawner.spawnEnemy();
    }
}
