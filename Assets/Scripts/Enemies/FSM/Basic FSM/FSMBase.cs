using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMBase : MonoBehaviour
{
    //Player Transform
    protected Transform playerTransform;

    //Next destination position
    protected Vector3 destPos;

    protected virtual void Initialize() { }
    protected virtual void FSMUpdate() { }
    protected virtual void FSMFixedUpdate() { }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        FSMUpdate();
    }

    private void FixedUpdate()
    {
        FSMFixedUpdate();
    }
}