using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    private Transform[] wayPoints;
    [SerializeField]
    private float moveSpeed;

    private enum State
    {
        Inc,
        Dec
    }

    private Rigidbody rb;
    private int increment;
    private State activeState;
    // Start is called before the first frame update
    void Start()
    {
        increment = 1;
        activeState = State.Inc;
        rb = gameObject.GetComponent<Rigidbody>();
        StartCoroutine(moveToPosition());
    }

    private IEnumerator moveToPosition()
    {
        float elapsedTime = 0;
        Vector3 start = transform.position;
        while (elapsedTime <= moveSpeed)
        {
            elapsedTime += Time.fixedDeltaTime;
            Vector3 lerped = Vector3.Lerp(start, wayPoints[increment].position, elapsedTime/moveSpeed);
            rb.MovePosition(lerped);

            yield return new WaitForFixedUpdate();
        }
        incrementWaypoint();
        StopCoroutine(moveToPosition());
    }

    private void incrementWaypoint()
    {
            if (increment == wayPoints.Length - 1)
            {
                activeState = State.Dec;
            }
            else if (increment == 0)
            {
                activeState = State.Inc;
            }

            switch (activeState)
            {
                case State.Inc:
                    increment++;
                    break;

                case State.Dec:
                    increment--;
                    break;
            }
            StartCoroutine(moveToPosition());
    }

}
