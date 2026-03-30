using DG.Tweening; // DOTween kütüphanesini dahil ettik
using UnityEngine;

public class DoorInteraction : Interactable
{
    [Header("Kapı Ayarları")]
    public float openAngle = 90f;          // Açılma açısı
    public float doorDuration = 1f;        // Açılma süresi (DOTween için)

    [Header("Ses Ayarları")]
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;            

    private bool isOpen = false;           // Kapı açık mı?
                                           // isMoving değişkenini tamamen sildik

    private Quaternion closedRotation;     // Kapalı pozisyon
    private Quaternion openRotation;       // Açık pozisyon
    private AudioSource audioSource;       // Ses çalmak için

    private void Start()
    {
        // Başlangıç rotasyonunu kaydet
        closedRotation = transform.rotation;

        // Açık rotasyonu hesapla (mevcut rotasyona göre relatif)
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);

        // Prompt mesajını ayarla
        promptMessage = "E - Kapıyı Aç";

        // AudioSource ekle
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    protected override void Interact()
    {
        // ARTIK BURADA "if (isMoving) return;" KONTROLÜ YOK!
        // Oyuncu istediği an kapıya müdahale edebilir.

        isOpen = !isOpen; // Durumu değiştir

        // Prompt mesajını güncelle
        promptMessage = isOpen ? "E - Kapıyı Kapat" : "E - Kapıyı Aç";

        Debug.Log(isOpen ? "Kapı Açılıyor" : "Kapı Kapanıyor");

        AudioClip clipToPlay = isOpen ? doorOpenSound : doorCloseSound;
        // Kapı sesi çal (açma ve kapama için aynı ses)
        if (audioSource != null && clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }

        // Hedef rotasyonu belirle
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;

        // --- DOTWEEN İLE KAPI HAREKETİ ---
        transform.DOKill(); // Eğer kapı şu an hareket halindeyse eski hareketi iptal et!

        // Yeni hedefe doğru yumuşak bir şekilde dön
        transform.DORotateQuaternion(targetRotation, doorDuration)
                 .SetEase(Ease.InOutQuad);

        // Görevler burada tetiklenir
        MissionObjective missionObj = GetComponent<MissionObjective>();
        if (missionObj != null)
        {
            // Varsa, görev sistemini tetikle!
            missionObj.OnInteracted();
        }
    }
}