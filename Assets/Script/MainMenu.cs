using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject mapPanel;

    public void Play()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Quitting the application.");
    }

    public void ShowMap()
    {
        mainMenuPanel.SetActive(false);
        mapPanel.SetActive(true);
    }

    public void BackFromMap()
    {
        mapPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
