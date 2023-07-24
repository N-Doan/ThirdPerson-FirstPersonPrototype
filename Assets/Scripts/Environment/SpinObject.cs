using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinObject : MonoBehaviour
{
    [SerializeField]
    private float rotSpeed;
    private Vector3 center;
    private Vector3 rotVector;
    private Rigidbody rb;
    private void Start()
    {
        center = gameObject.GetComponent<Renderer>().bounds.center;
        rb = gameObject.GetComponent<Rigidbody>();
        rotVector = new Vector3(0, rotSpeed, 0);
    }
    // Update is called once per frame
    void Update()
    {
        //gameObject.transform.RotateAround(center, new Vector3(0, 1, 0), rotSpeed);
        //rb.ro
    }

    private void FixedUpdate()
    {
        Quaternion deltaRot = Quaternion.Euler(rotVector * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation * deltaRot);
    }
}
