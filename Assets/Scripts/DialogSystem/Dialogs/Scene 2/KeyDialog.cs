using DG.Tweening;
using UnityEngine;

public class KeyDialog : MonoBehaviour
{
    [Header("References")]
    public Transform lookAtTarget; // Oyuncunun bakacağı hedef (Kuru karakteri)
    public GameObject objectToActivate; // Dialog sonunda aktif edilecek obje
    public Transform playerBody;            // Oyuncu gövdesi (Sağa/Sola)
    public Transform playerCamera;          // Oyuncu kamerası (Yukarı/Aşağı)

    [Header("Rotation Settings")]
    [Tooltip("Dönüş işleminin saniye cinsinden süresi")]
    public float rotationDuration = 1.5f;   // Speed -> Duration
    public float lookOffset = 0.0f;         // Kuru'nun yüzüne tam bakmak için ince ayar gerekirse

    [Header("Activation Settings")]
    public float activationDelay = 2f; // Dialog bittikten kaç saniye sonra obje aktif olacak

    private DialogSystem dialogSystem;

    private bool hasTriggered = false;
    private bool dialogCompleted = false; // Dialog tamamlandı mı?

    // Başlangıç rotasyonlarını saklamak için değişkenler
    private Quaternion startBodyRotation;
    private Quaternion startCamRotation;

    [System.NonSerialized] private DialogNode introNode;
    [System.NonSerialized] private DialogNode afterDialogNode;

    void Start()
    {
        // DialogSystem'i otomatik bul
        dialogSystem = FindFirstObjectByType<DialogSystem>();
        BuildDialogTree();
    }



    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Sadece ilk kez trigger'a girildiğinde intro dialog başlat
            if (!hasTriggered)
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

            // Obje aktif edilme süreci (Anahtar ortaya çıkıyor vs.)
            if (objectToActivate != null)
            {
                // Delay varsa bekle, yoksa hemen aç
                if (activationDelay > 0) yield return new WaitForSeconds(activationDelay);
                objectToActivate.SetActive(true);
            }

            // Dönüşün tamamlanması için süre tanı (rotationDuration kadar bekle)
            yield return new WaitForSeconds(rotationDuration);

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
            "(Shouts from inside) GIVE ZEKİ'S ROOM. I HAD THE DOOR FIXED YESTERDAY. HE WON'T BE COMING TO THE ROOM ANYWAY.",
            "(İçerden bağırır) ZEKİ'NİN ODASINI VER. KAPISINI DÜN YAPTIRDIM. ODA YAKINDA GELMEZ ZATEN.",
            "Kuru'nun Eşi"
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
            "Kuru'nun Eşi"
        );

        // === AFTER DIALOG NODE (Dialog tamamlandıktan sonra) ===
        afterDialogNode = DialogBuilder.CreateEndNode(
            "I gave you your key, go to your room.",
            "Anahtarını verdim, odana git.",
            "Kuru"
        );

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

                // Oyuncuyu Kuru'ya döndür
                RotateCameraToTarget();
            },
            true // isSilentOption
        );
        DialogBuilder.AddOption(introNode, introToContinue);

        // === KURU'DAN EŞİNE GEÇIŞ ===
        DialogOption kuruToWife = DialogBuilder.CreateOption("...", "...", wifeEnd, true);
        DialogBuilder.AddOption(kuruResponse, kuruToWife);
    }
}
