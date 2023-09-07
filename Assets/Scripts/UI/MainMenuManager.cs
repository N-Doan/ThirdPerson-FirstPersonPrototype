using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void onMechanicsDemo()
    {
        SceneManager.LoadScene(1);
    }

    public void onProcLevelDemo()
    {
        SceneManager.LoadScene(2);
    }

    public void onQuit()
    {
        Application.Quit();
    }
}
