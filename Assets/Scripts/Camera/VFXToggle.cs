using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXToggle : MonoBehaviour
{
    VisualEffect vfx;
    float originalRate;

    private void Start()
    {
        vfx = gameObject.GetComponent<VisualEffect>();
        originalRate = vfx.GetFloat("SpawnRate");
        vfx.SetFloat("SpawnRate", 0.0f);
        EventManager.instance.EOnPlayerDash += toggleFX;
    }

    public void toggleFX(int instanceID, bool toggle)
    {
        if (toggle)
        {
            vfx.SetFloat("SpawnRate", originalRate);
            return;
        }
        vfx.SetFloat("SpawnRate", 0.0f);
    }
}
