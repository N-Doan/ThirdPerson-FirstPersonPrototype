using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LDead : FSMState
{
    private LDeathAIProperties deathAIProperties;
    private FSMBaseAI ai;

    public LDead(FSMBaseAI controller, LDeathAIProperties properties)
    {
        stateID = StateIDs.Dead;
        ai = controller;
        deathAIProperties = properties;
    }

    public override void Act(Transform player, Transform npc)
    {

    }

    public override void Reason(Transform player, Transform npc)
    {

    }

}
