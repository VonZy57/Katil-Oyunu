using System; // Action kullanabilmek için ekledik
using UnityEngine;
using UnityEngine.UI;

public class SkillCheckSystem : MonoBehaviour
{
    [Header("UI Referansları")]
    public Transform needleTransform;
    public Image successZoneImage;
    public GameObject skillCheckUI;

    [Header("Ayarlar")]
    public float timeToComplete = 1.5f;
    public float zoneSizeAmount = 0.15f;

    private float currentProgress = 0f;
    private float safeZoneStart = 0f;
    private float safeZoneEnd = 0f;
    private bool isActive = false;

    // Sonucu BodyDragController'a iletmek için kullanacağımız köprü
    private Action<bool> onCheckComplete;

    void Start()
    {
        skillCheckUI.SetActive(false);
    }

    void Update()
    {
        if (!isActive) return;

        currentProgress += (1f / timeToComplete) * Time.deltaTime;
        needleTransform.localRotation = Quaternion.Euler(0, 0, -currentProgress * 360f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckResult();
        }
        else if (currentProgress > safeZoneEnd + 0.05f)
        {
            EndSkillCheck(false); // Süre doldu, kaçırdı
        }
    }

    // Artık bu fonksiyonu çağırırken bir Action (Geri çağırma) isteyeceğiz
    public void StartSkillCheck(Action<bool> callback)
    {
        onCheckComplete = callback;
        skillCheckUI.SetActive(true);
        isActive = true;
        currentProgress = 0f;

        successZoneImage.fillAmount = zoneSizeAmount;
        safeZoneStart = UnityEngine.Random.Range(0.2f, 1f - zoneSizeAmount);
        safeZoneEnd = safeZoneStart + zoneSizeAmount;
        successZoneImage.transform.localRotation = Quaternion.Euler(0, 0, -safeZoneStart * 360f);
    }

    private void CheckResult()
    {
        bool success = (currentProgress >= safeZoneStart && currentProgress <= safeZoneEnd);
        EndSkillCheck(success);
    }

    private void EndSkillCheck(bool success)
    {
        isActive = false;
        skillCheckUI.SetActive(false); // Arayüzü gizle

        // Sonucu kendisini çağıran scripte (BodyDragController) gönder
        onCheckComplete?.Invoke(success);
    }

    // Hedefe ulaşıldığında sistemi zorla durdurmak için
    public void ForceStop()
    {
        isActive = false;
        skillCheckUI.SetActive(false);
    }
}