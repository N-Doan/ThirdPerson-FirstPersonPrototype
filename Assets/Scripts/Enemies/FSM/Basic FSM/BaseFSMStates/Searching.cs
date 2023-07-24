using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Searching : FSMState
{
    private SearchAIProperties searchAIProperties;
    private FSMBaseAI ai;

    private float searchTime;
    public Searching(FSMBaseAI controller, SearchAIProperties properties)
    {
        stateID = StateIDs.Searching;
        ai = controller;
        searchAIProperties = properties;
    }

    public override void EnterStateInit(Transform player, Transform npc)
    {
        searchTime = searchAIProperties.searchTime;
    }

    public override void Act(Transform player, Transform npc)
    {
        Debug.Log("Searching");
        searchTime -= Time.fixedDeltaTime;
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

        if(searchTime <= 0)
        {
            ai.PerformTransition(Transitions.PlayerLost);
        }
    }

}
