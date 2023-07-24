using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Use this for data essential to all states
public abstract class AIProperties
{

}

public class PatrolAIProperties : AIProperties
{
    [SerializeField]
    public List<Transform> patrolPoints;
    public Transform travelTarget;
}

public class ChaseAIProperties : AIProperties
{

}

public class DeathAIProperties : AIProperties
{

}

public class SearchAIProperties : AIProperties
{
    public float searchTime = 2.5f;
}


public class FSMBaseAI : FSMExtended
{
    private SearchAIProperties searchAIProperties;
    private ChaseAIProperties chaseAIProperties;
    private DeathAIProperties deathAIProperties;
    private PatrolAIProperties patrolAIProperties;

    [Header("References")]
    public Rigidbody rb;

    [Header("Movement")]
    public float baseMoveSpeed = 30.0f;
    public float chaseMoveSpeed = 35.0f;

    [Header("Combat")]
    [SerializeField]
    private float maxHealth;
    public float getMaxHealth()
    {
        return maxHealth;
    }
    [HideInInspector]
    public float currentHealth;

    [SerializeField]
    private float attackDamage;
    public float getAttackDamage()
    {
        return attackDamage;
    }

    protected override void Initialize()
    {
        //playerTransform = GlobalVariableStorage.instance.playerLocation;
        base.Initialize();
        ConstructFSM();
        currentHealth = maxHealth;
    }

    protected override void FSMFixedUpdate()
    {
        if(CurrentState != null)
        {
            CurrentState.Reason(playerTransform, transform);
            CurrentState.Act(playerTransform, transform);
        }
    }


    public virtual void ConstructFSM()
    {
        Patrolling patrolState = new Patrolling(this, patrolAIProperties);
        AddFSMState(patrolState);
        patrolState.AddTransition(Transitions.PlayerSpotted, StateIDs.Chasing);
        patrolState.AddTransition(Transitions.NoHealth, StateIDs.Dead);

        Chasing chaseState = new Chasing(this, chaseAIProperties);
        AddFSMState(chaseState);
        chaseState.AddTransition(Transitions.NoHealth, StateIDs.Dead);
        chaseState.AddTransition(Transitions.PlayerLost, StateIDs.Searching);

        Searching searchState = new Searching(this, searchAIProperties);
        AddFSMState(searchState);
        searchState.AddTransition(Transitions.NoHealth, StateIDs.Dead);
        searchState.AddTransition(Transitions.PlayerSpotted, StateIDs.Chasing);
        searchState.AddTransition(Transitions.PlayerLost, StateIDs.Patrolling);

        Dead dead = new Dead(this, deathAIProperties);
        dead.AddTransition(Transitions.Revived, StateIDs.Patrolling);
        AddFSMState(dead);
    }

    public virtual void resetFSMEnemy()
    {
        switch (CurrentState.ID)
        {
            case StateIDs.Patrolling:
                break;
            case StateIDs.Attacking:
                PerformTransition(Transitions.NoHealth);
                break;
            case StateIDs.Chasing:
                PerformTransition(Transitions.NoHealth);
                break;
            case StateIDs.Dead:
                PerformTransition(Transitions.Revived);
                break;
        }
        if (CurrentState.ID == StateIDs.Dead)
        {
            PerformTransition(Transitions.Revived);
        }
        currentHealth = maxHealth;
        gameObject.transform.position = Vector3.zero;
        rb.transform.localPosition = Vector3.zero;
        rb.transform.rotation = Quaternion.Euler(Vector3.zero);
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }
}
