using UnityEngine;
using TMPro; // TextMeshPro kullanýmý için
using System.Collections;

public class MissionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private CanvasGroup canvasGroup; // Opaklýk kontrolü için (Fade in/out)

    private void Start()
    {
        // Manager'ýn eventlerine abone ol
        MissionManager.Instance.OnMissionStart += UpdateMissionText;

        // Baþlangýçta görünmez yap
        canvasGroup.alpha = 0;
    }

    private void OnDestroy()
    {
        // Sahne deðiþirse aboneliði iptal et (Hata almamak için)
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionStart -= UpdateMissionText;
        }
    }

    private void UpdateMissionText(MissionSO mission)
    {
        StartCoroutine(ShowNewObjective(mission.description));
    }

    // Yazýyý animasyonlu gösterme (Fade In -> Bekle -> Fade Out)
    private IEnumerator ShowNewObjective(string text)
    {
        // Önce yazýyý deðiþtir
        missionText.text = text;

        // Fade In (Görünür ol)
        float duration = 1f;
        float time = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // Fears to Fathom tarzýnda görev yazýsý ekranda kalýr,
        // görevi tamamlayýnca veya yenisi gelince efekt verilebilir.
        // Þimdilik sürekli görünür býrakýyoruz.
    }
}