using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    #region Singleton
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(UIManager)) as UIManager;
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion
    //title screens
    [SerializeField] GameObject creditsScreen;
    [SerializeField] GameObject titleScreen;

    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject helpScreen;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
        if (creditsScreen != null)
            creditsScreen.SetActive(false);
        if (helpScreen != null)
            helpScreen.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    public void CreditsScreenActive(bool active)
    {
        creditsScreen.SetActive(active);
        titleScreen.SetActive(!active);
    }
    public void OpenHelpScreen()
    {
        if (helpScreen.activeInHierarchy) return;
        Time.timeScale = 0;
        helpScreen.SetActive(true);
    }

    public void CloseHelpScreen()
    {
        Time.timeScale = 1;
        helpScreen.SetActive(false);
    }

    public void ReturnToTitle()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }
}
