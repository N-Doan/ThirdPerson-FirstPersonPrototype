using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class is adapted and modified from the FSM implementation class available on UnifyCommunity website
/// The license for the code is Creative Commons Attribution Share Alike.
/// It's originally the port of C++ FSM implementation mentioned in Chapter01 of Game Programming Gems 1
/// You're free to use, modify and distribute the code in any projects including commercial ones.
/// Please read the link to know more about CCA license @http://creativecommons.org/licenses/by-sa/3.0/

/// This class represents the States in the Finite State System.
/// Each state has a Dictionary with pairs (transition-state) showing
/// which state the FSM should be if a transition is fired while this state
/// is the current state.
/// Reason method is used to determine which transition should be fired .
/// Act method has the code to perform the actions the NPC is supposed to do if itÅLs on this state.
/// </summary>
public abstract class FSMState
{
    protected Dictionary<Transitions, StateIDs> map = new Dictionary<Transitions, StateIDs>();
    protected StateIDs stateID;
    public StateIDs ID { get { return stateID; } }
    protected Vector3 playerPos;
    protected Transform[] waypoints;
    protected float curRotSpeed;
    protected float curSpeed;

    public void AddTransition(Transitions transition, StateIDs id)
    {
        // Check if anyone of the args is invallid
        if (transition == Transitions.None || id == StateIDs.None)
        {
            Debug.LogWarning("FSMState : Null transition not allowed");
            return;
        }

        //Since this is a Deterministc FSM,
        //Check if the current transition was already inside the map
        if (map.ContainsKey(transition))
        {
            Debug.LogWarning("FSMState ERROR: transition is already inside the map");
            return;
        }

        map.Add(transition, id);
        Debug.Log("Added : " + transition + " with ID : " + id);
    }

    /// <summary>
    /// This method deletes a pair transition-state from this stateÅLs map.
    /// If the transition was not inside the stateÅLs map, an ERROR message is printed.
    /// </summary>
    public void DeleteTransition(Transitions trans)
    {
        // Check for NullTransition
        if (trans == Transitions.None)
        {
            Debug.LogError("FSMState ERROR: NullTransition is not allowed");
            return;
        }

        // Check if the pair is inside the map before deleting
        if (map.ContainsKey(trans))
        {
            map.Remove(trans);
            return;
        }
        Debug.LogError("FSMState ERROR: Transition passed was not on this StateÅLs List");
    }


    /// <summary>
    /// This method returns the new state the FSM should be if
    ///    this state receives a transition  
    /// </summary>
    public StateIDs GetOutputState(Transitions trans)
    {
        // Check for NullTransition
        if (trans == Transitions.None)
        {
            Debug.LogError("FSMState ERROR: NullTransition is not allowed");
            return StateIDs.None;
        }

        // Check if the map has this transition
        if (map.ContainsKey(trans))
        {
            return map[trans];
        }

        Debug.LogError("FSMState ERROR: " + trans + " Transition passed to the State was not on the list");
        return StateIDs.None;
    }

    /// <summary>
    /// Used to initialize variables when re-entering state
    /// </summary>
    public virtual void EnterStateInit(Transform player, Transform npc)
    {
    }

    /// <summary>
    /// Decides if the state should transition to another on its list
    /// NPC is a reference to the npc tha is controlled by this class
    /// </summary>
    public abstract void Reason(Transform player, Transform npc);

    /// <summary>
    /// This method controls the behavior of the NPC in the game World.
    /// Every action, movement or communication the NPC does should be placed here
    /// NPC is a reference to the npc that is controlled by this class
    /// </summary>
    public abstract void Act(Transform player, Transform npc);

    /// <summary>
    /// Find the next point
    /// </summary>
    public virtual void FindNextPoint()
    {
        int randomIndex = Random.Range(0, waypoints.Length);
        playerPos = waypoints[randomIndex].position;
    }

    public Transform GetFurthestWayPoint(Transform trans)
    {
        return GetWayPoint(trans, true);
    }

    public Transform GetClosestWaypoint(Transform trans)
    {
        return GetWayPoint(trans);
    }

    private Transform GetWayPoint(Transform trans, bool furthest = false)
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            return null;
        }

        float currSqrMagnitude = (trans.position - waypoints[0].position).sqrMagnitude;
        Transform retValue = waypoints[0];

        for (int i = 1; i < waypoints.Length; i++)
        {
            float sqrMagnitude = (trans.position - waypoints[i].position).sqrMagnitude;

            if ((furthest && (sqrMagnitude > currSqrMagnitude)) || (!furthest && (sqrMagnitude < currSqrMagnitude)))
            {
                retValue = waypoints[i];
                currSqrMagnitude = sqrMagnitude;
            }
        }

        return retValue;
    }

    /// <summary>
    /// Check whether the next random position is the same as current position
    /// </summary>
    /// <param name="pos">position to check</param>
    protected virtual bool IsInCurrentRange(Transform trans, Vector3 pos, float range)
    {
        bool inRange = false;
        Vector3 targetDir = pos - trans.position;
        float angle = Vector3.Angle(targetDir, trans.forward);
        float dist = Vector3.Distance(trans.position, pos);

        //Debug.Log(angle);

        if (dist <= range && angle <= 65.0f)
        {
            Debug.DrawLine(trans.position, pos, Color.red);
            inRange = true;
        }
        return inRange;
    }

}