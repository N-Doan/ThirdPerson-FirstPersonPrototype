using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *  Script attached to each Level Element used for orienting the element relative
 *  to its orient target and own orientation.
 * 
 */


public class OrientLevelElement : MonoBehaviour
{
    [SerializeField]
    private Transform orientTarget;
    public void setOrientTarget(Transform orient) { orientTarget = orient; }
    public Transform getOrientTarget() { return orientTarget; }


    [SerializeField]
    private Transform ourOrientation;
    //Set orient as active orient and Re-arrange parent/child relations so each other orient and the level itself are children
    //of the currently active Level Element
    public void setOurOrientation(Transform orient)
    {

        ourOrientation = orient;

        if (collidersAndMesh != null && ourOrientation != null)
        {
            collidersAndMesh.transform.SetParent(this.gameObject.transform);
            collidersAndMesh.transform.SetParent(ourOrientation);
            foreach (GameObject g in connectionPoints)
            {
                g.transform.SetParent(this.gameObject.transform);
            }
            foreach (GameObject g in connectionPoints)
            {
                if (g.GetInstanceID() != ourOrientation.GetInstanceID())
                {
                    g.transform.SetParent(ourOrientation);
                }
            }
            foreach (GameObject g in privateConnectionPoints)
            {
                if (g.GetInstanceID() != ourOrientation.GetInstanceID())
                {
                    g.transform.SetParent(ourOrientation);
                }
            }
        }
    }
    //Alt method for when no specific orient wanted as active
    public void setOurOrientation()
    {
        ourOrientation = connectionPoints[0].transform;

        if (collidersAndMesh != null && ourOrientation != null)
        {
            collidersAndMesh.transform.SetParent(ourOrientation);
            foreach (GameObject g in connectionPoints)
            {
                if (g.GetInstanceID() != ourOrientation.GetInstanceID())
                {
                    g.transform.SetParent(ourOrientation);
                }
            }
            foreach (GameObject g in privateConnectionPoints)
            {
                if (g.GetInstanceID() != ourOrientation.GetInstanceID())
                {
                    g.transform.SetParent(ourOrientation);
                }
            }
            //orientTerrain();
        }
    }
    public Transform getOurOrientation() { return ourOrientation; }

    private GameObject collidersAndMesh;
    [SerializeField]
    private List<GameObject> connectionPoints;

    //PRIVATE CONNECTION POINTS DO NOT INDICATE POTENTIAL BRIDGES (used for forcing proper orientation when such a thing can't be done)
    [SerializeField]
    private List<GameObject> privateConnectionPoints;

    public GameObject getOrientOfName(string name)
    {
        switch (name)
        {
            case "CONNECTION_POINT_FRONT":
                return findOrientWithRotation(new Vector3(0.0f, 90.0f, 0.0f));

            case "CONNECTION_POINT_BACK":
                return findOrientWithRotation(new Vector3(0.0f, 270.0f, 0.0f));

            case "CONNECTION_POINT_RIGHT":
                return findOrientWithRotation(new Vector3(0.0f, 0.0f, 0.0f));

            case "CONNECTION_POINT_LEFT":
                return findOrientWithRotation(new Vector3(0.0f, 180.0f, 0.0f));
        }
        return null;
    }

    //Special getOrient method which looks at both private and regular connection points when searching
    public Transform forceGetOrient(string name)
    {
        foreach (GameObject g in privateConnectionPoints)
        {
            if (g.name.Equals(name))
            {
                return g.transform;
            }
        }
        foreach (GameObject g in connectionPoints)
        {
            if (g.name.Equals(name))
            {
                return g.transform;
            }
        }
        return null;
    }

    //Update names of connection points after rotating the element
    public void updateConnectionNames()
    {
        foreach (GameObject g in connectionPoints)
        {
            if (g.transform.rotation.eulerAngles.y == 90.0f)
            {
                g.name = "CONNECTION_POINT_FRONT";
            }
            if (g.transform.rotation.eulerAngles.y == 180.0f)
            {
                g.name = "CONNECTION_POINT_LEFT";
            }
            if (g.transform.rotation.eulerAngles.y == 270.0f)
            {
                g.name = "CONNECTION_POINT_BACK";
            }
            if (g.transform.rotation.eulerAngles.y == 0f)
            {
                g.name = "CONNECTION_POINT_RIGHT";
            }
        }
        foreach (GameObject g in privateConnectionPoints)
        {
            if (g.transform.rotation.eulerAngles.y == 90.0f)
            {
                g.name = "CONNECTION_POINT_FRONT";
            }
            if (g.transform.rotation.eulerAngles.y == 180.0f)
            {
                g.name = "CONNECTION_POINT_LEFT";
            }
            if (g.transform.rotation.eulerAngles.y == 270.0f)
            {
                g.name = "CONNECTION_POINT_BACK";
            }
            if (g.transform.rotation.eulerAngles.y == 0f)
            {
                g.name = "CONNECTION_POINT_RIGHT";
            }
        }
    }

    //Finds orient with matching rotation
    private GameObject findOrientWithRotation(Vector3 rot)
    {
        foreach (GameObject g in connectionPoints)
        {
            if (rot.y == g.transform.rotation.eulerAngles.y)
            {
                return g;
            }
        }
        return null;
    }

    //Finds orient by vector input
    public GameObject getOrientByNum(Vector2 input)
    {
        switch (input.x)
        {
            case (1):
                return getOrientOfName("CONNECTION_POINT_FRONT");
            case (-1):
                return getOrientOfName("CONNECTION_POINT_BACK");
        }
        switch (input.y)
        {
            case (1):
                return getOrientOfName("CONNECTION_POINT_RIGHT");
            case (-1):
                return getOrientOfName("CONNECTION_POINT_LEFT");
        }
        return null;

    }

    // Start is called before the first frame update
    void Awake()
    {
        if (gameObject.transform.Find("CollidersAndMesh"))
        {
            collidersAndMesh = gameObject.transform.Find("CollidersAndMesh").gameObject;
        }
        if (collidersAndMesh != null && ourOrientation != null)
        {
            collidersAndMesh.transform.SetParent(ourOrientation);
            foreach (GameObject g in connectionPoints)
            {
                if (g.GetInstanceID() != ourOrientation.GetInstanceID())
                {
                    g.transform.SetParent(ourOrientation);
                }
            }
            //orientTerrain();
        }
    }

    //Moves GO to orientTarget plues the offset between our orient and the GO's position. This let's us easily
    //rotate the Level element by its GO position
    public bool orientTerrain()
    {
        if (ourOrientation != null && orientTarget != null)
        {

            gameObject.transform.position = orientTarget.position;
            gameObject.transform.position = gameObject.transform.position + ((ourOrientation.position - gameObject.transform.position) * -1);
            return true;
        }
        else
        {
            Debug.Log("NULL");
            return false;
        }
    }
}
