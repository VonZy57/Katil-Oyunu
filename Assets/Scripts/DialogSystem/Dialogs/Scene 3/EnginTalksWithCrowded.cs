using UnityEngine;
using DG.Tweening;
using System.Collections;

public class EnginTalksWithCrowded : MonoBehaviour
{
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private DialogNode crowdStartNode;
    [SerializeField] private MissionObjective missionObj;

    [Header("Kamera ve Bakış Referansları")]
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private Transform crowd1Transform; // Someone from the Crowd
    [SerializeField] private Transform crowd2Transform; // Another from the Crowd
    [SerializeField] private Transform crowd3Transform; // A Completely Different Person (Kadın)

    private FirstPersonController playerFPS;

    void Start()
    {
        if (playerCamera != null)
        {
            playerFPS = playerCamera.GetComponentInParent<FirstPersonController>();
        }
        BuildDialog();
    }

    public void StartCrowdDialog()
    {
        if (dialogSystem != null && crowdStartNode != null)
        {
            if (playerFPS != null) playerFPS.enabled = false;

            if (playerCamera != null && crowd1Transform != null)
            {
                Camera cam = playerCamera.GetComponent<Camera>();
                if (cam == null) cam = playerCamera.GetComponentInChildren<Camera>();
                if (cam == null) cam = Camera.main; // Fallback

                Transform camTransform = cam != null ? cam.transform : playerCamera.transform;
                camTransform.DOKill();
                camTransform.DOLookAt(crowd1Transform.position, 1f).SetEase(Ease.InOutSine); // İlk konuşana çevir

                Debug.Log($"Placeholder Animasyon: {crowd1Transform.name} (Konuşan kişi) Engin'e dönüyor.");
                crowd1Transform.DOKill();
                crowd1Transform.DOLookAt(camTransform.position, 1f, AxisConstraint.Y).SetEase(Ease.InOutSine);
            }

            StartCoroutine(DialogSequence());
        }
    }

    private IEnumerator DialogSequence()
    {
        dialogSystem.StartDialog(crowdStartNode);

        yield return null;
        yield return new WaitUntil(() => dialogSystem.dialogPanel.activeSelf);
        Debug.Log("Diyalog başladı.");
        yield return new WaitUntil(() => !dialogSystem.dialogPanel.activeSelf);

        if (playerFPS != null)
        {
            playerFPS.SyncCameraRotation(); // Eski açıya dönmesini engelle
            playerFPS.enabled = true; 
        }

        if (missionObj != null)
        {
            missionObj.OnInteracted();
            Debug.Log("Görev tamamlandı");
        }
    }

    private IEnumerator LookAtTargetSequence(Transform targetTransform, float duration = 0.7f)
    {
        if (playerCamera != null && targetTransform != null)
        {
            Camera cam = playerCamera.GetComponent<Camera>();
            if (cam == null) cam = playerCamera.GetComponentInChildren<Camera>();
            if (cam == null) cam = Camera.main;

            Transform camTransform = cam != null ? cam.transform : playerCamera.transform;
            camTransform.DOKill();
            camTransform.DOLookAt(targetTransform.position, duration).SetEase(Ease.InOutSine);

            Debug.Log($"Placeholder Animasyon: {targetTransform.name} (Konuşan kişi) Engin'e dönüyor.");
            targetTransform.DOKill();
            targetTransform.DOLookAt(camTransform.position, duration, AxisConstraint.Y).SetEase(Ease.InOutSine);
        }
        yield return null;
    }

