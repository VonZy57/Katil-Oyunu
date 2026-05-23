using UnityEngine;
using DG.Tweening;
using System.Collections;

public class MeetAndHelpUncle : Interactable
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode amcaStartNode;
    [SerializeField] private MissionObjective missionObj;

    [Header("Kamera ve Bakış Referansları")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform amcaTransform;

    private FirstPersonController playerFPS;
    private bool isInteracting = false;

    void Start()
    {
        promptMessage = "E - Help Uncle";

        if (playerCamera != null)
        {
            playerFPS = playerCamera.GetComponentInParent<FirstPersonController>();
        }
        BuildDialog();
    }

    protected override void Update()
    {
        base.Update(); // Outline ve raycast mesafe kontrolü için gerekli
        if (MissionManager.Instance != null && missionObj != null)
        {
            if (MissionManager.Instance.CurrentMission == missionObj.requiredMission && !isInteracting)
                promptMessage = "E - Help Uncle";
            else
                promptMessage = "";
        }
    }

    protected override void Interact()
    {
        if (!isInteracting && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
        {
            isInteracting = true;
            StartCoroutine(HelpUncleSequence());
        }
    }

    private IEnumerator HelpUncleSequence()
    {
        if (playerFPS != null) playerFPS.enabled = false;

        Debug.Log("Placeholder Animasyon: Engin amcayı yerden kaldırır...");
        yield return new WaitForSeconds(2f); // Kaldırma animasyonu süresi
        Debug.Log("Animasyon bitti: Engin amcayı kaldırdı.");

        if (dialogSystem != null && amcaStartNode != null)
        {
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
        dialogSystem.StartDialog(amcaStartNode);

        yield return null;
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);

        // 3. Diyalog: Görev diyalog bitince değişsin, animasyon ondan sonra başlasın
        if (missionObj != null)
        {
            missionObj.OnInteracted();
        }

        Debug.Log("Placeholder Animasyon: Diyalog bittikten sonra çalışacak animasyon (Örn: Üstünü başını silkeler, fırına yönelir)");
        yield return new WaitForSeconds(2f);
        Debug.Log("Amca diyalog sonrası animasyonu bitti.");

        // Diyalog tamamen bittiğinde oyuncu kontrollerini geri ver
        if (playerFPS != null)
        {
            playerFPS.SyncCameraRotation(); // Eski açıya dönmesini engelle
            playerFPS.enabled = true; 
        }
    }

    void BuildDialog()
    {
        amcaStartNode = DialogBuilder.CreateNode
        ("Thank you, young man. People like you are rare these days. If it weren't for you, I would've been left on the ground. Let me treat you to a pastry and some ayran. And I won't take no for an answer.",
        "Sağ olasın delikanlı. Senin gibi insanlar az bulunur oldu. Sen de olmasan yerde kalacaktım. İzin ver sana bir poğaça ayran ısmarlayayım. Ve hayırı cevap olarak kabul etmiyorum.",
        "Amca");

        // ==============================
        // AMCA SEÇENEK 1: "Yes." / "Evet"
        // ==============================
        DialogNode amcaYesEnginNode = DialogBuilder.CreateNode
        ("That sounds good, I'll get to have some breakfast too.",
        "Güzel olur, ben de kahvaltı yapmış olurum.",
        "Engin");
        DialogNode amcaYesResponseNode = DialogBuilder.CreateEndNode
        ("That's my boy.",
        "He yaşa oğlum benim.",
        "Amca");
        DialogBuilder.AddOption(amcaYesEnginNode, DialogBuilder.CreateOption("...", "...", amcaYesResponseNode, true));

        // ==============================
        // AMCA SEÇENEK 2: "No." / "Hayır"
        // ==============================
        DialogNode amcaNoEnginNode = DialogBuilder.CreateNode
        ("No, I'm busy, I don't have time for a pastry right now.",
        "Hayır benim işim gücüm var poğaçaya zaman ayıramam.",
        "Engin");
        DialogNode amcaNoResponseNode = DialogBuilder.CreateEndNode
        ("I won't accept that. There's always time for a pastry.",
        "Kabul etmiyorum. Her zaman poğaçaya ayrılacak zaman vardır.",
        "Amca");
        DialogBuilder.AddOption(amcaNoEnginNode, DialogBuilder.CreateOption("...", "...", amcaNoResponseNode, true));

        // Amca Düğümünden Oyuncu Seçenekleri
        DialogBuilder.AddOption(amcaStartNode, DialogBuilder.CreateOption("Yes.", "Evet", amcaYesEnginNode));
        DialogBuilder.AddOption(amcaStartNode, DialogBuilder.CreateOption("No.", "Hayır", amcaNoEnginNode));
    }
}