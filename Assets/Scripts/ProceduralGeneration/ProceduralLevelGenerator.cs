using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralLevelGenerator : MonoBehaviour
{

    public struct levelElement
    {
        /*
         * Array size = # of Connections
         * [1,0] = FRONT   [-1,0] = BACK
         * [0,-1] = LEFT,   [0,1] = RIGHT
         * */
        public Vector2[] connections;
        public GameObject levelPref;
        public List<levelElement> possibleTypes;

        //Deep copy input LE
        internal static levelElement copyLE(levelElement le)
        {
            levelElement output = new levelElement();
            output.connections = new Vector2[le.connections.Length];
            le.connections.CopyTo(output.connections, 0);
            output.levelPref = le.levelPref;
            output.possibleTypes = le.possibleTypes;
            return output;
        }

        internal static bool hasConnection(levelElement le, Vector2 connection)
        {
            for (int i = 0; i < le.connections.Length; i++)
            {
                if (le.connections[i].Equals(connection))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public struct sePath
    {
        public bool[,] visited;
        public Queue<int[]> pathQueue;
    }
    [Header("Level Element Prefabs")]
    [SerializeField]
    private GameObject startEndRoom;
    [SerializeField]
    private GameObject emptySpace;
    [SerializeField]
    private GameObject[] challengeRooms;
    [SerializeField]
    private GameObject[] corridors;
    [SerializeField]
    private GameObject gate;

    [SerializeField]
    private int gridLength = 8;
    [SerializeField]
    private int gridWidth = 8;

    private levelElement SERoomElement;
    private levelElement empty;
    private levelElement[] challengeRoomLibrary;
    private levelElement[] corridorLibrary;
    private levelElement[,] procLevelLayout;
    public Vector2 getProcLevelDims()
    {
        return new Vector2(procLevelLayout.GetLength(0), procLevelLayout.GetLength(1));
    }
    public levelElement getElementAt(int x, int y)
    {
        return procLevelLayout[x, y];
    }
    // Start is called before the first frame update
    void Awake()
    {

        challengeRoomLibrary = new levelElement[challengeRooms.Length];
        corridorLibrary = new levelElement[corridors.Length];
        procLevelLayout = new levelElement[gridLength, gridWidth];
        initLevelElementLibraries();
        initProcLevelGen();
    }

    private void initLevelElementLibraries()
    {

        //SE Room
        SERoomElement = new levelElement();
        parseElementInfo(ref SERoomElement, startEndRoom);
        parseElementInfo(ref empty, emptySpace);

        int i = 0;
        //Corridors
        foreach (GameObject g in corridors)
        {
            parseElementInfo(ref corridorLibrary[i], g);
            i += 1;
        }
        i = 0;
        //Rooms
        foreach (GameObject g in challengeRooms)
        {
            parseElementInfo(ref challengeRoomLibrary[i], g);
            i += 1;
        }

    }

    //Initializing LE structs for each possible LE
    private void parseElementInfo(ref levelElement lvl, GameObject elementPref)
    {
        OrientLevelElement o = elementPref.GetComponent<OrientLevelElement>();
        List<Vector2> validConnectionPoints = new List<Vector2>();
        if (o.getOrientOfName("CONNECTION_POINT_FRONT") != null)
        {
            validConnectionPoints.Add(new Vector2(1, 0));
        }
        if (o.getOrientOfName("CONNECTION_POINT_BACK") != null)
        {
            validConnectionPoints.Add(new Vector2(-1, 0));
        }
        if (o.getOrientOfName("CONNECTION_POINT_LEFT") != null)
        {
            validConnectionPoints.Add(new Vector2(0, -1));
        }
        if (o.getOrientOfName("CONNECTION_POINT_RIGHT") != null)
        {
            validConnectionPoints.Add(new Vector2(0, 1));
        }
        lvl.connections = new Vector2[validConnectionPoints.Count];
        int i = 0;
        foreach (Vector2 v in validConnectionPoints)
        {
            lvl.connections[i] = new Vector2(v.x, v.y);
            i += 1;
        }

        lvl.levelPref = elementPref;
        lvl.possibleTypes = new List<levelElement>();
    }

    private void initProcLevelGen()
    {
        //fill each spot with superpositions
        initLevelLayout();

        //Spawn start room
        int[] startIndex = new int[] { UnityEngine.Random.Range(0, (int)getProcLevelDims().x), UnityEngine.Random.Range(0, (int)getProcLevelDims().y) };
        //procLevelLayout[startIndex[0], startIndex[1]] = SERoomElement;
        procLevelLayout[startIndex[0], startIndex[1]] = levelElement.copyLE(SERoomElement);
        procLevelLayout[startIndex[0], startIndex[1]].possibleTypes = new List<levelElement>();
        GameObject startPos = Instantiate(procLevelLayout[startIndex[0], startIndex[1]].levelPref);
        procLevelLayout[startIndex[0], startIndex[1]].levelPref = startPos;

        startPos.GetComponent<OrientLevelElement>().setOurOrientation(startPos.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_LEFT").transform);

        //if the startPos spawns in an invalid spot, rotate it so the exit faces an adjacent space
        while (!checkBounds(startIndex, procLevelLayout[startIndex[0], startIndex[1]].connections[0]))
        {
            rotateLE(procLevelLayout[startIndex[0], startIndex[1]], startIndex);
        }

        //Spawn end room
        int[] endIndex = new int[] { UnityEngine.Random.Range(0, (int)getProcLevelDims().x), UnityEngine.Random.Range(0, (int)getProcLevelDims().y) };
        procLevelLayout[endIndex[0], endIndex[1]] = SERoomElement;
        procLevelLayout[endIndex[0], endIndex[1]].possibleTypes = new List<levelElement>();

        //if the endPos spawns in an invalid spot, rotate it so the exit faces an adjacent space
        sePath pathBetween = createPathBetween(startIndex, endIndex);

        //Loop through queue, find possible level elements which would satisfy the # of connections needed for the position
        //choose a random valid level element prefab and connect it to all adjacent LE which are a part of the initial path

        //previous LE on path (Queue starts with the Start element)
        int[] prev = pathBetween.pathQueue.Dequeue();
        procLevelLayout[prev[0], prev[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOurOrientation();

        int t = pathBetween.pathQueue.Count;
        for (int i = 0; i < t; i++)
        {
            int[] temp = pathBetween.pathQueue.Dequeue();
            //method to create a list of possible LEs:
            Stack<levelElement> possibleLEs = new Stack<levelElement>();
            if (pathBetween.pathQueue.Count > 0)
            {
                possibleLEs = getPossibleLEs(temp, pathBetween, prev);
            }
            else
            {
                possibleLEs.Push(SERoomElement);
            }
            levelElement[] leArr = new levelElement[possibleLEs.Count];
            possibleLEs.CopyTo(leArr, 0);

            //set tile on 2d array to a random LE from the list, connect to previous LE
            procLevelLayout[temp[0], temp[1]] = levelElement.copyLE(leArr[UnityEngine.Random.Range(0, leArr.Length)]);
            //instantiate and connect LE to the previous element
            GameObject le = GameObject.Instantiate(procLevelLayout[temp[0], temp[1]].levelPref);
            procLevelLayout[temp[0], temp[1]].levelPref = le;

            //checkOrientation(ref prev, difference);
            if (pathBetween.pathQueue.Count > 0)
            {
                checkOrientation(temp, pathBetween.visited);
            }
            else if(i == 0)
            {
                checkOrientation(temp, prev);
                checkOrientation(prev, temp);
            }
            else
            {
                checkOrientation(temp, prev);
            }

            connectLEs(procLevelLayout[prev[0], prev[1]], procLevelLayout[temp[0], temp[1]], new Vector2(temp[0] - prev[0], temp[1] - prev[1]));
            temp.CopyTo(prev, 0);
        }

        //Use wavefunction collapse algorithm to fill in all leftover empty spaces
        waveFunctionCollapse();
        removeUnconnectedAreas();
        blockDeadEnds();

    }

    private void waveFunctionCollapse()
    {
        //create list of all uncollapsed positions in the 2d array
        List<Vector2> uncollapsed = new List<Vector2>();
        uncollapsed = fillUncollapsed();

        //iterate through list and update the possible LEs to account for adjacent collapsed positions
        int i = 0;
        while (i < uncollapsed.Count)
        {
            if (updatePossibleLEs(procLevelLayout[(int)uncollapsed[i].x, (int)uncollapsed[i].y], uncollapsed[i]))
            {
                //remove collapsed LE from uncollapsed list, 
                uncollapsed.RemoveAt(i);
                i = 0;
            }
            else
            {
                //only incremement i if we didn't remove an element from uncollapsed
                i++;
            }
        }

        //START WF LOOP
        Queue collapseQueue = new Queue();

        while (uncollapsed.Count > 0)
        {
            //get LE from list with smallest entropy
            Vector2 lowest = new Vector2(0, 0);
            int entropy = int.MaxValue;
            int lowestIdex = 0;

            int inc = 0;
            foreach (Vector2 v in uncollapsed)
            {
                if (procLevelLayout[(int)v.x, (int)v.y].possibleTypes.Count <= entropy)
                {
                    lowest = new Vector2(v.x, v.y);
                    lowestIdex = inc;
                    entropy = procLevelLayout[(int)v.x, (int)v.y].possibleTypes.Count;
                }
                inc++;
            }
            if (procLevelLayout[(int)lowest.x, (int)lowest.y].possibleTypes.Count > 0)
            {
                collapseLE(lowest);
                uncollapsed.RemoveAt(lowestIdex);
                int j = 0;
                while (j < uncollapsed.Count)
                {
                    if (updatePossibleLEs(procLevelLayout[(int)uncollapsed[j].x, (int)uncollapsed[j].y], uncollapsed[j]))
                    {
                        //remove collapsed LE from uncollapsed list, 
                        uncollapsed.RemoveAt(j);
                        j = 0;
                    }
                    else
                    {
                        //only incremement i if we didn't remove an element from uncollapsed
                        j++;
                    }
                }
            }
        }
    }

    //fills all uncollapsed LEs with full list of potential LEs
    private List<Vector2> fillUncollapsed()
    {
        List<Vector2> output = new List<Vector2>();
        for (int i = 0; i < getProcLevelDims().x; i++)
        {
            for (int j = 0; j < getProcLevelDims().y; j++)
            {
                if (procLevelLayout[i, j].possibleTypes.Count != 0)
                {
                    output.Add(new Vector2(i, j));
                    //fill possibletypes with full list
                    for (int k = 0; k < corridorLibrary.Length; k++)
                    {
                        procLevelLayout[i, j].possibleTypes.Add(challengeRoomLibrary[k]);
                        procLevelLayout[i, j].possibleTypes.Add(corridorLibrary[k]);
                    }
                }
            }
        }
        return output;
    }

    //generates list of possible LEs, checks if its a candidate for collapsing and does so if true
    private bool updatePossibleLEs(levelElement le, Vector2 index)
    {
        if (procLevelLayout[(int)index[0], (int)index[1]].levelPref != null)
        {
            return false;
        }
        List<Vector2> connections = new List<Vector2>();
        int noConnectionCount = 0;
        int emptySpots = 0;

        //check bounds then check if there's a collapsed element in the position. If connection then add to connection list, else increment
        //no connection counter. If no connection increment no connection counter
        if (index[0] + 1 < procLevelLayout.GetLength(0) && procLevelLayout[(int)index[0] + 1, (int)index[1]].levelPref != null)
        {
            if (checkForConnection(procLevelLayout[(int)index[0] + 1, (int)index[1]], new Vector2(-1, 0)))
            {
                connections.Add(new Vector2(1, 0));
            }
            else
            {
                noConnectionCount++;
            }
        }
        else
        {
            emptySpots++;
        }
        if (index[0] - 1 >= 0 && procLevelLayout[(int)index[0] - 1, (int)index[1]].levelPref != null)
        {
            if (checkForConnection(procLevelLayout[(int)index[0] - 1, (int)index[1]], new Vector2(1, 0)))
            {
                connections.Add(new Vector2(-1, 0));
            }
            else
            {
                noConnectionCount++;
            }
        }
        else
        {
            emptySpots++;
        }
        if (index[1] + 1 < procLevelLayout.GetLength(1) && procLevelLayout[(int)index[0], (int)index[1] + 1].levelPref != null)
        {
            if (checkForConnection(procLevelLayout[(int)index[0], (int)index[1] + 1], new Vector2(0, -1)))
            {
                connections.Add(new Vector2(0, 1));
            }
            else
            {
                noConnectionCount++;
            }
        }
        else
        {
            emptySpots++;
        }
        if (index[1] - 1 >= 0 && procLevelLayout[(int)index[0], (int)index[1] - 1].levelPref != null)
        {
            if (checkForConnection(procLevelLayout[(int)index[0], (int)index[1] - 1], new Vector2(0, 1)))
            {
                connections.Add(new Vector2(0, -1));
            }
            else
            {
                noConnectionCount++;
            }
        }
        else
        {
            emptySpots++;
        }
        procLevelLayout[(int)index[0], (int)index[1]].possibleTypes = new List<levelElement>();
        switch (connections.Count)
        {
            case 1:
                if(noConnectionCount >= 3)
                {
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(empty);
                }
                else if(emptySpots == 1 && noConnectionCount == 2)
                {
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[0]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[0]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[1]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[1]);
                }
                else if(emptySpots == 2 && noConnectionCount == 1)
                {
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[0]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[0]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[1]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[1]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[2]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[2]);
                }
                break;
            case 2:
                //Debug.Log(Mathf.Abs(connections[0].x) + Mathf.Abs(connections[1].x));
                //Debug.Log(Mathf.Abs(connections[0].y) + Mathf.Abs(connections[1].y));
                if (Mathf.Abs(connections[0].x) + Mathf.Abs(connections[1].x) == 2.0f && Mathf.Abs(connections[0].y) + Mathf.Abs(connections[1].y) == 0.0f)
                {
                    //add straight 2 way
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[0]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[0]);
                }
                else if (Mathf.Abs(connections[0].x) + Mathf.Abs(connections[1].x) == 0.0f && Mathf.Abs(connections[0].y) + Mathf.Abs(connections[1].y) == 2.0f)
                {
                    //add straight 2 way
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[0]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[0]);
                }
                else if (Mathf.Abs(connections[0].x) + Mathf.Abs(connections[1].x) == 1.0f && Mathf.Abs(connections[0].y) + Mathf.Abs(connections[1].y) == 1.0f)
                {
                    //add corner 2 way
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[1]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[1]);
                }
                if (checkBounds(new int[] { (int)index.x, (int)index.y }) <= 1 && noConnectionCount <= 1)
                {
                    //add 3way
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[2]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[2]);
                }
                if (checkBounds(new int[] { (int)index.x, (int)index.y }) <= 0 && noConnectionCount <= 0)
                {
                    //add 4way
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[3]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[3]);
                }
                break;
            case 3:
                //add 3way
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[2]);
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[2]);

                if (checkBounds(new int[] { (int)index.x, (int)index.y }) <= 0 && noConnectionCount <= 0)
                {
                    //add 4way
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[3]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[3]);
                }
                break;
            case 4:
                //add 4way
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[3]);
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[3]);
                break;
        }
        switch (emptySpots)
        {
            case 2:
                if(connections.Count <= 1)
                {
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(empty);
                }
                break;
            case 3:
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[0]);
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[1]);
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[0]);
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[1]);
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[2]);
                procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[2]);
                break;
            case 4:
                for (int i = 0; i < corridorLibrary.Length; i++)
                {
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(corridorLibrary[i]);
                    procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(challengeRoomLibrary[i]);
                }
                break;
        }
        //Catch for no connections and no uncollapsed adj
        if(noConnectionCount + checkBounds(new int[] { (int)index.x, (int)index.y }) >= 4)
        {
            procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Clear();
            procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(empty);
        }

        //Catch for when no elements could be predicted
            if (procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Count == 0)
        {
            procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Add(empty);
        }

        //check if the element is a candidate for collapsing (possible LEs = 2)
        if (procLevelLayout[(int)index[0], (int)index[1]].possibleTypes.Count <= 2)
        {
            collapseLE(index);
            //return true if it collapses the LE
            return true;
        }
        return false;
        //check all 4 adjacent positions for collapsed elements
        //if collapsed element found, check if it has a connection pointing to the level element
        //  if yes: level element with c+1 connections needed, if no level element with c-1 connections needed

        //Store directions of adjacent element for cases w 2 connections (checking if its straight 2way or corner 2way
    }

    private bool checkForConnection(levelElement le, Vector2 check)
    {
        foreach (Vector2 v in le.connections)
        {
            if (v.Equals(check))
            {
                return true;
            }
        }
        return false;
    }

    //choose random level element from possible LEs, set that as the active LE, dump possible LE list
    private void collapseLE(Vector2 index)
    {
        //Debug.Log(index.x);
        //Debug.Log(index.y);
        //Debug.Log("COUNT: " + procLevelLayout[(int)index.x, (int)index.y].possibleTypes.Count);
        //if(procLevelLayout[(int)index.x, (int)index.y].possibleTypes.Count == 0)
        //{
            //Debug.Log("JKALSHLJHSA");
        //}

        procLevelLayout[(int)index.x, (int)index.y] =
            levelElement.copyLE(procLevelLayout[(int)index.x, (int)index.y].possibleTypes
            [(int)UnityEngine.Random.Range(0, procLevelLayout[(int)index.x, (int)index.y].possibleTypes.Count - 1)]);
        GameObject go = GameObject.Instantiate(procLevelLayout[(int)index.x, (int)index.y].levelPref);
        procLevelLayout[(int)index.x, (int)index.y].levelPref = go;
        procLevelLayout[(int)index.x, (int)index.y].possibleTypes.Clear();

        //procLevelLayout[(int)index.x, (int)index.y].levelPref.transform.position = procLevelLayout[(int)index.x + 1, (int)index.y].levelPref.transform.position;

        checkOrientation(new int[] { (int)index.x, (int)index.y });
    }

    //use randomized depth-first search to find a path between the start and end point and return the result
    private sePath createPathBetween(int[] start, int[] end)
    {
        //init array of false
        bool[,] visited = new bool[procLevelLayout.GetLength(0), procLevelLayout.GetLength(1)];

        Queue<int[]> path = new Queue<int[]>();
        path = DFS(start, end, path, visited);
        sePath p;
        p.pathQueue = path;
        p.visited = visited;
        return p;

    }

    //returns a stack containing all possible level elements for the level element on the start-end path
    private Stack<levelElement> getPossibleLEs(int[] index, sePath path, int[] prev)
    {
        Stack<levelElement> s = new Stack<levelElement>();

        //find how many boarders this tile has. If any, don't add 4ways to stack
        int numBorders = checkBounds(index);
        if (numBorders == 0)
        {
            s.Push(corridorLibrary[corridorLibrary.Length - 1]);
            s.Push(challengeRoomLibrary[challengeRoomLibrary.Length - 1]);
        }

        //check number of connecting tiles adjacent. If <= 2 add 2way as well as 3way
        List<Vector2> Connections = checkConnectionCount(index, path.visited);
        //AND ABSOLUTE VALUE OF CONNECTION INDEX MINUS PREVIOUS INDEX == 2,0 OR 0,2

        if (Connections.Count <= 2)
        {
            //Subtract connection points from each other to find out flow of connections
            int diffX = (int)(Connections[0].x - Connections[1].x);
            int diffY = (int)(Connections[0].y - Connections[1].y);
            //IF prev and next are in a straight line (difference between next and prev
            if (Mathf.Abs(diffX) == 2 && diffY == 0)
            {
                s.Push(corridorLibrary[0]);
                s.Push(challengeRoomLibrary[0]);
            }
            else if (Mathf.Abs(diffY) == 2 && diffX == 0)
            {
                s.Push(corridorLibrary[0]);
                s.Push(challengeRoomLibrary[0]);
            }
            //else add the corners
            else
            {
                s.Push(corridorLibrary[1]);
                s.Push(challengeRoomLibrary[1]);
            }

        }
        if (Connections.Count <= 3)
        {
            s.Push(corridorLibrary[2]);
            s.Push(challengeRoomLibrary[2]);
        }
        return s;
    }

    //Depth First Search used to generate path between predetermined start and endpoints
    private Queue<int[]> DFS(int[] loc, int[] end, Queue<int[]> path, bool[,] visited)
    {
        //add start to visited tiles
        visited[loc[0], loc[1]] = true;
        //add start to path
        path.Enqueue(loc);

        //check if our current cell is the end point, if yes return the path (EXIT CASE)
        if (loc[0] == end[0] && loc[1] == end[1])
        {
            return path;
        }

        List<Vector2> possibleDirs = new List<Vector2>();
        //iterate through each possible direction randomly calling DFS to iterate the search
        if (procLevelLayout[loc[0], loc[1]].connections == null)
        {
            possibleDirs = new List<Vector2> { new Vector2(0, 1), new Vector2(0, -1), new Vector2(1, 0), new Vector2(-1, 0) };
        }
        else
        {
            possibleDirs.Add(procLevelLayout[loc[0], loc[1]].connections[0]);
        }

        int c = possibleDirs.Count;
        for (int i = 0; i < c; i++)
        {
            int rand = (int)UnityEngine.Random.Range(0, possibleDirs.Count);
            if (checkBounds(loc, possibleDirs[rand]) && visited[loc[0] + (int)possibleDirs[rand].x, loc[1] + (int)possibleDirs[rand].y] != true)
            {
                int[] temp = new int[loc.Length];
                loc.CopyTo(temp, 0);
                temp[0] += (int)possibleDirs[rand].x;
                temp[1] += (int)possibleDirs[rand].y;
                Queue<int[]> copy = new Queue<int[]>(path);
                //reccursively call output
                Queue<int[]> output = DFS(temp, end, copy, visited);
                possibleDirs.RemoveAt(rand);
                if (output != null)
                {
                    return output;
                }
            }
            else
            {
                possibleDirs.RemoveAt(rand);
            }
        }

        return null;

    }

    //Connection method used when generating the start-finish path. Sets orientations according to the difference between current and previous vectors
    private void connectLEs(levelElement prev, levelElement cur, Vector2 difference)
    {
        if (Mathf.Abs(difference.x) > 1.0f || Mathf.Abs(difference.y) > 1.0f)
        {
            Debug.Log("NOT ADJ");
        }
        //switch case for different connections
        //connect cur to prev
        //cur.levelPref.gameObject.GetComponent<OrientLevelElement>().setOurOrientation();
        OrientLevelElement cO = cur.levelPref.gameObject.GetComponent<OrientLevelElement>();
        OrientLevelElement pO = prev.levelPref.gameObject.GetComponent<OrientLevelElement>();
        switch (difference.x)
        {
            case (1.0f):
                cO.setOrientTarget(pO.getOrientOfName("CONNECTION_POINT_FRONT").transform);
                cO.setOurOrientation(cO.getOrientOfName("CONNECTION_POINT_BACK").transform);
                break;
            case (-1.0f):
                cO.setOrientTarget(pO.getOrientOfName("CONNECTION_POINT_BACK").transform);
                cO.setOurOrientation(cO.getOrientOfName("CONNECTION_POINT_FRONT").transform);
                break;
        }
        switch (difference.y)
        {
            case (1.0f):
                cO.setOrientTarget(pO.getOrientOfName("CONNECTION_POINT_RIGHT").transform);
                cO.setOurOrientation(cO.getOrientOfName("CONNECTION_POINT_LEFT").transform);
                break;
            case (-1.0f):
                cO.setOrientTarget(pO.getOrientOfName("CONNECTION_POINT_LEFT").transform);
                cO.setOurOrientation(cO.getOrientOfName("CONNECTION_POINT_RIGHT").transform);
                break;
        }
        cO.orientTerrain();
    }

    //for each adjacent space to le

    //if visited = true
    //check le connection, if false rotate LE and call checkOrientation again
    //Used when creating path between start and endpoints
    private void checkOrientation(int[] le, bool[,] visited)
    {

        //FRONT
        if (le[0] + 1 < procLevelLayout.GetLength(0) && visited[le[0] + 1, le[1]] == true)
        {
            bool matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(1, 0)))
                {
                    matches = true;
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le, visited);
            }
        }
        //BACK
        if (le[0] - 1 >= 0 && visited[le[0] - 1, le[1]] == true)
        {
            bool matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(-1, 0)))
                {
                    matches = true;
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le, visited);
            }
        }
        //RIGHT
        if (le[1] + 1 < procLevelLayout.GetLength(1) && visited[le[0], le[1] + 1] == true)
        {
            bool matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(0, 1)))
                {
                    matches = true;
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le, visited);
            }
        }
        //LEFT
        if (le[1] - 1 >= 0 && visited[le[0], le[1] - 1] == true)
        {
            bool matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(0, -1)))
                {
                    matches = true;
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le, visited);
            }
        }
    }
    //alt checkOrientation method used in WFC
    private void checkOrientation(int[] le)
    {

        //Debug.Log(le[0] + " " + le[1]);
        bool matches = true;
        //backup orient setting
        setOrientToAnyAdj(le);

        if (procLevelLayout[le[0], le[1]].levelPref.name.Equals("Empty(Clone)"))
        {
            procLevelLayout[le[0], le[1]].levelPref.GetComponent<OrientLevelElement>().orientTerrain();
            return;
        }

        //FRONT
        //if adjacent le exists and has connection pointing back to le
        if (le[0] + 1 < procLevelLayout.GetLength(0) && procLevelLayout[le[0] + 1, le[1]].levelPref
            && levelElement.hasConnection(procLevelLayout[le[0] + 1, le[1]], new Vector2(-1, 0)))
        {
            matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(1, 0)))
                {
                    matches = true;
                    //SETTING UP ORIENT TARGET AND OUR ORIENTATION
                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOrientTarget
                        (procLevelLayout[le[0] + 1, le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_BACK").transform);

                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOurOrientation
                        (procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_FRONT").transform);
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le);
            }
        }
        //BACK
        if (le[0] - 1 >= 0 && procLevelLayout[le[0] - 1, le[1]].levelPref
            && levelElement.hasConnection(procLevelLayout[le[0] - 1, le[1]], new Vector2(1, 0)))
        {
            matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(-1, 0)))
                {
                    matches = true;
                    //SETTING UP ORIENT TARGET AND OUR ORIENTATION
                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOrientTarget
                        (procLevelLayout[le[0] - 1, le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_FRONT").transform);

                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOurOrientation
                        (procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_BACK").transform);
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le);
            }
        }
        //RIGHT
        if (le[1] + 1 < procLevelLayout.GetLength(1) && procLevelLayout[le[0], le[1] + 1].levelPref
            && levelElement.hasConnection(procLevelLayout[le[0], le[1] + 1], new Vector2(0, -1)))
        {
            matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(0, 1)))
                {
                    matches = true;
                    //SETTING UP ORIENT TARGET AND OUR ORIENTATION
                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOrientTarget
                        (procLevelLayout[le[0], le[1] + 1].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_LEFT").transform);

                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOurOrientation
                        (procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_RIGHT").transform);
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le);
            }
        }
        //LEFT
        if (le[1] - 1 >= 0 && procLevelLayout[le[0], le[1] - 1].levelPref
            && levelElement.hasConnection(procLevelLayout[le[0], le[1] - 1], new Vector2(0, 1)))
        {
            matches = false;
            for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
            {
                //check all the connections on le to see if they orient properly
                if (procLevelLayout[le[0], le[1]].connections[i].Equals(new Vector2(0, -1)))
                {
                    matches = true;
                    //SETTING UP ORIENT TARGET AND OUR ORIENTATION
                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOrientTarget
                        (procLevelLayout[le[0], le[1] - 1].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_RIGHT").transform);

                    procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().setOurOrientation
                        (procLevelLayout[le[0], le[1]].levelPref.gameObject.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_LEFT").transform);
                }
            }
            if (!matches)
            {
                rotateLE(ref procLevelLayout[le[0], le[1]]);
                checkOrientation(le);
            }
        }
        //Properly oriented after this point
        if (matches)
        {
            procLevelLayout[le[0], le[1]].levelPref.GetComponent<OrientLevelElement>().orientTerrain();
        }
    }

    //Special method which uses forceGetOrient instead of getOrient. Used when placing elements generated during the WFC phase of generation.
    //Finds any adjacent position that's collapsed and sets the orientations accordingly. Uses the "hidden" Orientation GOs which don't mark connections
    private void setOrientToAnyAdj(int[] LE)
    {
        if (LE[0] - 1 >= 0 && procLevelLayout[(int)LE[0] - 1, (int)LE[1]].possibleTypes.Count == 0)
        {
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOrientTarget
                (procLevelLayout[LE[0] - 1, LE[1]].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_FRONT"));
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOurOrientation
                (procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_BACK"));
        }
        else if (LE[0] + 1 < procLevelLayout.GetLength(0) && procLevelLayout[(int)LE[0] + 1, (int)LE[1]].possibleTypes.Count == 0)
        {
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOrientTarget
            (procLevelLayout[LE[0] + 1, LE[1]].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_BACK"));
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOurOrientation
                (procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_FRONT"));
        }
        else if (LE[1] - 1 >= 0 && procLevelLayout[(int)LE[0], (int)LE[1] - 1].possibleTypes.Count == 0)
        {
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOrientTarget
            (procLevelLayout[LE[0], LE[1] - 1].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_RIGHT"));
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOurOrientation
                (procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_LEFT"));
        }
        else if (LE[1] + 1 < procLevelLayout.GetLength(0) && procLevelLayout[(int)LE[0], (int)LE[1] + 1].possibleTypes.Count == 0)
        {
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOrientTarget
            (procLevelLayout[LE[0], LE[1] + 1].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_LEFT"));
            procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().setOurOrientation
                (procLevelLayout[LE[0], LE[1]].levelPref.GetComponent<OrientLevelElement>().forceGetOrient("CONNECTION_POINT_RIGHT"));
        }
    }

    //Alt orientation checking method which only takes the connecting element into account when checking. Only used by end element of generated queue
    private void checkOrientation(int[] le, int[] connection)
    {
        Vector2 difference = new Vector2(connection[0] - le[0], connection[1] - le[1]);

        bool matches = false;
        for (int i = 0; i < procLevelLayout[le[0], le[1]].connections.Length; i++)
        {
            if (procLevelLayout[le[0], le[1]].connections[i].Equals(difference))
            {
                matches = true;
            }
        }
        if (!matches)
        {
            rotateLE(ref procLevelLayout[le[0], le[1]]);
            checkOrientation(le, connection);
        }
    }

    //check bounds for direction, returns true if within bounds
    private bool checkBounds(int[] bounds, Vector2 dir)
    {
        if (bounds[0] + (int)dir.x >= 0 && bounds[0] + (int)dir.x < procLevelLayout.GetLength(0) && bounds[1] + (int)dir.y >= 0 && bounds[1] + dir.y
            < procLevelLayout.GetLength(1))
        {
            return true;
        }
        return false;
    }

    //check bounds in all directions, returns # of borders
    private int checkBounds(int[] bounds)
    {
        int output = 0;
        if (bounds[0] + 1 >= procLevelLayout.GetLength(0))
        {
            output += 1;
        }
        if (bounds[0] - 1 < 0)
        {
            output += 1;
        }
        if (bounds[1] + 1 >= procLevelLayout.GetLength(1))
        {
            output += 1;
        }
        if (bounds[1] - 1 < 0)
        {
            output += 1;
        }
        return output;
    }

    //return number of connections, used in path generation
    private List<Vector2> checkConnectionCount(int[] index, bool[,] pathArray)
    {
        List<Vector2> output = new List<Vector2>();
        if (checkBounds(index, new Vector2(1, 0)) && pathArray[index[0] + 1, index[1]] == true)
        {
            output.Add(new Vector2(index[0] + 1, index[1]));
        }
        if (checkBounds(index, new Vector2(-1.0f, 0)) && pathArray[index[0] - 1, index[1]] == true)
        {
            output.Add(new Vector2(index[0] - 1, index[1]));
        }
        if (checkBounds(index, new Vector2(0, 1)) && pathArray[index[0], index[1] + 1] == true)
        {
            output.Add(new Vector2(index[0], index[1] + 1));
        }
        if (checkBounds(index, new Vector2(0, -1)) && pathArray[index[0], index[1] - 1] == true)
        {
            output.Add(new Vector2(index[0], index[1] - 1));
        }
        return output;
    }

    //rotates level element so one of its exits points to another valid space in the 2d array (used for Start End Element fixing)
    private void rotateLE(levelElement le, int[] index)
    {
        if (index[0] + le.connections[0].x >= 0 && index[0] + le.connections[0].x < procLevelLayout.GetLength(0) &&
            index[1] + le.connections[0].y >= 0 && index[1] + le.connections[0].y < procLevelLayout.GetLength(1))
        {
            return;
        }
        else
        {
            le.levelPref.transform.Rotate(new Vector3(0, 90.0f, 0));
            updateLEConnections(ref le, 90.0f);
        }
    }
    private void rotateLE(ref levelElement le)
    {
        le.levelPref.transform.Rotate(new Vector3(0, 90.0f, 0));
        updateLEConnections(ref le, 90.0f);
    }

    //Updates level element connections after rotating by rot
    private void updateLEConnections(ref levelElement le, float rot)
    {
        /*
         * Array size = # of Connections
         * [1,0] = FRONT   [-1,0] = BACK
         * [0,-1] = LEFT,   [0,1] = RIGHT
         * */
        OrientLevelElement orient = le.levelPref.GetComponent<OrientLevelElement>();
        if (rot == 90.0f)
        {

            for (int i = 0; i < le.connections.Length; i++)
            {
                //front
                if (le.connections[i].Equals(new Vector2(1, 0)))
                {
                    le.connections[i] = new Vector2(0, -1);

                }
                //back
                else if (le.connections[i].Equals(new Vector2(-1, 0)))
                {
                    le.connections[i] = new Vector2(0, 1);
                }
                //right
                else if (le.connections[i].Equals(new Vector2(0, 1)))
                {
                    le.connections[i] = new Vector2(1, 0);
                }
                //left
                else if (le.connections[i].Equals(new Vector2(0, -1)))
                {
                    le.connections[i] = new Vector2(-1, 0);
                }
            }
            orient.updateConnectionNames();
        }
    }

    //initializes the possible types of all elements in proc layout array to be the superposition of all possible elements
    private void initLevelLayout()
    {
        List<levelElement> allLEs = new List<levelElement>();
        for (int i = 0; i < challengeRoomLibrary.Length; i++)
        {
            allLEs.Add(challengeRoomLibrary[i]);
        }
        for (int i = 0; i < corridorLibrary.Length; i++)
        {
            allLEs.Add(corridorLibrary[i]);
        }
        allLEs.Add(SERoomElement);


        for (int i = 0; i < getProcLevelDims().x; i++)
        {
            for (int j = 0; j < getProcLevelDims().y; j++)
            {
                procLevelLayout[i, j].possibleTypes = new List<levelElement>();
                //procLevelLayout[i, j].possibleTypes = allLEs;
                for (int k = 0; k < allLEs.Count; k++)
                {
                    procLevelLayout[i, j].possibleTypes.Add(allLEs[k]);
                }

            }
        }
    }

    //Cleans up unconnected level elements / strings of level elements after generation is complete. Cleanup step
    private void removeUnconnectedAreas()
    {
        //create temp list of unvisited LEs
        bool[,] les = new bool[gridLength, gridWidth];
        int[] startIndex = new int[] { 0, 0 };
        for (int i = 0; i < getProcLevelDims().x; i++)
        {
            for (int j = 0; j < getProcLevelDims().y; j++)
            {
                if (procLevelLayout[i, j].levelPref.name.Equals("Start_Finish(Clone)"))
                {
                    startIndex = new int[] { i, j };
                }
                les[i, j] = false;
            }
        }

        findUnconnected(ref les, startIndex);

        for (int i = 0; i < getProcLevelDims().x; i++)
        {
            for (int j = 0; j < getProcLevelDims().y; j++)
            {
                if (!les[i, j])
                {
                    Destroy(procLevelLayout[i, j].levelPref);
                    //Don't really have to move this into place, its empty...
                    procLevelLayout[i, j] = levelElement.copyLE(empty);
                    procLevelLayout[i, j].levelPref = GameObject.Instantiate(emptySpace);
                    
                }
            }

        }

            }

    //Recursive method that returns a 2D array marking all unconnected areas in the level as FALSE
    private bool[,] findUnconnected(ref bool[,] les, int[] startIndex)
    {
        //mark as a part of the main area of level
        les[startIndex[0], startIndex[1]] = true;
        foreach (Vector2 connection in procLevelLayout[startIndex[0], startIndex[1]].connections)
        {
            ///if within bounds and adjacent space has connection
            if (checkBounds(new int[] { startIndex[0], startIndex[1]}, new Vector2(connection[0],connection[1]))
                && checkForConnection(procLevelLayout[startIndex[0] + (int)connection.x, startIndex[1] + (int)connection.y]
                , new Vector2((int)connection.x * -1, (int)connection.y * -1)))
            {
                //call findUnconnected again on new space
                if(!les[startIndex[0] + (int)connection.x, startIndex[1] + (int)connection.y])
                {
                    les = findUnconnected(ref les, new int[] { startIndex[0] + (int)connection.x, startIndex[1] + (int)connection.y });
                }
            }
        }
        return les;
    }

    //Finds all dead ends and instantiates a gate preventing the player from walking out of the level
    private void blockDeadEnds()
    {
        for (int i = 0; i < getProcLevelDims().x; i++)
        {
            for (int j = 0; j < getProcLevelDims().y; j++)
            {
                for (int k = 0; k < procLevelLayout[i, j].connections.Length; k++)
                {
                    //if the connection is within bounds continue checking, else block it
                    if (checkBounds(new int[] { i, j }, procLevelLayout[i, j].connections[k]))
                    {
                        //if there isn't a connection between this element and the adj element BUT there's a connection point present(inpassible)
                        if (!checkForConnection(procLevelLayout[i + (int)procLevelLayout[i, j].connections[k].x, j + (int)procLevelLayout[i, j].connections[k].y],
                                -1 * procLevelLayout[i, j].connections[k]))
                        {
                            //instantiate gate with parent set to LE's corresponding connection
                            GameObject gateInstance = GameObject.Instantiate(gate, procLevelLayout[i, j].levelPref.
                                GetComponent<OrientLevelElement>().getOrientByNum(procLevelLayout[i, j].connections[k]).transform);
                            //block connection with gate
                        }
                    }
                    else
                    {
                        //block connection with gate
                        GameObject gateInstance = GameObject.Instantiate(gate, procLevelLayout[i, j].levelPref.
                            GetComponent<OrientLevelElement>().getOrientByNum(procLevelLayout[i, j].connections[k]).transform);
                    }
                }
            }
        }
    }

    /*//reccursive method for generation. (OLD, USED FOR TESTING)
    private void generateLevel(int[] startIndex, GameObject prev)
    {
        //If there's a space to the right of us and there's a maching exit point at our start:
        if (startIndex[1] + 1 < procLevelLayout.GetLength(0) && checkCompatibility(new Vector2(0, 1), procLevelLayout[startIndex[0], startIndex[1]])
            && checkOccupancy(new int[] { startIndex[0], startIndex[1] + 1 }))
        {
            procLevelLayout[(int)startIndex[0], (int)startIndex[1] + 1] = corridorLibrary[2];
            GameObject elementGO = GameObject.Instantiate(procLevelLayout[(int)startIndex[0], (int)startIndex[1] + 1].levelPref);
            OrientLevelElement o = elementGO.GetComponent<OrientLevelElement>();
            o.setOurOrientation(o.getOrientOfName("CONNECTION_POINT_LEFT").transform);
            o.setOrientTarget(prev.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_RIGHT").transform);
            o.orientTerrain();
            generateLevel(new int[] { (int)startIndex[0], (int)startIndex[1] + 1 }, elementGO);
        }
        //left
        if (startIndex[1] - 1 >= 0 && checkCompatibility(new Vector2(0, -1), procLevelLayout[startIndex[0], startIndex[1]])
            && checkOccupancy(new int[] { startIndex[0], startIndex[1] - 1 }))
        {
            procLevelLayout[(int)startIndex[0], (int)startIndex[1] - 1] = corridorLibrary[2];
            GameObject elementGO = GameObject.Instantiate(procLevelLayout[(int)startIndex[0], (int)startIndex[1] - 1].levelPref);
            OrientLevelElement o = elementGO.GetComponent<OrientLevelElement>();
            o.setOurOrientation(o.getOrientOfName("CONNECTION_POINT_RIGHT").transform);
            o.setOrientTarget(prev.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_LEFT").transform);
            o.orientTerrain();
            generateLevel(new int[] { (int)startIndex[0], (int)startIndex[1] - 1 }, elementGO);
        }
        //up
        if (startIndex[0] + 1 < procLevelLayout.GetLength(1) && checkCompatibility(new Vector2(1, 0), procLevelLayout[startIndex[0], startIndex[1]])
            && checkOccupancy(new int[] { startIndex[0] + 1, startIndex[1] }))
        {
            procLevelLayout[(int)startIndex[0] + 1, (int)startIndex[1]] = corridorLibrary[2];
            GameObject elementGO = GameObject.Instantiate(procLevelLayout[(int)startIndex[0] + 1, (int)startIndex[1]].levelPref);
            OrientLevelElement o = elementGO.GetComponent<OrientLevelElement>();
            o.setOurOrientation(o.getOrientOfName("CONNECTION_POINT_BACK").transform);
            o.setOrientTarget(prev.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_FRONT").transform);
            o.orientTerrain();
            generateLevel(new int[] { (int)startIndex[0] + 1, (int)startIndex[1] }, elementGO);
        }
        //down
        if (startIndex[0] - 1 >= 0 && checkCompatibility(new Vector2(-1, 0), procLevelLayout[startIndex[0], startIndex[1]])
            && checkOccupancy(new int[] { startIndex[0] - 1, startIndex[1] }))
        {
            procLevelLayout[(int)startIndex[0] - 1, (int)startIndex[1]] = corridorLibrary[2];
            GameObject elementGO = GameObject.Instantiate(procLevelLayout[(int)startIndex[0] - 1, (int)startIndex[1]].levelPref);
            OrientLevelElement o = elementGO.GetComponent<OrientLevelElement>();
            o.setOurOrientation(o.getOrientOfName("CONNECTION_POINT_FRONT").transform);
            o.setOrientTarget(prev.GetComponent<OrientLevelElement>().getOrientOfName("CONNECTION_POINT_BACK").transform);
            o.orientTerrain();
            generateLevel(new int[] { (int)startIndex[0] - 1, (int)startIndex[1] }, elementGO);
        }
    }*/


}
