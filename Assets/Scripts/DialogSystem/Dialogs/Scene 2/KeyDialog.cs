using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class KeyDialog : MonoBehaviour
{
    [Header("References")]
    public Transform lookAtTarget; // Oyuncunun bakacağı hedef (Kuru karakteri)
    public Transform kuruLookTarget; // Kuru'nun döneceği/bakacağı hedef (Örn: Eşinin olduğu yer veya anahtar)
    public GameObject objectToActivate; // Dialog sonunda aktif edilecek obje
    public Transform playerBody;            // Oyuncu gövdesi (Sağa/Sola)
    public Transform playerCamera;          // Oyuncu kamerası (Yukarı/Aşağı)

    [Header("Rotation Settings")]
    [Tooltip("Dönüş işleminin saniye cinsinden süresi")]
    public float rotationDuration = 1.5f;   // Speed -> Duration
    public float lookOffset = 0.0f;         // Kuru'nun yüzüne tam bakmak için ince ayar gerekirse

    [Header("Activation Settings")]
    public float activationDelay = 2f; // Dialog bittikten kaç saniye sonra obje aktif olacak

    [Header("Ses Referansları")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;
    [Header("Kuru's Wife Sesleri")]
    [SerializeField] private AudioClip clip_wife_intro;
    [SerializeField] private AudioClip clip_wife_keysHanging;
    [Header("Kuru Sesleri")]
    [SerializeField] private AudioClip clip_kuru_whereKey;

    [Header("Mission Settings")]
    [SerializeField] private DialogSystem dialogSystem;
    [SerializeField] private MissionObjective missionObj;
    [SerializeField] private MissionManager missionManager;
    
    private LookAtController kuruLookController;

    private bool hasTriggered = false;
    private bool dialogCompleted = false; // Dialog tamamlandı mı?

    // Başlangıç rotasyonlarını saklamak için değişkenler
    private Quaternion startBodyRotation;
    private Quaternion startCamRotation;
    private Quaternion startKuruRotation;

    [System.NonSerialized] private DialogNode introNode;
    //[System.NonSerialized] private DialogNode afterDialogNode;

    void Start()
    {
        // DialogSystem'i otomatik bul
        dialogSystem = FindFirstObjectByType<DialogSystem>();
        missionObj = GetComponent<MissionObjective>();

        // Kuru'nun LookAtController'ını referans alıyoruz
        if (lookAtTarget != null)
        {
            // lookAtTarget genellikle kafa kemiği olduğu için scripti Ana Gövdede (Parent) arıyoruz
            kuruLookController = lookAtTarget.GetComponentInParent<LookAtController>();
            if (kuruLookController == null)
            {
                kuruLookController = lookAtTarget.GetComponentInChildren<LookAtController>();
            }
        }
        BuildDialogTree();
    }



    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Sadece ilk kez trigger'a girildiğinde intro dialog başlat
            if (missionObj.requiredMission == missionManager.CurrentMission && !hasTriggered)
            {
                hasTriggered = true;
                StartIntroDialog();
            }
        }
    }


    void StartIntroDialog()
    {
        if (dialogSystem != null)
        {
            dialogSystem.SetSpeakers(speakerAudios);
            dialogSystem.StartDialog(introNode);
            StartCoroutine(CheckDialogEnd());
        }
    }

    private System.Collections.IEnumerator CheckDialogEnd()
    {
        // Dialog paneli kapanana kadar bekle
        while (dialogSystem != null && dialogSystem.dialogPanel.activeSelf)
        {
            yield return null;
        }


        if (!dialogCompleted)
        {
            dialogCompleted = true;

            // Delay varsa bekle
            if (activationDelay > 0) yield return new WaitForSeconds(activationDelay);

            // Kuru'nun override look target'ını temizle
            if (kuruLookController != null)
            {
                kuruLookController.ClearOverrideLookTarget();
                Transform kuruRoot = kuruLookController.transform;
                kuruRoot.DOKill();
                kuruRoot.DORotateQuaternion(startKuruRotation, rotationDuration).SetEase(Ease.OutQuad);
            }

            // Obje aktif edilme süreci (Anahtar ortaya çıkıyor)
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }

            // Trigger box işlevini tamamladı, artık silinebilir.
            Destroy(gameObject);
        }
    }

    void RotateCameraToTarget()
    {
        if (lookAtTarget == null) return;

        // Kuru bize dönmüyor, sadece biz ona bakıyoruz.

        // A. Gövde Dönüşü (Y ekseni - Sağa sola)
        if (playerBody != null)
        {
            playerBody.DOLookAt(lookAtTarget.position, rotationDuration, AxisConstraint.Y).SetEase(Ease.OutQuad);
        }

        // B. Kamera Dönüşü (X ekseni - Yukarı aşağı)
        if (playerCamera != null)
        {
            playerCamera.DOLookAt(lookAtTarget.position, rotationDuration).SetEase(Ease.OutQuad);
        }
        
        // C. Kuru'nun (NPC) Atanan Hedefe Dönmesi
        if (kuruLookTarget != null && kuruLookController != null)
        {
            // 1. Önce LookAtController scriptimizi kullanarak kafanın yumuşakça hedefe kilitlenmesini zorluyoruz (Override)
            kuruLookController.SetOverrideLookTarget(kuruLookTarget);

            // 2. Anahtar Kuru'nun tam arkasında olduğu için SADECE kafa/omurga dönüşü (IK) ile arkaya bakmaya çalışmak iskeleti bozar.
            // Bu yüzden karakterin ana gövdesini (Root objesini) de DOTween ile o yöne dönmeye zorluyoruz.
            Transform kuruRoot = kuruLookController.transform;
            
            kuruRoot.DOKill();
            kuruRoot.DOLookAt(kuruLookTarget.position, rotationDuration, AxisConstraint.Y).SetEase(Ease.OutQuad);
        }
    }

    void ReturnCameraToOriginal()
    {
        // Kaydettiğimiz başlangıç rotasyonuna geri dönüyoruz
        if (playerBody != null)
        {
            playerBody.DORotateQuaternion(startBodyRotation, rotationDuration).SetEase(Ease.OutQuad);
        }

        if (playerCamera != null)
        {
            playerCamera.DORotateQuaternion(startCamRotation, rotationDuration).SetEase(Ease.OutQuad);
        }
    }



    void BuildDialogTree()
    {
        // === INTRO NODE (Kuru'nun eşinin bağırması) ===
        introNode = DialogBuilder.CreateNode(
            "(Shouts from inside) GIVE ZEKİ HIS ROOM. I HAD THE DOOR FIXED YESTERDAY. HE WON'T BE COMING TO THE ROOM ANYWAY.",
            "(İçerden bağırır) ZEKİ'NİN ODASINI VER. KAPISINI DÜN YAPTIRDIM. O DA YAKINDA GELMEZ ZATEN.",
            "Kuru's Wife"
        );

        // === KURU'NUN CEVABI ===
        DialogNode kuruResponse = DialogBuilder.CreateNode(
            "Where's the key?",
            "Anahtarı nerde?",
            "Kuru"
        );

        // === EŞİNİN SON CEVABI ===
        DialogNode wifeEnd = DialogBuilder.CreateEndNode(
            "Where do we keep the keys? It's hanging behind you. Idiot.",
            "Biz anahtarları nereye koyuyoruz. Arkanda asılı duruyor. Salak.",
            "Kuru's Wife"
        );

        // === AFTER DIALOG NODE (Dialog tamamlandıktan sonra) ===
        //afterDialogNode = DialogBuilder.CreateEndNode(
        //    "I gave you your key, go to your room.",
        //    "Anahtarını verdim, odana git.",
        //    "Kuru"
        //);

        // === INTRO NODE'A "..." SEÇENEĞİNİ EKLE ===
        DialogOption introToContinue = DialogBuilder.CreateOptionWithEvent(
            "...",
            "...",
            kuruResponse, // Kuru'nun cevabına git
            () => {
                // "..." seçeneğine tıklandığında dönme hareketini gerçekleştir
                // Oyuncunun o anki rotasyonunu kaydet
                if (playerBody != null) startBodyRotation = playerBody.rotation;
                if (playerCamera != null) startCamRotation = playerCamera.rotation;
            if (kuruLookController != null) startKuruRotation = kuruLookController.transform.rotation;

                // Oyuncuyu Kuru'ya döndür
                RotateCameraToTarget();
            },
            true // isSilentOption
        );
        DialogBuilder.AddOption(introNode, introToContinue);

        // === KURU'DAN EŞİNE GEÇIŞ ===
        DialogOption kuruToWife = DialogBuilder.CreateOption("...", "...", wifeEnd, true);
        DialogBuilder.AddOption(kuruResponse, kuruToWife);

        introNode.voiceClip = clip_wife_intro;
        kuruResponse.voiceClip = clip_kuru_whereKey;
        wifeEnd.voiceClip = clip_wife_keysHanging;
    }
}
