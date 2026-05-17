using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SceneChangerTester : MonoBehaviour
{
    [Header("Geçiş Ayarları")]
    public Image fadeImage; // Ekranı kaplayan siyah UI resmi
    public float fadeDuration = 2f; // Kararma süresi

    private bool isTransitioning = false; // Geçişin birden fazla kez tetiklenmesini engellemek için

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
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
                    SceneManager.LoadScene("EndlessRunnerTestScene");
                });
            }
            else
            {
                // Eğer fadeImage atanmamışsa direkt geçiş yap (hata almamak için)
                SceneManager.LoadScene("EndlessRunnerTestScene");
            }
        }
    }
}
