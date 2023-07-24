using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    public static EventManager instance;

    private void Awake()
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

    //EVENTS
    public event Action<int> EOnFirstPersonEnabled;
    public event Action<int> EOnFirstPersonDisabled;
    public event Action<int, Collision> EOnBulletCollision;
    public event Action<int, float> EOnPlayerDamageTaken;

    //METHODS
    public void OnFirstPersonEnabled(int instanceID)
    {
        EOnFirstPersonEnabled?.Invoke(instanceID);
    }
    public void OnFirstPersonDisabled(int instanceID)
    {
        EOnFirstPersonDisabled(instanceID);
    }
    public void OnBulletCollision(int instanceID, Collision collision)
    {
        EOnBulletCollision(instanceID, collision);
    }
    public void OnPlayerDamageTaken(int instanceID, float damageVal)
    {
        EOnPlayerDamageTaken(instanceID, damageVal);
    }

}
