using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MotherCooking : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode carryTheBodies;
    [SerializeField] private MissionObjective missionObj;
    public Transform motherTableTransform; // Anne'nin yemek masasında duracağı pozisyon
    public GameObject cookingPot; // Masadaki tencere modeli

    [SerializeField] private SettingsMenuController settingsMenuController;

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;

    [Header("Anne Dialog Sesleri")]
    [SerializeField] private AudioClip clip_carryTheBodies;
    [SerializeField] private AudioClip clip_carryTheBodies2;

    [Header("Şarkı")]
    public AudioClip songClip;
    private AudioSource audioSource;

    [Header("Animasyon Ayarları")]
    [SerializeField] private Transform motherTransform;
    [SerializeField] private Transform motherLookTarget; // Kameranın bakacağı nokta (kafa vb.)
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject playerBody;

    [Header("Şarkı Altyazıları")]
    public TMPro.TextMeshProUGUI subtitleText;
    private List<SongSubtitle> songSubtitles;

    [System.Serializable]
    public struct SongSubtitle
    {
        public float startTime;
        public float duration;
        public string speakerName;
        public LocalizedText lineText;
    }

    void OnEnable()
    {
        BathtubDropInteractable.OnLastBodyDropped += TeleportMotherToTable;
    }
    void OnDisable()
    {
        BathtubDropInteractable.OnLastBodyDropped -= TeleportMotherToTable;
    }

    void TeleportMotherToTable()
    {
        gameObject.transform.position = motherTableTransform.position;
        gameObject.transform.rotation = motherTableTransform.rotation;
        cookingPot.SetActive(true); // Tencereyi görünür yap
        this.enabled = false; // Teleport işlemi tamamlandıktan sonra bu scripti devre dışı bırakıyoruz, böylece oyuncu tekrar etkileşime giremez.
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        BuildDialogTree();
        BuildSongSubtitles();
        StartCoroutine(PlaySongAfterStartDialog());
    }

    IEnumerator PlaySongAfterStartDialog()
    {
        if (settingsMenuController != null) settingsMenuController.isLocked = true;

        // FirstPersonController'ın Start() metodunun çalışmasına izin vermek için 1 frame bekle.
        // Eğer hemen fps.enabled = false yaparsak, Start metodu ertelenir ve diyalog
        // başladığında (fps.enabled = true yapıldığında) çalışarak ekranı aniden siyah yapar!
        yield return null;

        // Şarkı başlarken (karanlıktayken) oyuncu hareket edemesin diye kontrolleri hemen kapatıyoruz
        FirstPersonController fps = playerBody.GetComponent<FirstPersonController>();
        if (fps != null)
        {
            fps.enabled = false;
            
            // FirstPersonController'da otomatik olarak başlayan Fade Out'u iptal edip ekranı siyah kilitliyoruz.
            if (fps.fadeImage != null)
            {
                fps.fadeImage.DOKill();
                Color c = fps.fadeImage.color;
                c.a = 1f;
                fps.fadeImage.color = c;
                fps.fadeImage.gameObject.SetActive(true); // Siyah ekranı garanti altına al
            }
        }

        if (songClip != null)
        {
            audioSource.PlayOneShot(songClip);
            if (subtitleText != null)
                StartCoroutine(PlaySongSubtitles());
            yield return new WaitForSeconds(songClip.length); // Şarkının süresine göre bekleme (örneğin 57 saniye)
        }

        // Şarkı bitti! Ekranı göz kırpma (Blinking) efektiyle yavaşça aydınlatıyoruz.
        if (fps != null && fps.fadeImage != null)
        {
            // Kırpıştırma başlamadan hemen önce kamerayı tavana (-65 derece) bakacak şekilde ayarla
            if (playerCamera != null)
            {
                playerCamera.transform.localEulerAngles = new Vector3(-65f, 0f, 0f);
                
                // Kırpıştırmanın toplam animasyon süresini hesaplıyoruz (0.3 + 0.15 + 0.4 + 0.15 = 1 saniye + fadeDuration)
                float totalBlinkDuration = 1f + fps.fadeDuration;
                
                // Kırpıştırma ile senkronize bir şekilde kafayı yavaşça öne (0 derece) eğ
                playerCamera.transform.DOLocalRotate(Vector3.zero, totalBlinkDuration).SetEase(Ease.InOutSine);

                // Aynı anda baş dönmesi hissi için sağa-sola (Y ve Z eksenlerinde) hafif ve yavaş sarsıntı ekle
                // Vibrato (3) düşük tutularak titreme değil, sarhoş gibi sallanma hissi verilir
                //playerCamera.transform.DOShakeRotation(totalBlinkDuration, new Vector3(0f, 8f, 4f), 3, 45f, true);
            }
            
            yield return new WaitForSeconds(0.5f); // Kırpıştırma başlamadan önce kısa bir bekleme (isteğe bağlı)

            Sequence blinkSequence = DOTween.Sequence();

            // 1. Kırpma: Gözler hafifçe açılır ve hızla kapanır
            blinkSequence.Append(fps.fadeImage.DOFade(0.4f, 0.3f).SetEase(Ease.InOutSine));
            blinkSequence.Append(fps.fadeImage.DOFade(1f, 0.15f).SetEase(Ease.InOutSine));

            // 2. Kırpma: Gözler biraz daha fazla açılır ve kapanır
            blinkSequence.Append(fps.fadeImage.DOFade(0.1f, 0.4f).SetEase(Ease.InOutSine));
            blinkSequence.Append(fps.fadeImage.DOFade(1f, 0.15f).SetEase(Ease.InOutSine));

            // Son açılış: Gözler tamamen ve yumuşakça açılır
            blinkSequence.Append(fps.fadeImage.DOFade(0f, fps.fadeDuration).SetEase(Ease.InOutSine));

            blinkSequence.OnComplete(() =>
            {
                fps.fadeImage.gameObject.SetActive(false);
            });
            
            yield return blinkSequence.WaitForCompletion();
        }
        
        yield return new WaitForSeconds(1f); // Ekran açıldıktan sonra kısa bir süre bekle

        // GÖVDEYİ (playerBody) DÖNDÜRMÜYORUZ! (Gövde dönerse koltuğun yönü/0 noktası bozulur).
        // Sadece kamerayı anneye çeviriyoruz.
        Transform lookTarget = motherLookTarget != null ? motherLookTarget : motherTransform;
        playerCamera.transform.DOLookAt(lookTarget.position, 1f).OnComplete(() =>
        {
            if (fps != null) { fps.SyncCameraRotation(); fps.enabled = true; } // Yeni açıyı sisteme kaydet ve kontrolleri geri ver
            if (settingsMenuController != null) settingsMenuController.isLocked = false;
            dialogSystem.SetSpeakers(speakerAudios);
            dialogSystem.StartDialog(carryTheBodies); // Diyalog başlar.
            StartCoroutine(WaitForDialogEnd()); // Diyalogun bitmesini bekleyecek sistemi başlat
        });
    }

    private IEnumerator PlaySongSubtitles()
    {
        if (subtitleText == null) yield break;
        subtitleText.gameObject.SetActive(true);

        bool isTurkish = dialogSystem != null ? dialogSystem.GetCurrentLanguage() : true;
        float timer = 0f;

        foreach (var sub in songSubtitles)
        {
            while (timer < sub.startTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            string speaker = sub.speakerName;
            if (isTurkish)
            {
                if (speaker == "Man") speaker = "Adam";
                else if (speaker == "Woman") speaker = "Kadın";
                else if (speaker == "Mother") speaker = "Anne";
            }

            subtitleText.text = speaker + ": " + sub.lineText.GetText(isTurkish);

            while (timer < sub.startTime + sub.duration)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }
    }

    private IEnumerator WaitForDialogEnd()
    {
        // Diyalog panelinin önce açılmasını garanti olarak bekle (erken tetiklenmeyi önlemek için)
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
        
        // Diyalog paneli kapanana kadar (EndDialog çağrılana kadar) döngüyü beklet
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);
        
        // Diyalog kapandığında görevi ilerlet
        if (missionObj != null)
            missionObj.OnInteracted();
    }

    void BuildDialogTree()
    {
        carryTheBodies = DialogBuilder.CreateNode
        ("We're gonna eat, you son of a *****. Don't just sit there all curled up!",
        "Yemek yicez soysuzun çocuğu. Ne diye büzüşüp oturuyon!",
        "Mother");

        DialogNode carryTheBodies2 = DialogBuilder.CreateEndNode
        ("Stand up, do something useful you lazy piece of ****. Carry the bodies to the bathroom. We can't eat like this. My dear son, just like his father. Come on, help your poor mother.",
        "Kalk da bir boka yara. Cesetleri sırtlan da banyoya koy. Midemiz kaldırmaz ellam. Babası kılıklı canım oğluşum benim. Hadi garip anana bir yardım et.",
        "Mother");

        DialogOption silentOption = DialogBuilder.CreateOption("...", "...", carryTheBodies2, true);
        DialogBuilder.AddOption(carryTheBodies, silentOption);

        carryTheBodies.voiceClip  = clip_carryTheBodies;
        carryTheBodies2.voiceClip = clip_carryTheBodies2;
    }

    void BuildSongSubtitles()
    {
        songSubtitles = new List<SongSubtitle>
        {
            new SongSubtitle
            {
                startTime = 34f,
                duration = 2f,
                speakerName = "Man",
                lineText = new LocalizedText { english = "Stop, stop! Don't do it!", turkish = "Dur, dur yapma!" }
            },
            new SongSubtitle
            {
                startTime = 37f,
                duration = 4f,
                speakerName = "Woman",
                lineText = new LocalizedText { english = "Stop, don't do it, stop! We were going to help you!!", turkish = "Dur, yapma dur! Biz sana yardım edecektik!!" }
            },
            new SongSubtitle
            {
                startTime = 45f,
                duration = 3.5f,
                speakerName = "Engin",
                lineText = new LocalizedText { english = "I am sorry for everything.", turkish = "Her şey için özür dilerim." }
            },
            new SongSubtitle
            {
                startTime = 49f,
                duration = 6f,
                speakerName = "Mother",
                lineText = new LocalizedText { english = "You are a good person Engin. My good boy. You are a good person. Open your eyes. Wake up!", turkish = "Sen iyi birisin Engin. Uslu oğluşum benim. Sen iyi birisin. Aç gözlerini. Uyan!" }
            }
        };
    }
}
