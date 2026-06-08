using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class CarInteractable : Interactable
{
    [SerializeField] private MissionObjective missionObj;
    [SerializeField] private MissionManager missionManager;

    [Header("Geçiş Ayarları")]
    public Image fadeImage; // Ekranı kaplayan siyah UI resmi
    public float fadeDuration = 2f; // Kararma süresi
    private bool isTransitioning = false; // Geçişin birden fazla kez tetiklenmesini engellemek için
    public TextMeshProUGUI thanksText; // Demonun bitmesiyle gösterilecek teşekkür mesajı

    [Header("Ses Referansları")]
    public AudioSource carAudioSource; // Araba motor sesi için AudioSource

    void Start()
    {
        missionObj = GetComponent<MissionObjective>();
        carAudioSource = GetComponent<AudioSource>();
    }

    protected override void Update()
    {
        if (missionObj != null && missionObj.requiredMission == missionManager.CurrentMission)
        {
            promptMessage = "E - Get in";
        }
        else
        {
            promptMessage = "";
        }
    }

    protected override void Interact()
    {
        if(missionObj != null && missionManager.CurrentMission == missionObj.requiredMission && !isTransitioning)
        {
            Debug.Log("Car interacted with!");
            GetInTheCar();
        }

    }

    void GetInTheCar()
    {
        isTransitioning = true; // Geçiş başladı olarak işaretle
        FirstPersonController playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonController>();
        playerController.enabled = false; // Oyuncu kontrolünü devre dışı bırak

            if (fadeImage != null)
            {
                // Siyah resmi aktif et ve saydamlığını (alpha) 0'dan 1'e doğru artır
                fadeImage.gameObject.SetActive(true);
                Color color = fadeImage.color;
                color.a = 0f;
                fadeImage.color = color;

                fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
                {
                    if (carAudioSource != null) carAudioSource.Play();

                    if (thanksText != null)
                    {
                        // Başlangıçta görünmez yap
                        Color textColor = thanksText.color;
                        textColor.a = 0f;
                        thanksText.color = textColor;
                        thanksText.gameObject.SetActive(true);

                        // 5 saniye bekle, sonra 3 saniyede fade in yap
                        thanksText.DOFade(1f, 3f).SetDelay(5f).OnComplete(() =>
                        {
                            // Fade in bittikten 10 saniye sonra oyunu kapat
                            DOVirtual.DelayedCall(5f, () =>
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#else
                                Application.Quit();
#endif
                            });
                        });
                    }
                });
            }
            else
            {
                // Eğer fadeImage atanmamışsa direkt çıkış yap
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
    }
}
