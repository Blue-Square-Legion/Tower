using TMPro;
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
    [SerializeField] GameObject deselect;
    [SerializeField] GameObject helpScreen;
    [SerializeField] GameObject popUpScreen;
    [SerializeField] TMP_Text popUpMessage;

    [SerializeField] public GameObject runeSelection;
    [SerializeField] GameObject towerSelection;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
        if (creditsScreen != null)
            creditsScreen.SetActive(false);
        if (helpScreen != null)
            helpScreen.SetActive(false);
        if (popUpScreen != null)
            popUpScreen.SetActive(false);
        if (deselect != null)
            deselect.SetActive(false);
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        gameOverScreen.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1;
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

    public void SendPopUp(string text)
    {
        popUpMessage.SetText(text);
        popUpScreen.SetActive(true);
    }

    public void ClosePopUp()
    {
        popUpScreen.SetActive(false);
    }

    public void ToggleDeselect(bool isActive)
    {
        deselect.SetActive(isActive);
        if (!isActive)
        {
            ToggleRuneSelection(true);
            ToggleTowerSelection(true);
        }
    }
    public void ToggleRuneSelection(bool isActive)
    {
        runeSelection.SetActive(isActive);
    }

    public void ToggleTowerSelection(bool isActive)
    {
        towerSelection.SetActive(isActive);
    }
}