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
    public bool isDoorClosed = false;

    [Header("Kapı Kontrolü")]
    public Transform doorTransform;
    public float doorClosedThreshold = 5f;

    [Header("Uyuma Engel Mesajları")]
    public TextMeshProUGUI lightMessageText;
    public string lightOnMessage = "I need the turn off the light";
    public string doorOpenMessage = "I need to close the door";
    public float messageDisplayTime = 3f;
    private bool isShowingMessage = false;
    private MissionObjective missionObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        promptMessage = "E - Sleep";
        missionObj = GetComponent<MissionObjective>();
    }

    protected override void Update()
    {
        base.Update();

        if (doorTransform != null)
            isDoorClosed = Quaternion.Angle(doorTransform.localRotation, Quaternion.identity) < doorClosedThreshold;
    }

    protected override void Interact()
    {
        // Eğer bu etkileşimle ilişkili görev artık aktif değilse (yani tamamlanmışsa), hiçbir şey yapma.
        if (missionObj != null && MissionManager.Instance != null && MissionManager.Instance.CurrentMission != missionObj.requiredMission)
        {
            // Oyuncu yatağa tekrar tıklarsa etkileşim olmasın diye prompt'u kaldırıyoruz.
            promptMessage = "";
            return;
        }

        if (!isDoorClosed)
        {
            if (!isShowingMessage)
                StartCoroutine(ShowBlockMessage(doorOpenMessage));
            return;
        }

        if (!isLightsOff)
        {
            if (!isShowingMessage)
                StartCoroutine(ShowBlockMessage(lightOnMessage));
            return;
        }

        //Uyuyup uyanma cutscene'i başlar
        StartCoroutine(PlayClockFade());

        //Sonraki görevi tetikler
        if (missionObj != null)
        {
            // Varsa, görev sistemini tetikle!
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

    private IEnumerator ShowBlockMessage(string message)
    {
        isShowingMessage = true;

        if (lightMessageText != null)
        {
            lightMessageText.text = message;
            lightMessageText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageDisplayTime);

        if (lightMessageText != null)
            lightMessageText.gameObject.SetActive(false);

        isShowingMessage = false;
    }
}
