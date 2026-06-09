using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;

public class LavukBeatingAmca : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private MissionObjective missionObj;
    [SerializeField] private EnginTalksWithCrowded enginTalksWithCrowded;
    [SerializeField] private LavukMovement lavukMovement;

    [Header("Animasyon Referansları")]
    [SerializeField] private Animator lavukAnimator;

    [Header("Oyuncu Spline Hareketi")]
    [SerializeField] private SplineContainer playerPathSpline;
    [SerializeField] private Transform playerLookAtTarget;
    [SerializeField] private Transform amcaLookAtTarget;
    [SerializeField] private Transform kuruLookAtTarget; // Kuru'nun yerini veya bakılacak yüzünü belirten hedef
    [SerializeField] private float playerWalkSpeed = 3f;

    [System.Serializable]
    public struct SubtitleLine
    {
        public string speakerName;
        public LocalizedText lineText;
        public float displayDuration;
        public AudioClip voiceClip;
    }

    [Header("Diyalog Satırları")]
    [SerializeField] private List<SubtitleLine> dialogLines;

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;
    [Header("Lavuk Sesleri")]
    [SerializeField] private AudioClip clip_lavuk_1;
    [SerializeField] private AudioClip clip_lavuk_2;
    [SerializeField] private AudioClip clip_lavuk_3;
    [SerializeField] private AudioClip clip_lavuk_4;
    [SerializeField] private AudioClip clip_lavuk_5;
    [Header("Kuru Sesleri")]
    [SerializeField] private AudioClip clip_kuru_1;
    [SerializeField] private AudioClip clip_kuru_2;
    [Header("Amca Sesleri")]
    [SerializeField] private AudioClip clip_amca_1;

    public bool isFinished { get; private set; } = false;
    private bool hasTriggered = false;
    private GameObject currentPlayer;
    private Quaternion lavukOriginalRotation;

    void Start()
    {
        BuildDialog();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && MissionManager.Instance.CurrentMission == missionObj.requiredMission && !hasTriggered)
        {
            hasTriggered = true;
            currentPlayer = other.gameObject;
            StartCoroutine(PlayerWalkAndWatchSequence(other.gameObject));
            StartCoroutine(PlaySubtitleSequence());
        }
    }

    private IEnumerator PlayerWalkAndWatchSequence(GameObject player)
    {
        FirstPersonController fps = player.GetComponent<FirstPersonController>();
        CharacterController cc = player.GetComponent<CharacterController>();

        // Oyuncu kontrollerini ve collider'ı (spline'da takılmaması için) geçici olarak kapat
        if (fps != null) fps.enabled = false;
        if (cc != null) cc.enabled = false;

        // Spline takibine başlamadan önce 2 saniye bekle
        yield return new WaitForSeconds(1f);

        if (playerPathSpline != null)
        {
            SplineAnimate splineAnimate = player.GetComponent<SplineAnimate>();
            if (splineAnimate == null) splineAnimate = player.AddComponent<SplineAnimate>();

            splineAnimate.Container = playerPathSpline;
            splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
            splineAnimate.MaxSpeed = playerWalkSpeed;
            splineAnimate.Loop = SplineAnimate.LoopMode.Once;
            splineAnimate.Alignment = SplineAnimate.AlignmentMode.SplineElement; // Yüzünü spline yönüne dön

            splineAnimate.enabled = true;
            splineAnimate.Restart(true);

            // Spline rotası bitene kadar bekle
            yield return new WaitUntil(() => !splineAnimate.IsPlaying);
            splineAnimate.enabled = false;
        }

        // Spline rotası bittiğinde, karakteri ve kamerayı kavganın olduğu yere (hedefe) yumuşakça döndür
        if (playerLookAtTarget != null)
        {
            player.transform.DOKill();
            player.transform.DOLookAt(playerLookAtTarget.position, 2f, AxisConstraint.Y).SetEase(Ease.InOutSine);

            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOKill();
                cam.transform.DOLookAt(playerLookAtTarget.position, 2f).SetEase(Ease.InOutSine);
            }
        }

        // Spline işlemi bitince sadece CC'yi geri açıyoruz. (Yere basması ve diğer scriptlerin çarpışmaları okuyabilmesi için)
        // FPC (kamera/hareket kilitleri) ise EnginTalksWithCrowded scriptindeki diyalog bittiğinde otomatik açılacak!
        if (cc != null) cc.enabled = true;
    }

    private IEnumerator PlaySubtitleSequence()
    {
        isFinished = false;
        subtitleText.gameObject.SetActive(true);


        // Lavuk'un orijinal rotasyonunu kaydet, böylece sonra eski haline dönebilsin.
        if (lavukAnimator != null)
            lavukOriginalRotation = lavukAnimator.transform.rotation;

        for (int i = 0; i < dialogLines.Count; i++)
        {
            // Lavuk son lafını söylediğinde ayrı bir animasyon sekansı başlat
            if (i == dialogLines.Count - 1)
            {
                if (lavukAnimator != null)
                    lavukAnimator.SetTrigger("PunchTrigger");
                StartCoroutine(FinalAnimationSequencePlaceholder());
            }

            // 3. indeks: Lavuk'un Kuru'ya cevap vermeye başladığı an
            if (i == 3)
            {
                if (lavukAnimator != null) lavukAnimator.SetTrigger("PointTrigger");
                StartCoroutine(LavukLookAtKuruRoutine());
            }
            // 4. indeks: Kuru'nun Lavuk'a "Kaşınma bak!" dediği an (Eski rotasyona dön)
            else if (i == 4)
            {
                if (lavukAnimator != null)
                {
                    lavukAnimator.transform.DOKill();
                    lavukAnimator.transform.DORotateQuaternion(lavukOriginalRotation, 0.5f);
                }
            }

            SubtitleLine line = dialogLines[i];
            bool isTurkish = dialogSystem.GetCurrentLanguage();
            string speaker = line.speakerName;

            if (speakerAudios != null && line.voiceClip != null)
            {
                SpeakerAudio speakerAudio = speakerAudios.Find(s => s.speakerName == speaker);
                if (speakerAudio != null && speakerAudio.audioSource != null)
                {
                    speakerAudio.audioSource.Stop();
                    speakerAudio.audioSource.clip = line.voiceClip;
                    speakerAudio.audioSource.Play();
                }
            }

            subtitleText.text = speaker + ": " + line.lineText.GetText(isTurkish);
            yield return new WaitForSeconds(line.voiceClip != null ? line.voiceClip.length : line.displayDuration);

            // Konuşması bittiğinde (3. indeksin sonu) EnoughTrigger tetiklensin
            if (i == 3)
            {
                if (lavukAnimator != null) lavukAnimator.SetTrigger("EnoughTrigger");
            }
        }
        
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
        isFinished = true;
    }

    private IEnumerator LavukLookAtKuruRoutine()
    {
        yield return new WaitForSeconds(2f);
        if (lavukAnimator != null && kuruLookAtTarget != null)
        {
            lavukAnimator.transform.DOKill();
            lavukAnimator.transform.DOLookAt(kuruLookAtTarget.position, 0.5f, AxisConstraint.Y).SetEase(Ease.InOutSine);
        }
    }

    private IEnumerator FinalAnimationSequencePlaceholder()
    {
        // Lavuğun son cümlesinde ayrı oynayacak animasyon sekansı
        Debug.Log("Lavuğun son cümlesi! EndFight animasyonu başlıyor.");
        yield return new WaitForSeconds(2f);

        // Altyazıların tamamen bitmesini bekliyoruz ki diyalog paneliyle üst üste binmesin
        yield return new WaitUntil(() => isFinished);

        if (lavukMovement != null)
        {
            lavukMovement.StartWalkingToCar();
        }
        
        // Lavuk sahneden uzaklaşırken boş kalan 10 saniyeyi kamera geçişleriyle değerlendiriyoruz
        if (currentPlayer != null)
        {
            // 3 saniye hareket halindeki Lavuk'u takip et
            yield return StartCoroutine(TrackTargetForDuration(playerLookAtTarget, 3f));
            // 2 saniye Amca'ya dön ve bak
            yield return StartCoroutine(TrackTargetForDuration(amcaLookAtTarget, 2f));
            // Tekrar 3 saniye Lavuk'u takip et
            yield return StartCoroutine(TrackTargetForDuration(playerLookAtTarget, 3f));
            // Tekrar 2 saniye Amca'ya bak
            yield return StartCoroutine(TrackTargetForDuration(amcaLookAtTarget, 1.5f));
        }
        else
        {
            yield return new WaitForSeconds(10f); // Her ihtimale karşı fallback
        }

        // 1. Diyalog: Görev son animasyon bitince değişsin
        if (missionObj != null)
        {
            missionObj.OnInteracted();
        }

        if (enginTalksWithCrowded != null)
        {
            enginTalksWithCrowded.StartCrowdDialog();
        }
    }

    private IEnumerator TrackTargetForDuration(Transform target, float duration)
    {
        if (target == null || currentPlayer == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        Camera cam = currentPlayer.GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main;

        // Daha önceden kalan DOTween animasyonları varsa durdur ki titreme/çakışma yapmasın
        currentPlayer.transform.DOKill();
        if (cam != null) cam.transform.DOKill();

        float timer = 0f;
        while (timer < duration)
        {
            if (currentPlayer != null && cam != null)
            {
                // Hedefe yönelen vektör
                Vector3 dirToTarget = (target.position - currentPlayer.transform.position).normalized;
                
                // Gövdeyi (Player) sadece Y ekseninde (Sağa/Sola) yavaşça çevir
                Vector3 bodyDir = new Vector3(dirToTarget.x, 0, dirToTarget.z).normalized;
                if (bodyDir != Vector3.zero)
                    currentPlayer.transform.rotation = Quaternion.Slerp(currentPlayer.transform.rotation, Quaternion.LookRotation(bodyDir), Time.deltaTime * 3f);

                // Kamerayı hedefe tam yönlendir (Aşağı/Yukarı dâhil)
                if (dirToTarget != Vector3.zero)
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(dirToTarget), Time.deltaTime * 3f);
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void BuildDialog()
    {
        dialogLines = new List<SubtitleLine>
        {
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "Didn’t I tell you I never want to see you around here again, you **** *****! Huh, if I **** your liver right now, wouldn’t it be enough, wouuld it huh!", turkish = "Sana bir daha seni burada görmeyeceğim demedim mi **** *****. He şimdi senin ciğerini ****** az mı yapmış olurum l*n. *******." },
                displayDuration = 5f,
                voiceClip = clip_lavuk_1
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "You geezerrr, didn’t I tell you to leave this woman alone, huuuuuh! Why are you roaming around here like a sleazebag, you ****.", turkish = "Moruuukk sana bu kadının peşini bırak demedim mi haaaa!? Ne gevşek gevşek dolanıyorsun buralarda *****." },
                displayDuration = 4.5f,
                voiceClip = clip_lavuk_2
            },
            new SubtitleLine {
                speakerName = "Kuru",
                lineText = new LocalizedText { english = "Just walk away, man, he’s gonna die by your hands. Alright we get it, you’re the toughest around, now just hit the road.", turkish = "Çek git oğlum, elinde kalacak adam. Tamam en kral sensin bak yoluna." },
                displayDuration = 4f,
                voiceClip = clip_kuru_1
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "What do you mean hit the road, man! I’ll **** your neighborhood and **** you too! It’s easy to talk from up there. Come on, come here!", turkish = "Ne bakayım lan yoluma! Olum sizin mahallenizi de ***** sizi de ******. Oradan konuşmak kolay. Gelsene lan buraya." },
                displayDuration = 5.5f,
                voiceClip = clip_lavuk_3
            },
            new SubtitleLine {
                speakerName = "Kuru",
                lineText = new LocalizedText { english = "Don’t push your luck, boy!", turkish = "Oğlum kaşınma bak!" },
                displayDuration = 2f,
                voiceClip = clip_kuru_2
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "Are you hitting on my wife, my property, my bread and butter, you ***! Don’t I have the right to beat the **** out of you right in front of this neighborhood, huh!", turkish = "Sen benim karıma, sen benim malıma, ekmek tekneme laf mı atıyon ***. Bu mahallelinin önünde seni çatır çutur **** hakkım değil mi lan!" },
                displayDuration = 6f,
                voiceClip = clip_lavuk_4
            },
            new SubtitleLine {
                speakerName = "Amca",
                lineText = new LocalizedText { english = "Please, let me speak to her just once. I missed her so much. I love her. She is my everything.", turkish = "Ne olur bir kez konuşayım onunla onu çok özledim. Onu seviyorum. O benim her şeyim" },
                displayDuration = 4.5f,
                voiceClip = clip_amca_1
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "Get the hell out of here, man. Look, I’m swearing in front of everyone. I swear to God on everything holy, if I see you around Asuman again, I’ll choke the life out of you. If I don’t, I’m the biggest *** ** *****.", turkish = "Yürrrü git lan. Bak herkesin önünde yemin ediyorum. Ekmek musap Kuran çarpsın, seni bir daha Asuman’ın etrafında görürsem senin ümüğünü sıkarım. Yapmazsam en adi ****** *******." },
                displayDuration = 7f,
                voiceClip = clip_lavuk_5
            }
        };
    }
}
