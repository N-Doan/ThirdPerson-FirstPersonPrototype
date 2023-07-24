using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnTouchApplyDamage : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameObject.GetComponentInParent<BaseEnemyCombatManager>().applyForceToPlayer(collision);
        }
    }
}
