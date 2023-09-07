using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapCamController : MonoBehaviour
{
    [SerializeField]
    private float cameraHeightOffset = 40.0f;
    private Vector3 offsetVector;
    [SerializeField]
    private GameObject cam;

    [SerializeField]
    private RectTransform playerUIIndicator;

    private void Start()
    {
        offsetVector = new Vector3(0.0f, cameraHeightOffset, 0.0f);
        playerUIIndicator.transform.rotation = Quaternion.FromToRotation(playerUIIndicator.transform.forward, playerUIIndicator.transform.up);
    }
    // Update is called once per frame
    void Update()
    {
        //Keep player in center of minimap view
        transform.position = GlobalVariableStorage.instance.playerLocation.position + offsetVector;

        //Rotate player UI Indicator so it matches the player's look direction
        Quaternion UIRot = Quaternion.LookRotation(cam.transform.forward);
        Quaternion Trans = Quaternion.Euler(new Vector3(0.0f, 0.0f, -1.0f * UIRot.eulerAngles.y - 180.0f));
        UIRot = UIRot.normalized;
        playerUIIndicator.transform.rotation = Trans;
    }
}
