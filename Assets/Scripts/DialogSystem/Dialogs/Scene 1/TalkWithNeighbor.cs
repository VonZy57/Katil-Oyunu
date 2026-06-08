using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Splines;

public class TalkWithNeighbor : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private TextMeshProUGUI subtitleText;

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;

    [Header("Komşu Sesleri")]
    [SerializeField] private AudioClip clip_neighbor1; // Selamun aleyküm
    [SerializeField] private AudioClip clip_neighbor2; // Keksizlikten yanıyorum

    [Header("Anne Sesleri")]
    [SerializeField] private AudioClip clip_mother1; // Gene mi geldin
    [SerializeField] private AudioClip clip_mother2; // Tamam hadi bir bardak daha

    [System.Serializable]
    public struct SubtitleLine
    {
        public string speakerName;
        public LocalizedText lineText;
        public float displayDuration;
        public AudioClip voiceClip; // YENİ — null ise ses çalınmaz
    }

    private List<SubtitleLine> dialogLines;

    [Header("Diyalog Sonrası Animasyon Ayarları")]
    public Transform neighborTransform; // Komşu objesi
    public Transform motherTransform;   // Anne objesi
    public Transform doorTransform;     // Kapı objesi

    public SplineContainer neighborSplineRoute; // Komşunun gideceği Spline rotası
    public SplineContainer motherSplineRoute;   // Annenin gideceği Spline rotası

    public Animator neighborAnimator;           // Komşunun animatörü (Opsiyonel)
    public Animator motherAnimator;             // Annenin animatörü (Opsiyonel)
    public string walkAnimParam = "isWalking";  // Yürüme animasyon parametresi

    public float walkSpeed = 2f;             // Karakterlerin yürüme hızı
    public float doorCloseZAngle = 0f;       // Kapının kapanacağı Z açısı (Örn: 0)
    public float doorCloseDuration = 1f;     // Kapının kapanma süresi

    public bool isFinished { get; private set; } = false;

    void Start()
    {
        BuildDialog();
    }

    void BuildDialog()
    {
        dialogLines = new List<SubtitleLine>
        {
            new SubtitleLine
            {
                speakerName = "Neighbor",
                lineText = new LocalizedText { english = "Hi, neighbor! Could I ask if you have a cup of sugar?", turkish = "Selamun aleyküm, komşum! Bir fincan şekere ihtiyacım var, verebilir misin?" },
                displayDuration = clip_neighbor1 != null ? clip_neighbor1.length : 4f,
                voiceClip = clip_neighbor1
            },
            new SubtitleLine
            {
                speakerName = "Mother",
                lineText = new LocalizedText { english = "Why did you come again, infidel? How many times is it this week? Don't you know how bad too much sugar is for you? You idiot!", turkish = "Gene mi geldin imansız. Kaçıncı oldu bu hafta. Fazla şeker ne kadar zararlı biliyor musun? Deyyus!" },
                displayDuration = clip_mother1 != null ? clip_mother1.length : 6f,
                voiceClip = clip_mother1
            },
            new SubtitleLine
            {
                speakerName = "Neighbor",
                lineText = new LocalizedText { english = "I'm dying of cakelessness, honey. I can't live without cookies or that 'shaky' pudding.", turkish = "Keksizlikten yanıyorum güzelim. Kurabiye olmadan yaşayamam. Ya da o 'titrek' sütlaç." },
                displayDuration = clip_neighbor2 != null ? clip_neighbor2.length : 5f,
                voiceClip = clip_neighbor2
            },
            new SubtitleLine
            {
                speakerName = "Mother",
                lineText = new LocalizedText { english = "Okay, let's go get it one more time. But if you touch the raw cake, I'll **** on your bed again.", turkish = "Tamam hadi bir bardak daha olsun. Ama elin pişmemiş keke giderse gene sıçarım yatağına." },
                displayDuration = clip_mother2 != null ? clip_mother2.length : 5f,
                voiceClip = clip_mother2
            }
        };
    }

    public void StartNeighborDialog()
    {
        StartCoroutine(PlaySubtitleDialog());
    }

    private IEnumerator PlaySubtitleDialog()
    {
        isFinished = false;
        
        foreach (SubtitleLine line in dialogLines)
        {
            bool isTurkish = dialogSystem.GetCurrentLanguage();
            // Dil Türkçeyse isimleri de çeviriyoruz.
            string speaker = isTurkish ? (line.speakerName == "Mother" ? "Anne" : "Komşu") : line.speakerName;

            subtitleText.text = speaker + ": " + line.lineText.GetText(isTurkish);

            // Konuşmacının AudioSource'unu bul ve sesi çal
            if (line.voiceClip != null && speakerAudios != null)
            {
                SpeakerAudio sa = speakerAudios.Find(s => s.speakerName == line.speakerName);
                if (sa != null && sa.audioSource != null)
                {
                    sa.audioSource.spatialBlend = sa.isPlayer ? 0f : 1f;
                    sa.audioSource.clip = line.voiceClip;
                    sa.audioSource.Play();
                }
            }

            yield return new WaitForSeconds(line.displayDuration);
        }
        
        subtitleText.text = "";
        
        // Diyalog altyazıları bittikten sonra hareketleri ve kapı kapanmasını başlat
        yield return StartCoroutine(MoveAndCloseDoorSequence());

    }

    private IEnumerator MoveAndCloseDoorSequence()
    {
        // 1. Önce komşu hareket eder
        if (neighborTransform != null && neighborSplineRoute != null)
        {
            yield return StartCoroutine(MoveAlongSpline(neighborTransform, neighborSplineRoute, neighborAnimator));
        }

        // 2. Sonra anne hareket eder
        if (motherTransform != null && motherSplineRoute != null)
        {
            yield return StartCoroutine(MoveAlongSpline(motherTransform, motherSplineRoute, motherAnimator));
        }

        // 3. Kapı kapanır
        if (doorTransform != null)
        {
            Vector3 closeRot = new Vector3(doorTransform.localEulerAngles.x, doorTransform.localEulerAngles.y, doorCloseZAngle);
            yield return doorTransform.DOLocalRotate(closeRot, doorCloseDuration).SetEase(Ease.InOutQuad).WaitForCompletion();
        }

        // 4. Nesneler kapatılır
        if (neighborTransform != null) neighborTransform.gameObject.SetActive(false);
        if (motherTransform != null) motherTransform.gameObject.SetActive(false);

        isFinished = true;
    }

    private IEnumerator MoveAlongSpline(Transform actorTransform, SplineContainer splineRoute, Animator animator = null)
    {
        if (animator != null)
        {
            animator.SetBool(walkAnimParam, true);
        }

        SplineAnimate splineAnimate = actorTransform.GetComponent<SplineAnimate>();
        if (splineAnimate == null)
        {
            splineAnimate = actorTransform.gameObject.AddComponent<SplineAnimate>();
        }
        
        splineAnimate.enabled = true; // Bileşen daha önce kapatılmış olabileceği için aktif hale getir

        splineAnimate.Container = splineRoute;
        splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        splineAnimate.MaxSpeed = walkSpeed;
        splineAnimate.Loop = SplineAnimate.LoopMode.Once;
        splineAnimate.Alignment = SplineAnimate.AlignmentMode.SplineElement; // Yüzünü gittiği yöne dön

        splineAnimate.Restart(true);
        yield return new WaitUntil(() => !splineAnimate.IsPlaying);
        
        splineAnimate.enabled = false;

        if (animator != null)
        {
            animator.SetBool(walkAnimParam, false);
        }
    }
}
