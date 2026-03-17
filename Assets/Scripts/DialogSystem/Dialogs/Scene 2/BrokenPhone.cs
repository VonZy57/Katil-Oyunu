using DG.Tweening;
using UnityEngine;

public class BrokenPhone : Interactable
{
    [Header("References")]
    public Transform lookAtTarget;    // Bakılacak hedef (Kız)
    public Transform playerBody;      // Karakterin ana gövdesi (Sağa/Sola dönüş için)
    public Transform playerCamera;    // Karakterin kamerası (Yukarı/Aşağı bakış için)

    [Header("Rotation Settings")]
    public float rotationDuration = 2f;  // Dönüş hızı
    [Tooltip("Target'ın ne kadar üstüne bakılsın?")]
    public float lookOffset = 0.5f;   // Hedefin ne kadar üstüne bakılacağı

    private DialogSystem dialogSystem;
    private MissionObjective missionObj;
    private DialogNode phoneDialogNode;
    private bool hasInteracted = false; // Tek seferlik kontrol için


    void Start()
    {
        BuildDialogTree();
        dialogSystem = FindFirstObjectByType<DialogSystem>();
        missionObj = GetComponent<MissionObjective>();
    }

    void Update()
    {
        if (!hasInteracted && missionObj != null && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
        {
            promptMessage = "E - Telephone";
        }
        else
        {
            promptMessage = ""; // Görev aktif değilse veya etkileşime girildiyse yazı çıkmasın
        }

        
    }

    protected override void Interact()
    {
        // Eğer daha önce etkileşime girildiyse veya görev sırası değilse işlem yapma
        if (hasInteracted || (missionObj != null && MissionManager.Instance.CurrentMission != missionObj.requiredMission))
            return;

        // 1. Etkileşimi kilitle ve mesajı sil
        hasInteracted = true;
        promptMessage = "";

        // 2. Bakış işlemini başlat (DOTween)
        RotateCameraToTarget();

        // 3. Diyalog sistemini başlat
        if (dialogSystem != null)
        {
            dialogSystem.StartDialog(phoneDialogNode);
            StartCoroutine(CheckDialogEnd());
        }
    }

    void RotateCameraToTarget()
    {
        if (lookAtTarget == null) return;

        // A. Karakterin Gövdesini (Body) hedefe döndür (Sadece Y ekseninde)
        // Böylece karakterin vücudu hedefe döner ama havaya bakmaz.
        if (playerBody != null)
        {
            playerBody.DOLookAt(lookAtTarget.position, rotationDuration, AxisConstraint.Y);
        }

        // B. Kamerayı (Camera) hedefin biraz üstüne döndür
        // Hedef pozisyon + Offset (Yukarı)
        if (playerCamera != null)
        {
            Vector3 targetPositionWithOffset = lookAtTarget.position + (Vector3.up * lookOffset);
            playerCamera.DOLookAt(targetPositionWithOffset, rotationDuration);
        }
        
    }

    private System.Collections.IEnumerator CheckDialogEnd()
    {
        // Dialog paneli kapanana kadar bekle
        while (dialogSystem != null && dialogSystem.dialogPanel.activeSelf)
        {
            yield return null;
        }



        // Sonraki görevi tetikler
        if (missionObj != null)
            missionObj.OnInteracted();
    }

    void BuildDialogTree()
    {
        // Kız'ın telefon hakkındaki diyaloğu (tek seferlik)
        phoneDialogNode = DialogBuilder.CreateEndNode(
            "Big brother, that phone is broken.",
            "Abi o telefon bozuk.",
            "Little Girl"
        );
    }
}
