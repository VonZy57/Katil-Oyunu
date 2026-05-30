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

    void Start()
    {
        missionObj = GetComponent<MissionObjective>();
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
        Debug.Log("Car interacted with!");
        GetInTheCar();
    }

    void GetInTheCar()
    {
        isTransitioning = true; // Geçiş başladı olarak işaretle

            if (fadeImage != null)
            {
                // Siyah resmi aktif et ve saydamlığını (alpha) 0'dan 1'e doğru artır
                fadeImage.gameObject.SetActive(true);
                Color color = fadeImage.color;
                color.a = 0f;
                fadeImage.color = color;

                fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
                {
                    SceneManager.LoadScene("DemoEndScene");
                });
            }
            else
            {
                // Eğer fadeImage atanmamışsa direkt geçiş yap (hata almamak için)
                SceneManager.LoadScene("DemoEndScene");
            }
    }
}
