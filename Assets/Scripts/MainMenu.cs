using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject mainMenuPanel, settingsMenuPanel;
    public void OnStartBtn()
    {
        SceneManager.LoadScene("Level");
    }
    public void OnSettingsBtn()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }
    public void OnQuitBtn()
    {
        Application.Quit();
    }
    public void OnBackBtn()
    {
        if(settingsMenuPanel.activeInHierarchy)
        {
            settingsMenuPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
        }
    }
}
