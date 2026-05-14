using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TalkWithNeighbor : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private TextMeshProUGUI subtitleText;

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
                displayDuration = 4f
            },
            new SubtitleLine
            {
                speakerName = "Mother",
                lineText = new LocalizedText { english = "Why did you come again, infidel? How many times is it this week? Don't you know how bad too much sugar is for you? You idiot!", turkish = "Gene mi geldin imansız. Kaçıncı oldu bu hafta. Fazla şeker ne kadar zararlı biliyor musun? Deyyus!" },
                displayDuration = 6f
            },
            new SubtitleLine
            {
                speakerName = "Neighbor",
                lineText = new LocalizedText { english = "I'm dying of cakelessness, honey. I can't live without cookies or that 'shaky' pudding.", turkish = "Keksizlikten yanıyorum güzelim. Kurabiye olmadan yaşayamam. Ya da o 'titrek' sütlaç." },
                displayDuration = 5f
            },
            new SubtitleLine
            {
                speakerName = "Mother",
                lineText = new LocalizedText { english = "Okay, let's go get it one more. But if you touch raw cake, I'll shit on your bed again.", turkish = "Tamam hadi bir bardak daha olsun. Ama elin pişmemiş keke giderse gene sıçarım yatağına." },
                displayDuration = 5f
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
            yield return new WaitForSeconds(line.displayDuration);
        }
        
        subtitleText.text = "";
        isFinished = true;
    }
}
