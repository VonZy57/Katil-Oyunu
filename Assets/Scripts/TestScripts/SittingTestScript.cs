using UnityEngine;
using UnityEngine.UIElements;

public class SittingTestScript : Interactable
{
    [Header("Oturma Ayarlarý")]
    public Transform sitReference;
    public Transform standReference;
    public float sitSpeed = 2f;

    [Header("Oyuncu Referansý")]
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
        // Eðer hareket halindeysek (oturuyor veya kalkýyorsak) komutu reddet
        if (isMoving) return;

        isSitting = !isSitting;
        isMoving = true;

        if (isSitting)
        {
            // --- OTURMA BAÞLADI ---
            originalPosition = player.transform.position;
            originalRotation = player.transform.rotation;

            if (playerController) playerController.enabled = false;
            if (playerFPS) playerFPS.enabled = false;

            promptMessage = "E - Kalk";
        }
        else
        {
            // --- KALKMA BAÞLADI ---
            if (playerController) playerController.enabled = false;

            if (playerFPS)
            {
                playerFPS.SetSittingState(false);
                playerFPS.enabled = false;
            }

            promptMessage = "E - Otur";
        }
    }

    private void Update()
    {
        // 1. DÜZELTME: EÐER OTURUYORSAK, RAYCAST'E ÝHTÝYAÇ DUYMADAN E'YÝ DÝNLE
        // Oyuncu otururken koltuða bakmýyor olabilir, bu yüzden manuel input kontrolü ekliyoruz.
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

        // HEDEFE GÝDÝÞ (LERP)
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
                // Koltuða oturdu
                if (playerFPS)
                {
                    playerFPS.enabled = true;
                    playerFPS.SetSittingState(true);
                }
            }
            else
            {
                // Ayaða kalktý
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