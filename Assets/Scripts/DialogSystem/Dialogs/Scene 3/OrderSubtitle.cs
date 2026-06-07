using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OrderSubtitle : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private MissionObjective missionObj;

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

    void BuildDialog()
    {
        dialogLines = new List<SubtitleLine>
        {
            new SubtitleLine
            {
                speakerName = "Amca",
                lineText = new LocalizedText { english = "Kamil Efendi, get us two pastries each and some ayran. Make it bottled.", turkish = "Kamil Efendi, bize ikişer poğaça ve ayran. Ayran şişe olsun." },
                displayDuration = 4f
            },
            new SubtitleLine
            {
                speakerName = "Kamil Efendi",
                lineText = new LocalizedText { english = "Hmmmm...", turkish = "Hmmmm..." },
                displayDuration = 2f
            }
        };
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Amca") && MissionManager.Instance.CurrentMission == missionObj.requiredMission && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(PlaySubtitleDialog());
        }
    }

    private IEnumerator PlaySubtitleDialog()
    {
        isFinished = false;
        
        foreach (SubtitleLine line in dialogLines)
        {
            bool isTurkish = dialogSystem.GetCurrentLanguage();
            string speaker = line.speakerName;
            
            subtitleText.text = speaker + ": " + line.lineText.GetText(isTurkish);
            yield return new WaitForSeconds(line.displayDuration);
        }
        
        subtitleText.text = "";
        isFinished = true;
    }
}