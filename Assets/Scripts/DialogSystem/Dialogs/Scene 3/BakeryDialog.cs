using UnityEngine;
using DG.Tweening;
using System.Collections;

public class BakeryDialog : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode bakeryStartNode;
    [SerializeField] private MissionObjective missionObj;

    [Header("Kamera ve Bakış Referansları")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform amcaTransform;

    private FirstPersonController playerFPS;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (playerCamera != null)
        {
            playerFPS = playerCamera.GetComponentInParent<FirstPersonController>();
        }
        BuildDialog();
    }

    public void StartBakeryDialog()
    {
        if (dialogSystem != null && bakeryStartNode != null)
        {
            if (playerFPS != null) playerFPS.enabled = false;

            if (playerCamera != null && amcaTransform != null)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (cam == null) cam = playerCamera.GetComponentInChildren<Camera>();
                if (cam == null) cam = Camera.main; // Fallback

                Transform camTransform = cam != null ? cam.transform : playerCamera.transform;
                camTransform.DOKill();
                camTransform.DOLookAt(amcaTransform.position, 1f).SetEase(Ease.InOutSine); // Kamerayı Amca'ya çevir
            }

            StartCoroutine(DialogSequence());
        }
    }

    private IEnumerator DialogSequence()
    {
        dialogSystem.StartDialog(bakeryStartNode);

        yield return null;
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);

        // Diyalog tamamen bittiğinde oyuncu kontrollerini olduğu açıda geri ver
        if (playerFPS != null)
        {
            playerFPS.SyncCameraRotation(); // Eski açıya dönmesini engelle
            playerFPS.enabled = true; 
        }

        if (missionObj != null)
        {
            missionObj.OnInteracted();
        }
    }

    void BuildDialog()
    {
        // ==========================================
        // BAŞLANGIÇ
        // ==========================================
        bakeryStartNode = DialogBuilder.CreateNode(
            "Ughhh...",
            "Offff...",
            "Amca"
        );

        // ==========================================
        // 1. SEÇİM DALLLARI: Are you okay / Who was that?
        // ==========================================
        DialogNode enginAreYouOkay = DialogBuilder.CreateNode(
            "You look pretty banged up, old man. Are you okay?",
            "Amca epeyce hırpalanmış gibisin. İyi misin? ",
            "Engin"
        );
        DialogNode oldManBeenThrough = DialogBuilder.CreateNode(
            "If you only knew what I've been through back in the day. This is nothing.",
            "Ben zamanında neler gördüm bir bilsen. Bu hiçbir şey.",
            "Amca"
        );
        DialogNode enginSeeDoctor = DialogBuilder.CreateNode(
            "You should see a doctor if you need to.",
            "Doktora git istersen.",
            "Engin"
        );
        DialogNode oldManNonsense = DialogBuilder.CreateEndNode(
            "Nonsense, no way. What for? I never take a punch I can't get back up from.",
            "Yok canım daha neler. Ne hacet var. Ben sonrasında ayağa kalkamayacağım hiçbir yumruğu yemem.",
            "Amca"
        );

        DialogBuilder.AddOption(enginAreYouOkay, DialogBuilder.CreateOption("...", "...", oldManBeenThrough, true));
        DialogBuilder.AddOption(oldManBeenThrough, DialogBuilder.CreateOption("...", "...", enginSeeDoctor, true));
        DialogBuilder.AddOption(enginSeeDoctor, DialogBuilder.CreateOption("...", "...", oldManNonsense, true));

        // Branch 1 - 2. Seçenek
        DialogNode enginWhoPunk = DialogBuilder.CreateNode(
            "Amca, pardon my asking, but who was that 'Lavuk'?",
            "Amca ayıptır sorması kimdi o Lavuk?",
            "Engin"
        );
        DialogNode oldManLongStory = DialogBuilder.CreateNode(
            "Long story. Just some punk.",
            "Uzun hikâye. Lavuğun biri işte.",
            "Amca"
        );
        DialogNode enginFilthyMouth = DialogBuilder.CreateNode(
            "He had a filthy mouth.",
            "Ağzı pis biriydi.",
            "Engin"
        );
        DialogNode oldManSoap = DialogBuilder.CreateEndNode(
            "If you gathered all the soap in the Eminönü, you couldn't wash that mouth clean. Let's eat something, I'll tell you what happened, don't worry.",
            "Eminönü’ndeki tüm sabunları toplasan onun ağzını temizleyemezsin. Bir şeyler yiyelim anlatacağım sana olanı biteni merak etme.",
            "Amca"
        );

        DialogBuilder.AddOption(enginWhoPunk, DialogBuilder.CreateOption("...", "...", oldManLongStory, true));
        DialogBuilder.AddOption(oldManLongStory, DialogBuilder.CreateOption("...", "...", enginFilthyMouth, true));
        DialogBuilder.AddOption(enginFilthyMouth, DialogBuilder.CreateOption("...", "...", oldManSoap, true));

        // Kamil Efendi mırıldandıktan sonra oyuncu seçim yapar
        DialogBuilder.AddOption(bakeryStartNode, DialogBuilder.CreateOption("Are you okay, Amca?", "İyi misin Amca?", enginAreYouOkay));
        DialogBuilder.AddOption(bakeryStartNode, DialogBuilder.CreateOption("Who was that?", "Kimdi o?", enginWhoPunk));
    }
}
