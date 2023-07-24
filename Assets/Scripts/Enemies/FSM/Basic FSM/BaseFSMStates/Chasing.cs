using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chasing : FSMState
{
    private ChaseAIProperties chaseAIProperties;
    private FSMBaseAI ai;

    public Chasing(FSMBaseAI controller, ChaseAIProperties properties)
    {
        stateID = StateIDs.Chasing;
        ai = controller;
        chaseAIProperties = properties;
    }

    public override void Act(Transform player, Transform npc)
    {
        Debug.Log("Chasing");
        //Move towards the current destination
        ai.rb.AddForce((GlobalVariableStorage.instance.playerLocation.position - ai.rb.position).normalized * ai.chaseMoveSpeed, ForceMode.Force);
    }

    public override void Reason(Transform player, Transform npc)
    {
        RaycastHit hit;
        if (Physics.Raycast(ai.rb.position, GlobalVariableStorage.instance.playerLocation.position - ai.rb.position, out hit, 15))
        {
            Debug.DrawRay(ai.rb.position, hit.transform.position - ai.rb.position, Color.red);
            if (!hit.collider.CompareTag("Player"))
            {
                ai.PerformTransition(Transitions.PlayerLost);
            }
        }
    }

}
