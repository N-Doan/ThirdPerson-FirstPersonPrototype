using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dead : FSMState
{
    private DeathAIProperties deathAIProperties;
    private FSMBaseAI ai;

    public Dead(FSMBaseAI controller, DeathAIProperties properties)
    {
        stateID = StateIDs.Dead;
        ai = controller;
        deathAIProperties = properties;
    }

    public override void Act(Transform player, Transform npc)
    {
        throw new System.NotImplementedException();
    }

    public override void Reason(Transform player, Transform npc)
    {
        throw new System.NotImplementedException();
    }

}
