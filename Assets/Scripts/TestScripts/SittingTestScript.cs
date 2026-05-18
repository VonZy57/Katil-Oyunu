using UnityEngine;
using UnityEngine.UIElements;

public class SittingTestScript : Interactable
{
    [Header("Oturma Ayarlar�")]
    public Transform sitReference;
    public Transform standReference;
    public float sitSpeed = 2f;

    [Header("Oyuncu Referans�")]
    public GameObject player;

    private bool isSitting = false;
    private bool isMoving = false;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private FirstPersonController playerFPS;
    private CharacterController playerController;

    private void Start()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerFPS = player.GetComponent<FirstPersonController>();
            playerController = player.GetComponent<CharacterController>();
        }

        promptMessage = "E - Otur";
    }

    protected override void Interact()
    {
        // E�er hareket halindeysek (oturuyor veya kalk�yorsak) komutu reddet
        if (isMoving) return;

        isSitting = !isSitting;
        isMoving = true;

        if (isSitting)
        {
            // --- OTURMA BA�LADI ---
            originalPosition = player.transform.position;
            originalRotation = player.transform.rotation;

            if (playerController) playerController.enabled = false;
            if (playerFPS) playerFPS.enabled = false;

            promptMessage = "E - Kalk";
        }
        else
        {
            // --- KALKMA BA�LADI ---
            if (playerController) playerController.enabled = false;

            if (playerFPS)
            {
                playerFPS.SetSittingState(false);
                playerFPS.enabled = false;
            }

            promptMessage = "E - Otur";
        }
    }

    protected override void Update()
    {
        base.Update(); // Üst sınıftaki (Interactable) outline mesafe kontrolünü çalıştır

        // 1. D�ZELTME: E�ER OTURUYORSAK, RAYCAST'E �HT�YA� DUYMADAN E'Y� D�NLE
        // Oyuncu otururken koltu�a bakm�yor olabilir, bu y�zden manuel input kontrol� ekliyoruz.
        if (isSitting && !isMoving)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }

        if (!isMoving || player == null) return;

        Vector3 targetPosition = isSitting ? sitReference.position : standReference.position;
        Quaternion targetRotation = isSitting ? sitReference.rotation : standReference.rotation;

        float dist = Vector3.Distance(player.transform.position, targetPosition);

        // HEDEFE G�D�� (LERP)
        if (dist > 0.05f)
        {
            player.transform.position = Vector3.Lerp(player.transform.position, targetPosition, Time.deltaTime * sitSpeed);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, Time.deltaTime * sitSpeed);
        }
        else
        {
            // HEDEFE VARDIK
            player.transform.position = targetPosition;
            player.transform.rotation = targetRotation;

            isMoving = false;

            if (isSitting)
            {
                // Koltu�a oturdu
                if (playerFPS)
                {
                    playerFPS.enabled = true;
                    playerFPS.SetSittingState(true);
                }
            }
            else
            {
                // Aya�a kalkt�
                if (playerController) playerController.enabled = true;
                if (playerFPS)
                {
                    playerFPS.enabled = true;
                    playerFPS.SetSittingState(false);
                }
            }
        }
    }
}