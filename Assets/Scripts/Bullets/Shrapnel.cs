using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrapnel : MonoBehaviour
{
    [SerializeField]
    private GameObject shrapnelPrefab;
    [SerializeField]
    private int shrapnelCount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDestroy()
    {
        Vector3 shrapnelTravelDir;
        for(int i = 0; i < shrapnelCount; i++)
        {
            shrapnelTravelDir = new Vector3(Random.Range(-2.0f, 2.0f), Random.Range(0.0f, 2.0f), Random.Range(-2.0f, 2.0f));
            //shrapnelTravelDir = shrapnelTravelDir.normalized;asa
            GameObject piece = GameObject.Instantiate(shrapnelPrefab,gameObject.transform.position, Quaternion.identity);
            piece.GetComponent<ShrapnelPiece>().setTravelDirection(shrapnelTravelDir);
        }
    }
}
