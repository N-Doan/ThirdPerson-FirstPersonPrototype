using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Enemy Combat manager works in sequence with AI. Handles health, knockback, and UI changes
 */
public class BaseEnemyCombatManager : MonoBehaviour
{
    private Rigidbody rb;
    private bool laserHitCooldown;
    private FSMBaseAI fsmAi;
    //linearly increasing knockback each time the enemy is hit
    private float incKnockback = 1.0f;
    //hit cooldown for physical attacks
    private bool hitCooldown = true;
    //Launch power for when hit by player attacks
    [SerializeField]
    private float launchPower;
    // Start is called before the first frame update
    private EnemyDebugUI debugUI;


    void Start()
    {
        rb = gameObject.GetComponentInChildren<Rigidbody>();
        fsmAi = gameObject.GetComponent<FSMBaseAI>();
        debugUI = gameObject.GetComponent<EnemyDebugUI>();
    }

    //Deactivate enemy if it fell out of the world
    protected virtual void Update()
    {
        if(rb.transform.position.y <= -50.0f)
        {
            GlobalVariableStorage.instance.enemyPool.deactivateEnemy(gameObject);
        }
    }

    //Reset UI Disp when re-enabled
    private void OnEnable()
    {
        if (debugUI != null)
        {
            debugUI.updateStatus(incKnockback);
        }
    }


    public void EnemyHit(BaseBullet bullet, Vector3 collisionNormal)
    {
        fsmAi.currentHealth -= bullet.getBulletDamage();

        if(fsmAi.currentHealth <= 0)
        {
            GlobalVariableStorage.instance.enemyPool.deactivateEnemy(gameObject);
        }

        //Set enemy to chasing after being hit
        switch (fsmAi.CurrentStateID)
        {
            case StateIDs.Attacking:
                break;
            case StateIDs.Chasing:
                break;
            case StateIDs.Patrolling:
                fsmAi.PerformTransition(Transitions.PlayerSpotted);
                break;
            case StateIDs.Searching:
                fsmAi.PerformTransition(Transitions.PlayerSpotted);
                break;
        }

        launchEnemy(collisionNormal * bullet.getBulletKnockback());
        if(debugUI != null)
        {
            debugUI.updateStatus(incKnockback);
        }
    }
    public void EnemyHit(BaseLaser laser, Vector3 collisionNormal)
    {
        if (!laserHitCooldown)
        {
            fsmAi.currentHealth -= laser.laserDamage;

            //Set enemy to chasing after being hit
            switch (fsmAi.CurrentStateID)
            {
                case StateIDs.Attacking:
                    break;
                case StateIDs.Chasing:
                    break;
                case StateIDs.Patrolling:
                    fsmAi.PerformTransition(Transitions.PlayerSpotted);
                    break;
                case StateIDs.Searching:
                    fsmAi.PerformTransition(Transitions.PlayerSpotted);
                    break;
            }

            if (fsmAi.currentHealth <= 0)
            {
                Destroy(gameObject);
            }
            StartCoroutine(initLaserHitCooldown());
            launchEnemy(collisionNormal);
        }
    }

    public void applyForceToPlayer(Collision collisionNormal)
    {
        if (collisionNormal.body.CompareTag("Player") && hitCooldown)
        {
            collisionNormal.body.transform.parent.gameObject.GetComponent<PlayerCombatManager>()
                .playerHit(gameObject.GetComponentInParent<LeaperAI>().getAttackDamage(), collisionNormal.GetContact(0).normal);
            hitCooldown = false;
            StartCoroutine(CollisionCooldown());
        }
    }
    private IEnumerator CollisionCooldown()
    {
        yield return new WaitForSeconds(0.15f);
        hitCooldown = true;
        yield return null;
    }

    private void launchEnemy(Vector3 collisionNormal)
    {
        //controller.StartCoroutine("useLaunchedMaxVelocity");
        if (collisionNormal.y > 0)
        {
            collisionNormal.y = Random.Range(-0.2f, -0.01f);
        }
        collisionNormal = new Vector3(collisionNormal.x, collisionNormal.y, collisionNormal.z);
        rb.velocity = new Vector3(0, 0, 0);
        rb.AddForce(-collisionNormal * launchPower * incKnockback, ForceMode.VelocityChange);
        if(incKnockback < 10.0f)
        {
            incKnockback++;
        }

    }

    public void resetEnemy()
    {
        incKnockback = 1.0f;
        fsmAi.resetFSMEnemy();
    }

    private IEnumerator initLaserHitCooldown()
    {
        laserHitCooldown = true;
        yield return new WaitForSeconds(0.2f);
        laserHitCooldown = false;
    }
}
