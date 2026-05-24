using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Ana Menü Butonları")]
    public Button playButton;
    public Button settingsButton;
    public Button creditsButton;
    public Button quitButton;

    [Header("Geri Butonları")]
    public Button settingsBackButton;
    public Button creditsBackButton;

    [Header("Credits Video")]
    public VideoPlayer creditsVideoPlayer;

    [Header("Oyun Sahnesi")]
    public string gameSceneName = "SampleScene";

    void Awake()
    {
        if (playButton != null)         playButton.onClick.AddListener(OnPlayButton);
        if (settingsButton != null)     settingsButton.onClick.AddListener(OnSettingsButton);
        if (creditsButton != null)      creditsButton.onClick.AddListener(OnCreditsButton);
        if (quitButton != null)         quitButton.onClick.AddListener(OnQuitButton);
        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(OnBackButton);
        if (creditsBackButton != null)  creditsBackButton.onClick.AddListener(OnBackButton);

        if (creditsVideoPlayer != null)
            creditsVideoPlayer.loopPointReached += _ => ShowPanel(mainPanel);
    }

    void Start()
    {
        ShowPanel(mainPanel);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    void OnPlayButton()     => SceneManager.LoadScene(gameSceneName);
    void OnSettingsButton() => ShowPanel(settingsPanel);
    void OnBackButton()     => ShowPanel(mainPanel);

    void OnCreditsButton()
    {
        ShowPanel(creditsPanel);
        if (creditsVideoPlayer != null)
        {
            creditsVideoPlayer.Stop();
            creditsVideoPlayer.Play();
        }
    }

    void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ShowPanel(GameObject target)
    {
        if (target == mainPanel && creditsVideoPlayer != null)
            creditsVideoPlayer.Stop();

        if (mainPanel != null)     mainPanel.SetActive(mainPanel == target);
        if (settingsPanel != null) settingsPanel.SetActive(settingsPanel == target);
        if (creditsPanel != null)  creditsPanel.SetActive(creditsPanel == target);
    }
}
