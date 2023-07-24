using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField]
    private float timeLimit = 2.5f;
    [SerializeField]
    private float respawnTime = 5.0f;

    private Rigidbody rb;
    private Material meshMaterial;
    private Vector3 originalLocation;
    private Quaternion originalRotation;
    private bool triggered = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        meshMaterial = gameObject.GetComponent<MeshRenderer>().material;
        originalLocation = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        originalRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && triggered == false)
        {
            triggered = true;
            StartCoroutine(dropPlatform());
        }
    }

    private IEnumerator dropPlatform()
    {
        float elapsedTime = 0.0f;
        Color startingColor = meshMaterial.color;

        while(elapsedTime <= timeLimit)
        {
            elapsedTime += Time.deltaTime;

            meshMaterial.color = new Color(Mathf.Lerp(startingColor.r, 1, elapsedTime / timeLimit), Mathf.Lerp(startingColor.g, 0, elapsedTime / timeLimit), Mathf.Lerp(startingColor.b, 0, elapsedTime / timeLimit));
            yield return null;
        }
        disablePlatformScripts();
        rb.isKinematic = false;
        rb.useGravity = true;
        StartCoroutine(respawnCountdown(startingColor));
        StopCoroutine(dropPlatform());
    }

    private void disablePlatformScripts()
    {
        if (gameObject.GetComponent<SpinObject>())
        {
            gameObject.GetComponent<SpinObject>().enabled = false;
        }
        if (gameObject.GetComponent<MovingPlatform>())
        {
            gameObject.GetComponent<MovingPlatform>().enabled = false;
        }
    }
    private void enablePlatformScripts()
    {
        if (gameObject.GetComponent<SpinObject>())
        {
            gameObject.GetComponent<SpinObject>().enabled = true;
        }
        if (gameObject.GetComponent<MovingPlatform>())
        {
            gameObject.GetComponent<MovingPlatform>().enabled = true;
        }
    }

    private IEnumerator respawnCountdown(Color startingColor)
    {
        yield return new WaitForSeconds(respawnTime);
        meshMaterial.color = startingColor;
        rb.isKinematic = true;
        rb.useGravity = false;
        gameObject.transform.position = originalLocation;
        gameObject.transform.rotation = originalRotation;
        enablePlatformScripts();
        triggered = false;

    }
}
