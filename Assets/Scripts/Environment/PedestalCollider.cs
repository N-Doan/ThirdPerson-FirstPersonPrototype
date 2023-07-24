using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestalCollider : MonoBehaviour
{
    WeaponPedestal parent;

    private void Start()
    {
        parent = gameObject.GetComponentInParent<WeaponPedestal>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parent.setPlayerWeaponPref(other.gameObject.GetComponentInParent<PlayerCombatManager>());
        }
    }
}
