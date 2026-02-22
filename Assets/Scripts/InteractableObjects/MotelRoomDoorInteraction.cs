using UnityEngine;
using System.Collections;
using TMPro;
using DG.Tweening;

public class MotelRoomDoorInteraction : Interactable
{
    [Header("Kapı Ayarları")]
    public float openAngle = 110f;          // Açılma açısı
    public float doorDuration = 1f;         // Açılma/Kapanma süresi (DOTween için)

    private bool isOpen = false;            // Kapı açık mı?
                                            // isMoving DEĞİŞKENİNİ TAMAMEN SİLDİK

    private Quaternion closedRotation;      // Kapalı pozisyon
    private Quaternion openRotation;        // Açık pozisyon

    public RoomKeyInteraction haveKey;

    [Header("Anahtar Mesajı")]
    public TextMeshProUGUI noKeyMessageText;
    public string noKeyMessage = "I need the room key from the lobby";
    public float messageDisplayTime = 3f;
    private bool isShowingMessage = false;

    [Header("Ses Efektleri")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    private AudioSource audioSource;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        promptMessage = "E - Kapıyı Aç";

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    protected override void Interact()
    {
        // Anahtar yoksa uyarı mesajı göster
        if (!haveKey.isKeyCollected)
        {
            if (!isShowingMessage)
            {
                StartCoroutine(ShowNoKeyMessage());
            }
            return;
        }

        // ARTIK BURADA "if (isMoving) return;" KONTROLÜ YOK!
        // Oyuncu istediği an tekrar E'ye basabilir.

        isOpen = !isOpen; // Durumu değiştir

        // Prompt mesajını güncelle
        promptMessage = isOpen ? "E - Kapıyı Kapat" : "E - Kapıyı Aç";

        Debug.Log(isOpen ? "Kapı Açılıyor" : "Kapı Kapanıyor");

        // Ses efektini çal
        PlayDoorSound();

        // Hedef rotasyonu belirle
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        // --- GÜNCELLENMİŞ DOTWEEN KODU ---
        transform.DOKill(); // Eğer kapı şu an hareket halindeyse o hareketi anında iptal et!

        // Yeni hedefe doğru yola çık
        transform.DORotateQuaternion(targetRotation, doorDuration)
                 .SetEase(Ease.InOutQuad);
        // OnComplete kısmını da sildik, artık ihtiyacımız yok.

        // Görevler burada tetiklenir
        MissionObjective missionObj = GetComponent<MissionObjective>();
        if (missionObj != null)
        {
            missionObj.OnInteracted();
        }
    }

    private IEnumerator ShowNoKeyMessage()
    {
        isShowingMessage = true;

        if (noKeyMessageText != null)
        {
            noKeyMessageText.text = noKeyMessage;
            noKeyMessageText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageDisplayTime);

        if (noKeyMessageText != null)
        {
            noKeyMessageText.gameObject.SetActive(false);
        }

        isShowingMessage = false;
    }

    private void PlayDoorSound()
    {
        if (audioSource != null)
        {
            AudioClip clipToPlay = isOpen ? doorOpenSound : doorCloseSound;
            if (clipToPlay != null)
            {
                audioSource.PlayOneShot(clipToPlay);
            }
        }
    }
}