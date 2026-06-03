using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class CarInteractable : Interactable
{
    [SerializeField] private MissionObjective missionObj;
    [SerializeField] private MissionManager missionManager;

    [Header("Geçiş Ayarları")]
    public Image fadeImage; // Ekranı kaplayan siyah UI resmi
    public float fadeDuration = 2f; // Kararma süresi
    private bool isTransitioning = false; // Geçişin birden fazla kez tetiklenmesini engellemek için

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
                    carAudioSource.Play();
                    float audioLength = carAudioSource.clip != null ? carAudioSource.clip.length : 2f; // Ses yoksa varsayılan 2 saniye bekle
                    DOVirtual.DelayedCall(audioLength, () =>
                    {
                        SceneManager.LoadScene("DemoEndScene");
                    });
                });
            }
            else
            {
                // Eğer fadeImage atanmamışsa direkt geçiş yap (hata almamak için)
                SceneManager.LoadScene("DemoEndScene");
            }
    }
}
