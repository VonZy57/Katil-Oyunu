using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class BedInteraction : Interactable
{
    public ButtonSmashGame miniGameScript;

    public Image clockImage;
    public TextMeshProUGUI clockTextMeshPro;

    public float fadeInDuration = 1.0f;
    public float fadeOutDuration = 2.0f;
    public float waitDuration = 3.0f;

    public bool isLightsOff = false;

    [Header("Işık Açık Mesajı")]
    public TextMeshProUGUI lightMessageText; // TextMeshPro referansı
    public string lightOnMessage = "I need the turn off the light";
    public float messageDisplayTime = 3f;  // Mesaj ne kadar süre görünsün
    private bool isShowingMessage = false; // Mesaj gösteriliyor mu?

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        promptMessage = "E - Sleep";
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override void Interact()
    {

        if (!isLightsOff) // I��k kapal� de�ilse engelle.
        {
            if (!isShowingMessage)
            {
                StartCoroutine(ShowLightOnMessage());
            }
            return;
        }

        //Uyuyup uyanma cutscene'i ba�lar
        StartCoroutine(PlayClockFade());

        //Sonraki g�revi tetikler
        MissionObjective missionObj = GetComponent<MissionObjective>();
        if (missionObj != null)
        {
            // Varsa, g�rev sistemini tetikle!
            missionObj.OnInteracted();
        }
    }

    IEnumerator PlayClockFade()
    {
        clockImage.gameObject.SetActive(true);
        if (clockTextMeshPro != null) clockTextMeshPro.gameObject.SetActive(false);

        promptMessage = null;

        // --- FADE IN (G�r�n�r Olma) ---
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            SetAlpha(currentAlpha);
            yield return null; // Bir sonraki kareye bekle
        }

        // Saat yazısını ekranda göster
        if (clockTextMeshPro != null)
            clockTextMeshPro.gameObject.SetActive(true);

        // --- BEKLEME (3 Saniye) ---
        yield return new WaitForSeconds(waitDuration/2);

        //Button smash minigamei ba�lat�r.
        if (miniGameScript != null)
        {
            miniGameScript.StartMiniGame();
        }

        yield return new WaitForSeconds(waitDuration);

        if (clockTextMeshPro != null)
            clockTextMeshPro.gameObject.SetActive(false);

        // --- FADE OUT (Kaybolma) ---
        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            SetAlpha(currentAlpha);
            yield return null;
        }

        SetAlpha(0); // Kesinlikle tam g�r�nmez oldu�undan emin ol

        clockImage.gameObject.SetActive(false); // Imageı kapat
        SetAlpha(1); //Tekrar siyah hale getir.
    }

    // Hem Image hem Text'in �effafl���n� ayn� anda de�i�tiren yard�mc� fonksiyon
    void SetAlpha(float alpha)
    {
        if (clockImage != null)
        {
            Color imgColor = clockImage.color;
            imgColor.a = alpha;
            clockImage.color = imgColor;
        }

        if (clockTextMeshPro != null)
        {
            Color txtColor = clockTextMeshPro.color;
            txtColor.a = alpha;
            clockTextMeshPro.color = txtColor;
        }
    }

    private IEnumerator ShowLightOnMessage()
    {
        isShowingMessage = true;

        // TextMeshPro'ya mesaj� yaz
        if (lightMessageText != null)
        {
            lightMessageText.text = lightOnMessage;
            lightMessageText.gameObject.SetActive(true);
        }

        // Belirtilen s�re kadar bekle
        yield return new WaitForSeconds(messageDisplayTime);

        // TextMeshPro'yu gizle
        if (lightMessageText != null)
        {
            lightMessageText.gameObject.SetActive(false);
        }

        isShowingMessage = false;
    }
}
