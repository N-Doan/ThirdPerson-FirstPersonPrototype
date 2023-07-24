using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Must be in Ascending Order")]
    [SerializeField]
    private Transform[] checkpoints;
    private Transform activeCheckpoint;


    private void Start()
    {
        activeCheckpoint = checkpoints[0];
    }
    public void setCheckpoint(Transform newCheckpoint)
    {
        bool success = false;
        foreach(Transform t in checkpoints)
        {
            t.GetChild(0).gameObject.SetActive(false);
            if (t.position.Equals(newCheckpoint.position))
            {
                activeCheckpoint = newCheckpoint;
                t.GetChild(0).gameObject.SetActive(true);
                t.gameObject.GetComponent<Collider>().enabled = false;
                success = true;
            }
        }
        if (!success)
        {
            Debug.Log("No Matching Checkpoint Found!");
        }
    }

    public void takePlayerToActiveCheckpoint()
    {
        GlobalVariableStorage.instance.playerLocation.position = activeCheckpoint.position;
    }

}
