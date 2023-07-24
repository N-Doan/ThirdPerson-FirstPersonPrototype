using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float distanceFrom = 1;


    private Vector3 previousPosition;
    private Vector3 currentPosition;
    private Camera cam;
    private float mouseX, mouseY;

    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        offset = gameObject.transform.localPosition + target.localPosition;
        previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.localPosition = target.localPosition + offset;
        updateRotation();
        mouseBoundsCheck();
    }

    private void updateRotation()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");

        /*currentPosition = cam.ScreenToViewportPoint(Input.mousePosition);
        Vector3 difference = previousPosition - currentPosition;

        float xRot = difference.y * 180;
        float yRot = -difference.x * 180;

        cam.transform.Rotate(new Vector3(1, 0, 0), xRot);
        cam.transform.Rotate(new Vector3(0, 1, 0), yRot, Space.World);

        cam.transform.Translate(new Vector3(0, 0, -distanceFrom));
        previousPosition = currentPosition;*/
    }

    void mouseBoundsCheck()
    {
        if(Input.mousePosition.x >= 0)
        {
        }
    }
}
