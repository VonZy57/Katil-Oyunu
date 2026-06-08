using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("Paneller")]
    public GameObject mainPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public Image fadeImage; // Siyah ekran için UI Image
    public TextMeshProUGUI gameNameText; // Oyun adının yazdığı TextMeshProUGUI

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
        // Sahne başladığında ekranın siyahtan normale dönmesi (Fade In/Out)
        if (fadeImage != null)
        {
            // Başlangıçta tamamen siyah yap ve aktif et
            Color startColor = fadeImage.color;
            startColor.a = 1f;
            fadeImage.color = startColor;
            fadeImage.gameObject.SetActive(true);

            // DOTween kullanarak belirtilen süre kadar bekledikten sonra alpha değerini 0'a indir (Saydamlaştır)
            fadeImage.DOFade(0f, 3f).SetDelay(1f).OnComplete(() =>
            {
                fadeImage.gameObject.SetActive(false); // İşlem bitince objeyi kapat
                fadeImage.color = startColor; // Bir sonraki kullanım için alpha'yı tekrar 1 yap
            });

        }
        ShowPanel(mainPanel);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    void OnPlayButton()     => StartGameLoadSequence();
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

    void StartGameLoadSequence()
    {   
        gameNameText.DOFade(0f, 1f);
        settingsButton.gameObject.GetComponent<Image>().DOFade(0f, 1f);
        creditsButton.gameObject.GetComponent<Image>().DOFade(0f, 1f);
        quitButton.gameObject.GetComponent<Image>().DOFade(0f, 1f);
        playButton.gameObject.GetComponent<Image>().DOFade(0f, 1f).OnComplete(() =>
        {
            Camera.main.DOFieldOfView(1f, 10f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                SceneManager.LoadScene(gameSceneName);
                Cursor.lockState = CursorLockMode.Locked;
            });
        });
    }
}
