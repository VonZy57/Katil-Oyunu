using System.Collections;
using TMPro; // TextMeshPro kullan�m� i�in
using UnityEngine;

public class MissionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private CanvasGroup canvasGroup; // Opakl�k kontrol� i�in (Fade in/out)

    private void Start()
    {
        // Manager'�n eventlerine abone ol
        MissionManager.Instance.OnMissionStart += UpdateMissionText;

        // Ba�lang��ta g�r�nmez yap
        canvasGroup.alpha = 0;
    }

    private void OnDestroy()
    {
        // Sahne de�i�irse aboneli�i iptal et (Hata almamak i�in)
        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.OnMissionStart -= UpdateMissionText;
        }
    }

    private void UpdateMissionText(MissionSO mission)
    {
        StartCoroutine(ShowNewObjective(mission.description));
    }

    // Yaz�y� animasyonlu g�sterme (Fade In -> Bekle -> Fade Out)
    private IEnumerator ShowNewObjective(string text)
    {
        // �nce yaz�y� de�i�tir
        missionText.text = text;

        // Fade In (G�r�n�r ol)
        float duration = 1f;
        float time = 0;

        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;

        // Fears to Fathom tarz�nda g�rev yaz�s� ekranda kal�r,
        // g�revi tamamlay�nca veya yenisi gelince efekt verilebilir.
        // �imdilik s�rekli g�r�n�r b�rak�yoruz.
    }
}