using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPedestal : MonoBehaviour
{
    [SerializeField]
    private GameObject weaponPref;

    public void setPlayerWeaponPref(PlayerCombatManager player)
    {
        player.setActiveBullet(weaponPref);
    }
}
