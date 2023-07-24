using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIElementFacePlayer : MonoBehaviour
{

    void Update()
    {
        gameObject.transform.LookAt(GlobalVariableStorage.instance.playerLocation.position);
    }
}
