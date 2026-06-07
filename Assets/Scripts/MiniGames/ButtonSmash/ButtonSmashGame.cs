using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem; // Yeni Input System kütüphanesi
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ButtonSmashGame : MonoBehaviour
{
    [Header("UI Ayarları")]
    public Slider smashBar;          // Ekranda görünecek bar
    public GameObject gamePanel;     // Barın olduğu UI panel (açıp kapatmak için)
    public Image eyeImage;           // Transparency için kullanılan siyah resim/panel
    public GameObject objectToActivate;   // Oyun başladığında açılacak obje
    public GameObject objectToDeactivate; // Oyun başladığında kapanacak obje
    public GameObject kuruObject;
    public GameObject girlObject;

    [Header("Mesaj Ayarları")]
    public TextMeshProUGUI messageText; // Mesaj gösterme için TextMeshPro
    public string message = "";         // Gösterilecek mesaj
    public float messageDelay = 0f;     // Mesajın görünme gecikmesi (saniye)

    [Header("Oyun Ayarları")]
    public float maxBarValue = 100f;
    public float increaseAmount = 5f;   // Tuşa basınca artış miktarı
    public float decayRate = 10f;       // Saniyedeki azalma hızı
    public float transparencyLevel = 0f;

    private float currentBarValue;
    private bool isMiniGameActive = false;
    private bool hasFirstSpacePressed = false; // İlk kez tuşa basıldı mı?

    [Header("Post-Processing Ayarları")]
    public Volume globalVolume; // Sahnedeki Global Volume buraya atanacak
    private Vignette vignette;  // Kod ile kontrol edeceğimiz efekt

    [Header("Vignette (Göz Kararması) Ayarları")]
    [Range(0f, 1f)] public float minIntensity = 0.5f; // Bar doluyken (Gözler açık)
    [Range(0f, 1f)] public float maxIntensity = 1.0f; // Bar boşken (Gözler kapalı/kısık)
    public float vignetteSpeed = 5f; // Kararma/Açılma yumuşaklığı

    [Header("Ses Ayarları")]
    public AudioSource phoneLoopAudioSource; // Minigame sırasında çalacak hazır AudioSource (Telefon üzerindeki)
    public AudioClip phonePickupSound; // Minigame bitince çalacak telefon açma sesi
    private AudioSource oneShotAudioSource; // Tek seferlik sesler için

    [Header("Işık Ayarları")]
    public LightSwitchInteractable lightSwitch; // Oyun bitince açılacak ışık anahtarı

    [Header("Kapı Ayarları")]
    public DoorInteraction roomDoor; // Oyun bitince kapanacak normal kapı (varsa)
    public MotelRoomDoorInteraction motelRoomDoor; // Oyun bitince kapanacak motel kapısı (varsa)

    // --- YENİ INPUT SYSTEM DEĞİŞKENİ ---
    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        // DİKKAT: Burada 'Jump' action'ını dinliyorum (genelde Space tuşudur).
        // Eğer senin Input Actions listesinde "Smash" veya "Interact" diye geçiyorsa
        // burayı 'controls.Player.Smash.performed' şeklinde değiştirmelisin!
        controls.Player.SpaceButton.performed += OnSmashButtonPressed;
    }

    private void OnDisable()
    {
        controls.Disable();
        controls.Player.SpaceButton.performed -= OnSmashButtonPressed; // Memory leak'i önle
    }

    private void Start()
    {
        // Başlangıçta paneli gizle
        if (gamePanel != null)
            gamePanel.SetActive(false);

        // Mesajı gizle
        if (messageText != null)
            messageText.gameObject.SetActive(false);

        // Tek seferlik sesler için ikinci AudioSource
        oneShotAudioSource = gameObject.AddComponent<AudioSource>();
        oneShotAudioSource.loop = false;
        oneShotAudioSource.playOnAwake = false;

        // Volume profilinden Vignette efektini bulmaya çalış
        if (globalVolume != null && globalVolume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0.0f;
            transparencyLevel = 0.0f;
        }
        else
        {
            Debug.LogWarning("Global Volume atanmadı veya profilde Vignette efekti yok!");
        }
    }

    private void Update()
    {
        if (!isMiniGameActive) return;

        // --- GÖRSEL EFEKTLER ---
        if (vignette != null)
        {
            //transparencyLevel = vignette.intensity.value; //Eksi hesaplama

            float currentVignette = vignette.intensity.value;

            // Mathf.InverseLerp: Eğer currentVignette 0.75 ve altındaysa 0 döndürür.
            // 1.0 ve üzerindeyse 1 döndürür.
            // 0.75 ile 1.0 arasındaysa (örneğin 0.875) orantılı olarak 0 ile 1 arasında bir değer (0.5) döndürür!
            transparencyLevel = Mathf.InverseLerp(0.75f, 1.0f, currentVignette);


            if (eyeImage != null)
            {
                Color tempEyeColor = eyeImage.color;
                tempEyeColor.a = transparencyLevel;
                eyeImage.color = tempEyeColor;
            }

            // --- VIGNETTE KONTROLÜ ---
            float barRatio = currentBarValue / maxBarValue;
            float targetIntensity = Mathf.Lerp(maxIntensity, minIntensity, barRatio);
            vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, Time.deltaTime * vignetteSpeed);
        }

        // --- BAR AZALTMA İŞLEMİ (Decay) ---
        currentBarValue -= decayRate * Time.deltaTime;
        currentBarValue = Mathf.Clamp(currentBarValue, 0f, maxBarValue);

        // UI Güncelleme
        if (smashBar != null)
            smashBar.value = currentBarValue;
    }

    // --- YENİ INPUT SİSTEMİ İLE TUŞA BASMA (SMASH) İŞLEMİ ---
    private void OnSmashButtonPressed(InputAction.CallbackContext context)
    {
        // Sadece minigame aktifken tuş basımlarını say
        if (!isMiniGameActive) return;

        currentBarValue += increaseAmount;

        // İlk kez tuşa basıldığında mesajı gizle
        if (!hasFirstSpacePressed)
        {
            hasFirstSpacePressed = true;
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
        }
    }

    public void StartMiniGame()
    {
        isMiniGameActive = true;

        if (kuruObject != null) kuruObject.SetActive(false);
        if (girlObject != null) girlObject.SetActive(false);

        hasFirstSpacePressed = false;
        currentBarValue = 25f;

        if (gamePanel != null)
            gamePanel.SetActive(true);

        // Mesajı gecikmeli göster
        if (messageText != null && !string.IsNullOrEmpty(message))
        {
            StartCoroutine(ShowMessageWithDelay());
        }

        // Telefonu değiştir
        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (objectToDeactivate != null)
            objectToDeactivate.SetActive(false);

        // Oyuna başladığında vignette'i mevcut bar değerine göre ayarla
        if (vignette != null)
            vignette.intensity.value = maxIntensity; // Karanlık başla

        // Ses çalmaya başla
        if (phoneLoopAudioSource != null)
        {
            phoneLoopAudioSource.Play();
        }

        Debug.Log("Oyun Başladı!");
    }

    private IEnumerator ShowMessageWithDelay()
    {
        if (messageDelay > 0)
        {
            yield return new WaitForSeconds(messageDelay);
        }

        if (messageText != null)
        {
            messageText.text = message;
            messageText.gameObject.SetActive(true);
        }
    }

    public void EndGame()
    {
        isMiniGameActive = false;

        if (gamePanel != null)
            gamePanel.SetActive(false);

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        // Oyun bitince Vignette'i anlık değerinden yavaşça 0'a (normale) DOTween ile döndür
        if (vignette != null)
        {
            DOTween.To(() => vignette.intensity.value, x => vignette.intensity.value = x, 0f, 1.5f).SetEase(Ease.InOutSine);
        }

        // Loop sesi durdur
        if (phoneLoopAudioSource != null && phoneLoopAudioSource.isPlaying)
        {
            phoneLoopAudioSource.Stop();
        }

        // Gözleri (Siyah paneli) DOTween ile yavaşça tamamen şeffaf yap
        if (eyeImage != null)
        {
            eyeImage.DOFade(0f, 1.5f).SetEase(Ease.InOutSine);
        }

        // Oyun bittiğinde ışıkları aç
        if (lightSwitch != null)
        {
            lightSwitch.TurnOnLights();
        }

        // Oyun bittiğinde kapıyı kapat
        if (roomDoor != null)
        {
            roomDoor.CloseDoor();
        }
        if (motelRoomDoor != null)
        {
            motelRoomDoor.CloseDoor();
        }

        Debug.Log("Oyun Bitti! Skor (Bar Doluluğu): " + currentBarValue);
    }
}