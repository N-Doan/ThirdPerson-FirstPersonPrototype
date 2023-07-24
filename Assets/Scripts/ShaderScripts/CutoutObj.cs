using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutoutObj : MonoBehaviour
{
    [SerializeField]
    private Transform targetObject;

    [SerializeField]
    private LayerMask wallMask;

    [SerializeField]
    private Camera thirdPersonCamera;

    [Header("Cutout Details")]
    [SerializeField]
    private float cutoutSize = 0.5f;
    [SerializeField]
    private float falloffSize = 0.3f;
    private List<GameObject> currentBlockers;

    private void Start()
    {
        currentBlockers = new List<GameObject>();
    }
    private void FixedUpdate()
    {
        Vector2 cutoutPos = thirdPersonCamera.WorldToViewportPoint(targetObject.position);

        cutoutPos.y = cutoutPos.y / (Screen.width / Screen.height);

        Vector3 offset = targetObject.position - transform.position;
        RaycastHit[] hitObj = Physics.RaycastAll(transform.position, offset, offset.magnitude, wallMask);

        for(int i = 0; i < hitObj.Length; i++)
        {
            if(hitObj[i].transform.GetComponent<Renderer>() != null)
            {
                Material[] materials = hitObj[i].transform.GetComponent<Renderer>().materials;

                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j].SetVector("_CutoutPosition", cutoutPos);
                    materials[j].SetFloat("_CutoutSize", cutoutSize);
                    materials[j].SetFloat("_FalloffSize", falloffSize);
                }
                currentBlockers.Add(hitObj[i].transform.gameObject);
            }
        }

        if(hitObj.Length == 0)
        {
            for(int i = 0; i < currentBlockers.Count; i++)
            {
                Material wallMat = currentBlockers[i].GetComponent<Renderer>().material;
                wallMat.SetVector("_CutoutPosition", cutoutPos);
                wallMat.SetFloat("_CutoutSize", 0.0f);
                wallMat.SetFloat("_FalloffSize", 0.0f);
            }
            currentBlockers.Clear();
        }

    }

    public void clearCutoutList()
    {
        if (currentBlockers.Count > 0)
        {
            for (int i = 0; i < currentBlockers.Count; i++)
            {
                Material wallMat = currentBlockers[i].GetComponent<Renderer>().material;
                //wallMat.SetVector("_CutoutPosition", cutoutPos);
                wallMat.SetFloat("_CutoutSize", 0.0f);
                wallMat.SetFloat("_FalloffSize", 0.0f);
            }
            currentBlockers.Clear();
        }
    }
}
