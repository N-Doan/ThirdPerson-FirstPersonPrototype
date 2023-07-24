using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastBullet : BaseBullet
{
    public LineRenderer lineRenderer;

    protected override void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, 0.0f);
        lineRenderer.endColor = new Color(lineRenderer.endColor.r, lineRenderer.endColor.g, lineRenderer.endColor.b, 0.0f);
    }

    public override void applyBulletEffects()
    {
        throw new System.NotImplementedException();
    }

    protected override void OnEnable()
    {
        return;
    }

    public override IEnumerator lifeTimeCountdown()
    {
        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }

    public void fire(Ray ray)
    {
        RaycastHit hit;
        if(Physics.SphereCast(ray, 0.05f, out hit, LayerMask.GetMask("Terrain", "JumpableWall", "Enemies", "Interactable")))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                if (hit.collider.GetComponentInParent<BaseEnemyCombatManager>())
                {
                    hit.collider.GetComponentInParent<BaseEnemyCombatManager>().EnemyHit(this, -ray.direction);
                }
            }
            else if (hit.collider.CompareTag("Interactable"))
            {
                if (hit.collider.GetComponent<BaseInteractable>())
                {
                    hit.collider.GetComponent<BaseInteractable>().onRaycastHit();
                }
            }
        }
        lineRenderer.positionCount = 2;
        //local position of ray origin
        lineRenderer.SetPosition(0, transform.InverseTransformPoint(ray.origin));
        //END position needs to have start position added, as the second point of linerenderer is rendered from 0,0,0 rather than relative to the first position!
        lineRenderer.SetPosition(1, ray.direction.normalized*100.0f + transform.InverseTransformPoint(ray.origin));
        StartCoroutine(fadeOutLine());
    }

    private IEnumerator fadeOutLine()
    {
        lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, 1.0f);
        lineRenderer.endColor = new Color(lineRenderer.endColor.r, lineRenderer.endColor.g, lineRenderer.endColor.b, 1.0f);

        float fadeRate = 5.7f;
        float elaplsedTime = 0.0f;
        while(elaplsedTime <= fadeRate)
        {
            float alphaLerp = Mathf.Lerp(1.0f, 0.0f, elaplsedTime / fadeRate);
            lineRenderer.startColor = new Color(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, alphaLerp);
            lineRenderer.endColor = new Color(lineRenderer.endColor.r, lineRenderer.endColor.g, lineRenderer.endColor.b, alphaLerp);
            elaplsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
}
