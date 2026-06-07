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
        public AudioClip voiceClip;
    }

    [Header("Diyalog Satırları")]
    [SerializeField] private List<SubtitleLine> dialogLines;

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;
    [Header("Amca Sesleri")]
    [SerializeField] private AudioClip clip_amca_order;
    [Header("Kamil Efendi Sesleri")]
    [SerializeField] private AudioClip clip_kamil_hmm;

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
                displayDuration = 4f,
                voiceClip = clip_amca_order
            },
            new SubtitleLine
            {
                speakerName = "Kamil Efendi",
                lineText = new LocalizedText { english = "Hmmmm...", turkish = "Hmmmm..." },
                displayDuration = 2f,
                voiceClip = clip_kamil_hmm
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
            yield return new WaitForSeconds(line.displayDuration);
        }
        
        subtitleText.text = "";
        isFinished = true;
    }
}