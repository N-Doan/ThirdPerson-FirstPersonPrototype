using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Object pool for enemy spawning and resetting
 */
public class EnemyObjectPool : MonoBehaviour
{
    [SerializeField]
    private int enemyPoolSize = 50;
    [SerializeField]
    private GameObject enemyPref;

    private Stack<GameObject> inactiveEnemies;

    private List<GameObject> activeEnemies;

    // Start is called before the first frame update
    void Start()
    {
        inactiveEnemies = new Stack<GameObject>();
        activeEnemies = new List<GameObject>();

        for(int i = 0; i < enemyPoolSize; i++)
        {
            GameObject enemy = GameObject.Instantiate(enemyPref);
            enemy.SetActive(false);
            inactiveEnemies.Push(enemy);
        }
    }

    public GameObject activateEnemy(Vector3 point)
    {
        GameObject output = inactiveEnemies.Pop();
        output.transform.position = point;
        output.SetActive(true);
        activeEnemies.Add(output);
        return output;
    }

    public void deactivateEnemy(GameObject enemy)
    {
        if (enemy.GetComponent<FSMBase>())
        {
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                inactiveEnemies.Push(enemy);
                BaseEnemyCombatManager script = enemy.GetComponent<BaseEnemyCombatManager>();
                //Reset Enemy Components
                script.resetEnemy();
            }
        }
        else
        {
            Debug.Log("Not an Enemy!");
        }
    }

    public void deactivateAllEnemies()
    {
        float f = activeEnemies.Count;
        for(int i = 0; i < f; i++)
        {
            deactivateEnemy(activeEnemies[0]);
        }
    }
}
