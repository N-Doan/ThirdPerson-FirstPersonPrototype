using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyDebugUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI statusText;

    private void Start()
    {
        updateStatus(1.0f);
    }
    public void updateStatus(float damage)
    {
        statusText.text = "Next Launch Force: " + damage;
    }
}
