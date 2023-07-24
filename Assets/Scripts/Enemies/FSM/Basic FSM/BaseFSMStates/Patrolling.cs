using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrolling : FSMState
{
    private PatrolAIProperties patrolAIProperties;
    private FSMBaseAI ai;

    public Patrolling(FSMBaseAI controller, PatrolAIProperties properties)
    {
        stateID = StateIDs.Patrolling;
        ai = controller;
        patrolAIProperties = properties;
    }

    public override void Act(Transform player, Transform npc)
    {
        //Move towards the current destination
        ai.rb.AddForce((patrolAIProperties.travelTarget.position - ai.rb.position).normalized * ai.baseMoveSpeed, ForceMode.Force);

        Debug.Log("PATROLLING");
        if ((patrolAIProperties.travelTarget.position - ai.rb.position).magnitude < 1.0f)
        {
            if (patrolAIProperties.patrolPoints.Count > patrolAIProperties.patrolPoints.IndexOf(patrolAIProperties.travelTarget) + 1)
            {
                patrolAIProperties.travelTarget = patrolAIProperties.patrolPoints[patrolAIProperties.patrolPoints.IndexOf(patrolAIProperties.travelTarget) + 1];
            }
            else
            {
                patrolAIProperties.travelTarget = patrolAIProperties.patrolPoints[0];
            }
        }
    }

    public override void Reason(Transform player, Transform npc)
    {
        RaycastHit hit;

        if (Physics.Raycast(ai.rb.position, GlobalVariableStorage.instance.playerLocation.position - ai.rb.position, out hit, 15))
        {
            Debug.DrawRay(ai.rb.position, hit.transform.position - ai.rb.position, Color.red);
            if (hit.collider.CompareTag("Player"))
            {
                ai.PerformTransition(Transitions.PlayerSpotted);
            }
        }
    }
}
