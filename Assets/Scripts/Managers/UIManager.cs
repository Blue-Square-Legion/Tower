using System.Collections;
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
    [SerializeField] GameObject creditsScreen1;
    [SerializeField] GameObject creditsScreen2;
    [SerializeField] GameObject titleScreen;

    [SerializeField] GameObject gameOverScreen;
    [SerializeField] GameObject deselect;
    [SerializeField] GameObject helpScreen;
    [SerializeField] GameObject upgradePanel;
    [SerializeField] GameObject upgradeCircle;
    [SerializeField] GameObject upgradeArrow;
    [SerializeField] GameObject popUpScreen;
    [SerializeField] TMP_Text popUpMessage;
    [Header("Tower Panel")]
    [SerializeField] RectTransform towersPanel;
    [SerializeField] RectTransform collapsePositionTowerPanelRectTransform;
    private Vector3 towerPanelExpandedPosition, towerPanelCollapsedPosition;

    [SerializeField] public GameObject runeSelection;
    [SerializeField] GameObject towerSelection;

    [SerializeField] private GameObject upgradeScreen;

    [Tooltip("Path 1")]
    [SerializeField] private TextMeshProUGUI path1Upgrade1;
    [SerializeField] private TextMeshProUGUI path1Upgrade2, path1Upgrade3;

    [Tooltip("Path 2")]
    [SerializeField] private TextMeshProUGUI path2Upgrade1;
    [SerializeField] private TextMeshProUGUI path2Upgrade2, path2Upgrade3;

    [Tooltip("Path 3")]
    [SerializeField] private TextMeshProUGUI path3Upgrade1;
    [SerializeField] private TextMeshProUGUI path3Upgrade2, path3Upgrade3;

    [SerializeField] TMP_Text autoStartText;
    [SerializeField] TMP_Text showPathsText;
    [SerializeField] GameObject autoStartButton;
    [SerializeField] GameObject showPathsButton;

    private float popUpDuration;
    private bool showTowers;
    private bool toggleTowersClickable;

    private void Start()
    {
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
        if (creditsScreen1 != null)
            creditsScreen1.SetActive(false);
        if (creditsScreen2 != null)
            creditsScreen2.SetActive(false);
        if (helpScreen != null)
            helpScreen.SetActive(false);
        if (popUpScreen != null)
            popUpScreen.SetActive(false);
        if (deselect != null)
            deselect.SetActive(false);
        popUpDuration = 0;
        showTowers = true;
        toggleTowersClickable = true;
        if (collapsePositionTowerPanelRectTransform != null)
        {
            towerPanelCollapsedPosition = collapsePositionTowerPanelRectTransform.anchoredPosition;
        }
        if (towersPanel != null)
        {
            towerPanelExpandedPosition = towersPanel.anchoredPosition;
        }
            
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

    public void StartGame(int level)
    {
        SceneManager.LoadScene(level);
    }

    public void CreditsScreen1Active(bool active)
    {
        creditsScreen1.SetActive(active);
        creditsScreen2.SetActive(!active);
        titleScreen.SetActive(!active);
    }

    public void CreditsScreen2Active(bool active)
    {
        creditsScreen1.SetActive(!active);
        creditsScreen2.SetActive(active);
        titleScreen.SetActive(!active);
    }

    public void CloseCreditScreen()
    {
        creditsScreen2.SetActive(false);
        creditsScreen1.SetActive(false);
        titleScreen.SetActive(true);
    }

    public void OpenHelpScreen()
    {
        if (helpScreen.activeInHierarchy) return;
        Time.timeScale = 0;
        helpScreen.SetActive(true);
        autoStartButton.SetActive(false);
        showPathsButton.SetActive(false);
        upgradeCircle.SetActive(upgradePanel.activeInHierarchy);
        upgradeArrow.SetActive(upgradePanel.activeInHierarchy);
        
    }

    public void CloseHelpScreen()
    {
        Time.timeScale = 1;
        helpScreen.SetActive(false);
        autoStartButton.SetActive(true);
        showPathsButton.SetActive(true);
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

        popUpDuration += 3;
        StartCoroutine(PopUpDisappear());
    }

    IEnumerator PopUpDisappear()
    {
        yield return new WaitForSeconds(popUpDuration);
        popUpScreen.SetActive(false);
        yield return null;
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

    public void UpdateUpgradeScreen(TowerBehavior towerType)
    {
        //Path 1
        path1Upgrade1.text = "Path 1 - Level 1\n" + towerType.GetUpgradeName(0, 1).Substring(0,
            towerType.GetUpgradeName(0, 1).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(0, 1);
        path1Upgrade2.text = "Path 1 - Level 2\n" + towerType.GetUpgradeName(1, 1).Substring(0, 
            towerType.GetUpgradeName(1, 1).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(1, 1);
        path1Upgrade3.text = "Path 1 - Level 3\n" + towerType.GetUpgradeName(2, 1).Substring(0,
            towerType.GetUpgradeName(2, 1).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(2, 1);

        //Path 2
        path2Upgrade1.text = "Path 2 - Level 1\n" + towerType.GetUpgradeName(0, 2).Substring(0,
            towerType.GetUpgradeName(0, 2).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(0, 2);
        path2Upgrade2.text = "Path 2 - Level 2\n" + towerType.GetUpgradeName(1, 2).Substring(0,
            towerType.GetUpgradeName(1, 2).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(1, 2);
        path2Upgrade3.text = "Path 2 - Level 3\n" + towerType.GetUpgradeName(2, 2).Substring(0,
            towerType.GetUpgradeName(2, 2).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(2, 2);

        //Path 3
        path3Upgrade1.text = "Path 3 - Level 1\n" + towerType.GetUpgradeName(0, 3).Substring(0,
            towerType.GetUpgradeName(0, 3).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(0, 3);
        path3Upgrade2.text = "Path 3 - Level 2\n" + towerType.GetUpgradeName(1, 3).Substring(0,
            towerType.GetUpgradeName(1, 3).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(1, 3);
        path3Upgrade3.text = "Path 3 - Level 3\n" + towerType.GetUpgradeName(2, 3).Substring(0,
            towerType.GetUpgradeName(2, 3).IndexOf("\n")) + "\n" + towerType.GetUpgradeDescription(2, 3);
    }

    public void ShowCompendium()
    {
        upgradeScreen.SetActive(true);
    }

    public void HideCompendium()
    {
        upgradeScreen.SetActive(false);
    }

    public void UpdateAutoStartText(string text)
    {
        autoStartText.text = text;
    }

    public void UpdateShowPathsText(string text)
    {
        showPathsText.text = text;
    }

    public void ToggleTowersPressed()
    {
        if (toggleTowersClickable)
        {
            toggleTowersClickable = false;
            showTowers = !showTowers;

            if (showTowers)
                StartCoroutine(ExpandTowers());
            else
                StartCoroutine(CollapseTowers());
        }
    }

    IEnumerator ExpandTowers()
    {
        while (Mathf.Abs(towersPanel.anchoredPosition.x - towerPanelExpandedPosition.x) >= 0.1)
        {
            towersPanel.anchoredPosition = new Vector2(Vector2.MoveTowards(towersPanel.anchoredPosition, towerPanelExpandedPosition, 20 * Time.deltaTime * 60).x, towersPanel.anchoredPosition.y);
            yield return new WaitForEndOfFrame();
        }
        toggleTowersClickable = true;
        yield return null;
    }

    IEnumerator CollapseTowers()
    {
        while (Mathf.Abs(towersPanel.anchoredPosition.x - towerPanelCollapsedPosition.x) >= 0.1)
        {
            towersPanel.anchoredPosition = new Vector2(Vector2.MoveTowards(towersPanel.anchoredPosition, towerPanelCollapsedPosition, 20 * Time.deltaTime * 60).x, towersPanel.anchoredPosition.y);
            yield return new WaitForEndOfFrame();
        }
        toggleTowersClickable = true;
        yield return null;
    }

    public void Quit()
    {
        Application.Quit();
    }
}