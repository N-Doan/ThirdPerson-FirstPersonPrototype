using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * UI Manager controlling menu and UI Element visibility and checking for application quit
 */

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Image scope;

    [SerializeField]
    private Text playerDamagedText;

    [SerializeField]
    private GameObject ControlsToggle;
    int instanceID;

    // Start is called before the first frame update
    void Start()
    {
        instanceID = gameObject.GetInstanceID();
        scope.gameObject.SetActive(false);

        EventManager.instance.EOnFirstPersonEnabled += firstPersonEnabled;
        EventManager.instance.EOnFirstPersonDisabled += firstPersonDisabled;
        EventManager.instance.EOnPlayerDamageTaken += playerDamaged;
        Cursor.lockState = CursorLockMode.Locked;
        playerDamaged(instanceID, 0.0f);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ControlsToggle.SetActive(!ControlsToggle.activeInHierarchy);
        }
    }

    private void firstPersonEnabled(int id)
    {
            scope.gameObject.SetActive(true);
    }
    private void firstPersonDisabled(int id)
    {
            scope.gameObject.SetActive(false);
    }
    private void playerDamaged(int id, float damageVal)
    {
        playerDamagedText.text = "Damage: " + damageVal.ToString();
        if(damageVal > 0)
        {
            playerDamagedText.color = new Color(damageVal/10, 0, 0);
        }
    }
}
