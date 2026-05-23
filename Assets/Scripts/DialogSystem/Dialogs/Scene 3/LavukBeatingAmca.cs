using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LavukBeatingAmca : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private MissionObjective missionObj;
    [SerializeField] private EnginTalksWithCrowded enginTalksWithCrowded;

    [System.Serializable]
    public struct SubtitleLine
    {
        public string speakerName;
        public LocalizedText lineText;
        public float displayDuration;
    }

    [Header("Diyalog Satırları")]
    [SerializeField] private List<SubtitleLine> dialogLines;

    public bool isFinished { get; private set; } = false;
    private bool hasTriggered = false;

    void Start()
    {
        BuildDialog();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && MissionManager.Instance.CurrentMission == missionObj.requiredMission && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlaySubtitleSequence());
        }
    }

    private IEnumerator PlaySubtitleSequence()
    {
        isFinished = false;

        // Konuşma başladığında peş peşe oynayacak animasyonlar
        StartCoroutine(InitialAnimationsPlaceholder());

        for (int i = 0; i < dialogLines.Count; i++)
        {
            // Lavuk son lafını söylediğinde ayrı bir animasyon sekansı başlat
            if (i == dialogLines.Count - 1)
            {
                StartCoroutine(FinalAnimationSequencePlaceholder());
            }

            SubtitleLine line = dialogLines[i];
            bool isTurkish = dialogSystem.GetCurrentLanguage();
            string speaker = line.speakerName;
            
            subtitleText.text = speaker + ": " + line.lineText.GetText(isTurkish);
            yield return new WaitForSeconds(line.displayDuration);
        }
        
        subtitleText.text = "";
        isFinished = true;
    }

    private IEnumerator InitialAnimationsPlaceholder()
    {
        // Peş peşe gelecek animasyonlar için placeholder
        Debug.Log("Diyalog başladı! Animasyon 1 (örn: Lavuk vurur)");
        yield return new WaitForSeconds(1.5f); // 1. Animasyon süresi
        
        Debug.Log("Animasyon 2 (örn: Amca yere düşer)");
        yield return new WaitForSeconds(1.5f); // 2. Animasyon süresi

        Debug.Log("Animasyon 3 (örn: Lavuk tepesinde dikilir)");
        yield return null;
    }

    private IEnumerator FinalAnimationSequencePlaceholder()
    {
        // Lavuğun son cümlesinde ayrı oynayacak animasyon sekansı
        Debug.Log("Lavuğun son cümlesi! Ayrı animasyon sekansı başlıyor (örn: Lavuk son bir tekme atıp arkasını döner)");
        yield return new WaitForSeconds(2f);
        
        Debug.Log("Lavuk sahneden uzaklaşır...");
        
        // Altyazıların tamamen bitmesini bekliyoruz ki diyalog paneliyle üst üste binmesin
        yield return new WaitUntil(() => isFinished);

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

    void BuildDialog()
    {
        dialogLines = new List<SubtitleLine>
        {
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "Didn't I tell you I never want to see you around here again, you **** *****! Huh, if I **** your liver right now, would it be enough, huh!", turkish = "Sana bir daha seni burada görmeyeceğim demedim mi **** *****. He şimdi senin ciğerini ****** az mı yapmış olurum l*n. *******." },
                displayDuration = 5f
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "You geezerrr, didn't I tell you to leave this woman alone, huuuuuh! Why are you roaming around here like a sleazebag, you ****.", turkish = "Moruuukk sana bu kadının peşini bırak demedim mi haaaa!? Ne gevşek gevşek dolanıyorsun buralarda *****." },
                displayDuration = 4.5f
            },
            new SubtitleLine {
                speakerName = "Kuru",
                lineText = new LocalizedText { english = "Just walk away, man, he's gonna die in your hands. Alright, you're the toughest around, now just hit the road.", turkish = "Çek git oğlum, elinde kalacak adam. Tamam en kral sensin bak yoluna." },
                displayDuration = 4f
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "What do you mean hit the road, man! I'll **** your neighborhood and **** you too! It's easy to talk from up there. Come on, come here!", turkish = "Ne bakayım lan yoluma! Olum sizin mahallenizi de ***** sizi de ******. Oradan konuşmak kolay. Gelsene lan buraya." },
                displayDuration = 5.5f
            },
            new SubtitleLine {
                speakerName = "Kuru",
                lineText = new LocalizedText { english = "Don't push your luck, boy!", turkish = "Oğlum kaşınma bak!" },
                displayDuration = 2f
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "Are you hitting on my wife, my property, my bread and butter, you ***! Don't I have the right to beat the **** out of you right in front of this neighborhood, huh!", turkish = "Sen benim karıma, sen benim malıma, ekmek tekneme laf mı atıyon ***. Bu mahallelinin önünde seni çatır çutur **** hakkım değil mi lan!" },
                displayDuration = 6f
            },
            new SubtitleLine {
                speakerName = "Amca",
                lineText = new LocalizedText { english = "Please, let me speak to her just once. I missed her so much. I love her. She is my everything.", turkish = "Ne olur bir kez konuşayım onunla onu çok özledim. Onu seviyorum. O benim her şeyim" },
                displayDuration = 4.5f
            },
            new SubtitleLine {
                speakerName = "Lavuk",
                lineText = new LocalizedText { english = "Get the hell out of here, man. Look, I'm swearing in front of everyone. I swear to God on everything holy, if I see you around Asuman again, I'll choke the life out of you. If I don't, I'm the biggest *** ** *****.", turkish = "Yürrrü git lan. Bak herkesin önünde yemin ediyorum. Ekmek musap Kuran çarpsın, seni bir daha Asuman’ın etrafında görürsem senin ümüğünü sıkarım. Yapmazsam en adi ****** *******." },
                displayDuration = 7f
            }
        };
    }
}