    void BuildDialog()
    {
        crowdStartNode = DialogBuilder.CreateNode
        ("Somebody help the old man up.",
        "Biri amcayı kaldırsın.",
        "Someone from the Crowd");

        // Silent Opt - 1 Place

        DialogNode crowd2Node = DialogBuilder.CreateNode
        ("It's your turn, you're picking up the old guy this time.",
        "Sıra sende bu sefer sen yaşlı kaldırıyorsun.",
        "Someone from the Crowd but Woman ");

        // Silent Opt - 1
        DialogOption crowd2Opt = DialogBuilder.CreateOptionWithEvent("...", "...", crowd2Node, () => { StartCoroutine(LookAtTargetSequence(crowd2Transform)); }, true);
        DialogBuilder.AddOption(crowdStartNode, crowd2Opt);

        // Silent Opt - 2 Place

        DialogNode crowd3Node = DialogBuilder.CreateNode
        ("I just picked one up last week!",
        "Daha geçen hafta ben kaldırdım.",
        "Someone from the Crowd");

        // Silent Opt - 2
        DialogOption crowd3Opt = DialogBuilder.CreateOptionWithEvent("...", "...", crowd3Node, () => { StartCoroutine(LookAtTargetSequence(crowd1Transform)); }, true);
        DialogBuilder.AddOption(crowd2Node, crowd3Opt);

        // Silent Opt - 3 Place

        DialogNode crowd4Node = DialogBuilder.CreateNode
        ("Do you have any idea how many old people fall down around here in a week?",
        "Bir hafta da kaç yaşlı düşüyor burada haberin var mı?",
        "A Completely Different Person");

        // Silent Opt - 3
        DialogOption crowd4Opt = DialogBuilder.CreateOptionWithEvent("...", "...", crowd4Node, () => { StartCoroutine(LookAtTargetSequence(crowd3Transform)); }, true);
        DialogBuilder.AddOption(crowd3Node, crowd4Opt);

        // Silent Opt - 4 Place
        
        DialogNode crowd5Node = DialogBuilder.CreateNode
        ("Young man, you're new here, you help him up. Join our neighborhood's tradition.",
        "Delikanlı, sen yenisin sen kaldır. Mahallemizin bu adetine dahil ol.",
        "Someone from the Crowd");

        // Silent Opt - 4
        DialogOption crowd5Opt = DialogBuilder.CreateOptionWithEvent("...", "...", crowd5Node, () => { StartCoroutine(LookAtTargetSequence(crowd1Transform)); }, true);
        DialogBuilder.AddOption(crowd4Node, crowd5Opt);

        // ==============================
        // 1. DAL: "Sure." / "Olur"
        // ==============================
        DialogNode sureEnginNode = DialogBuilder.CreateNode
        ("Of course. Helping the elderly is our duty.",
        "Tabii ki. Yaşlıları kaldırmak boynumuzun borcu.",
        "Engin");

        DialogNode sureCrowdNode = DialogBuilder.CreateEndNode
        ("Good for you. They don't make young men like you anymore.",
        "Helal olsun sana. Kalmadı senin gibi delikanlılar.",
        "Someone from the Crowd");

        DialogOption sureEnginToCrowdOpt = DialogBuilder.CreateOptionWithEvent("...", "...", sureCrowdNode, () => { StartCoroutine(LookAtTargetSequence(crowd1Transform)); }, true);
        DialogBuilder.AddOption(sureEnginNode, sureEnginToCrowdOpt);

        // ==============================
        // 2. DAL: "Why me?" / "Niye ben?"
        // ==============================
        DialogNode whyMeEnginNode = DialogBuilder.CreateNode
        ("Why do I have to do it, man? I don't even know the guy.",
        "Neden ben yapıyorum abi. Amcayı tanımam etmem.",
        "Engin");

        DialogNode whyMeCrowdNode = DialogBuilder.CreateEndNode
        ("Oh, sure, we all know and love him so much. Look, don't piss me off. I don't want to carry another old man this week.",
        "He biz tanırız bayılırız zaten. Bak benim tepemin tasını attırma. Ben bu hafta bir tane daha yaşlı taşımak istemiyorum.",
        "Someone from the Crowd");
        
        DialogOption whyMeEnginToCrowdOpt = DialogBuilder.CreateOptionWithEvent("...", "...", whyMeCrowdNode, () => { StartCoroutine(LookAtTargetSequence(crowd1Transform)); }, true);
        DialogBuilder.AddOption(whyMeEnginNode, whyMeEnginToCrowdOpt);

        // Ana Düğümden Oyuncu Seçenekleri
        DialogOption sureOption = DialogBuilder.CreateOption("Sure.", "Olur", sureEnginNode);
        DialogOption whyMeOption = DialogBuilder.CreateOption("Why me?", "Niye ben?", whyMeEnginNode);

        DialogBuilder.AddOption(crowd5Node, sureOption);
        DialogBuilder.AddOption(crowd5Node, whyMeOption);
    }
}
