using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class LChaseAIProperties : ChaseAIProperties
{
    [Header("Ranges")]
    public float chaseRange;
    public float jumpRange;
    public float attackRange;

    [Header("Intervals and Variances")]
    public float jumpIntervals;
    public float jumpIntervalsVariance;
    public float landingOffset = 1.0f;

    [Header("Terrain LayerMask")]
    public LayerMask terrainLayerMask;

    [Header("Misc")]
    public float activationForce = 20.0f;
    [HideInInspector]
    public bool isOnWall = false;
    [HideInInspector]
    public bool activated = false;
}
[System.Serializable]
public class LDeathAIProperties : DeathAIProperties
{

}

[System.Serializable]
public class LPatrolAIProperties : PatrolAIProperties
{

}


public class LeaperAI : FSMBaseAI
{

    public LDeathAIProperties lDeathAIProperties;

    public LPatrolAIProperties lPatrolAIProperties;

    public LChaseAIProperties lChaseAIProperties;

    [HideInInspector]
    public LeaperCoroutines leaperCoroutines;
    [HideInInspector]
    public NavMeshAgent agent;
    [HideInInspector]
    public LeaperAnimController animController;

    //Initialize AI and find coroutine and agent variables
    protected override void Initialize()
    {
        leaperCoroutines = gameObject.GetComponent<LeaperCoroutines>();
        agent = rb.gameObject.GetComponentInChildren<NavMeshAgent>();
        agent.speed = chaseMoveSpeed;
        animController = GetComponent<LeaperAnimController>();

        agent.enabled = false;
        
        base.Initialize();

        if (lPatrolAIProperties.patrolPoints.Count != 0)
        {
            lPatrolAIProperties.travelTarget = lPatrolAIProperties.patrolPoints[0];
        }
    }

    protected override void FSMFixedUpdate()
    {
        base.FSMFixedUpdate();
    }


    public override void ConstructFSM()
    {
        LPatrolling patrolState = new LPatrolling(this, lPatrolAIProperties);
        AddFSMState(patrolState);
        patrolState.AddTransition(Transitions.PlayerSpotted, StateIDs.Chasing);
        patrolState.AddTransition(Transitions.NoHealth, StateIDs.Dead);

        LChasing chaseState = new LChasing(this, lChaseAIProperties);
        AddFSMState(chaseState);
        chaseState.AddTransition(Transitions.NoHealth, StateIDs.Dead);
        chaseState.AddTransition(Transitions.PlayerLost, StateIDs.Searching);


        LDead dead = new LDead(this, lDeathAIProperties);
        dead.AddTransition(Transitions.Revived, StateIDs.Patrolling);
        AddFSMState(dead);
    }

    public override void resetFSMEnemy()
    {
        base.resetFSMEnemy();
    }
}
