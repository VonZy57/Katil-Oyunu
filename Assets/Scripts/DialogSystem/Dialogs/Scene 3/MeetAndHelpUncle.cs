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
    [SerializeField] private Animator amcaAnimator;
    [SerializeField] private AmcaMovement amcaMovement;

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

        if (amcaAnimator != null)
        {
            amcaAnimator.SetTrigger("HelpUpTrigger");
        }

        Debug.Log("Animasyon: Engin amcayı yerden kaldırır...");
        
        Camera cam = playerCamera != null ? playerCamera.GetComponent<Camera>() : null;
        if (cam == null && playerCamera != null) cam = playerCamera.GetComponentInChildren<Camera>();
        if (cam == null) cam = Camera.main; // Fallback

        Transform camTransform = cam != null ? cam.transform : (playerCamera != null ? playerCamera.transform : transform);

        float waitTime = 6f; // Kaldırma animasyonu süresi
        float timer = 0f;

        // Amca ayağa kalkarken oyuncunun kamerası amcayı yumuşakça takip etsin
        while (timer < waitTime)
        {
            if (camTransform != null && amcaTransform != null)
            {
                Vector3 direction = (amcaTransform.position - camTransform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(direction);
                    camTransform.rotation = Quaternion.Slerp(camTransform.rotation, targetRot, Time.deltaTime * 5f);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("Animasyon bitti: Engin amcayı kaldırdı.");

        // Animasyon bitince Amca yüzünü oyuncuya (kameraya) dönsün
        if (amcaTransform != null && camTransform != null)
        {
            amcaTransform.DOKill();
            yield return gameObject.transform.DOLookAt(camTransform.position, 1f, AxisConstraint.Y).SetEase(Ease.InOutSine).WaitForCompletion();
            Debug.Log("Amca kameraya döndü.");
        }

        // Amca döndükten sonra diyalog başlasın
        if (dialogSystem != null && amcaStartNode != null)
        {
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

        // Diyalog bittiğinde Amca'nın tag'ını "Amca" olarak değiştir
        if (amcaTransform != null)
        {
            this.gameObject.tag = "Amca";
        }

        yield return new WaitForSeconds(1f); // Diyalog kapanma animasyonu için küçük bir bekleme
        
        Debug.Log("Amca fırına gider ve masaya oturur...");
        amcaMovement.StartWalkingToSeat();

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
        ("Atta boy.",
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