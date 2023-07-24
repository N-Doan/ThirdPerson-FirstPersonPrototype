using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    [SerializeField]
    private GameObject mainCam;
    [SerializeField]
    private GameObject scopeCam;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private LayerMask scopedMask;
    [SerializeField]
    private LayerMask unscopedMask;
    [SerializeField]
    private CutoutObj cutout;

    public bool scoped = false;

    int instanceID;

    private CinemachineVirtualCamera scopedVC;
    private CinemachineFreeLook unscopedVC;

    // Start is called before the first frame update
    void Start()
    {
        scopeCam.SetActive(false);
        instanceID = gameObject.GetInstanceID();
        scopedVC = scopeCam.GetComponent<CinemachineVirtualCamera>();
        unscopedVC = mainCam.GetComponent<CinemachineFreeLook>();
    }

    public void swapCamera()
    {
        if (mainCam.activeInHierarchy)
        {
            swapToScopeCam();
        }
        else if (scopeCam.activeInHierarchy)
        {
            swapToMainCam();
        }
        else
        {
            Debug.LogError("CAMERA ERROR!");
        }
    }

    private void swapToMainCam()
    {
        //set unscoped rotation to match scoped rotation
        float prevHoriz = scopedVC.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value;

        scopeCam.SetActive(false);
        mainCam.SetActive(true);
        EventManager.instance.OnFirstPersonDisabled(instanceID);
        scoped = false;
        cam.cullingMask = unscopedMask;
        cutout.enabled = true;

        unscopedVC.m_XAxis.Value = prevHoriz;
    }

    private void swapToScopeCam()
    {
        float prevHoriz = unscopedVC.m_XAxis.Value;
        float prevVert = unscopedVC.m_YAxis.Value;

        scopeCam.SetActive(true);
        mainCam.SetActive(false);

        //set scope rotation to match unscoped rotation
        scopedVC.GetCinemachineComponent<CinemachinePOV>().m_HorizontalAxis.Value = prevHoriz;
        scopedVC.GetCinemachineComponent<CinemachinePOV>().m_VerticalAxis.Value = prevVert;

        EventManager.instance.OnFirstPersonEnabled(instanceID);
        scoped = true;
        cam.cullingMask = scopedMask;
        cutout.clearCutoutList();
        cutout.enabled = false;
    }
}
