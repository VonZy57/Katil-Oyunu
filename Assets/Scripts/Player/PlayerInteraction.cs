using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Kamera Referansı")]
    public Transform cameraTransform;

    [Header("Etkileşim Ayarları")]
    public float interactDistance = 3f;
    public LayerMask interactLayer;
    public LayerMask obstacleLayer;

    [Header("UI Referansları")]
    public GameObject promptPanel;
    public TextMeshProUGUI promptText;
    public DialogSystem dialogSystem;
    private CanvasGroup promptCanvasGroup;

    private PlayerControls controls;
    private Interactable currentInteractable;
    private bool hasInteracted = false;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Player.Enable(); // Input System aktif

        // Oyuncu Interact tuşuna bastığında "OnInteractPressed" fonksiyonunu tetikle diyoruz.
        controls.Player.Interact.performed += OnInteractPressed;
    }

    private void OnDisable()
    {
        controls.Player.Disable(); // Input System pasif

        // Obje kapanırsa dinlemeyi bırak, aksi takdirde memory leak (bellek sızıntısı) yapar.
        controls.Player.Interact.performed -= OnInteractPressed;
    }

    private void Start()
    {
        if (promptPanel != null)
        {
            promptCanvasGroup = promptPanel.GetComponent<CanvasGroup>();
            if (promptCanvasGroup == null)
            {
                promptCanvasGroup = promptPanel.AddComponent<CanvasGroup>();
            }

            promptPanel.SetActive(true);
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }

        if (dialogSystem == null)
            dialogSystem = FindFirstObjectByType<DialogSystem>();
    }

    private void Update()
    {
        // Update içinde ARTIK TUŞ KONTROLÜ YOK. Sadece nereye baktığımızı kontrol ediyoruz.
        CheckForInteractable();
        

    }

    // --- ETKİLEŞİM FONKSİYONU ---
    // Tuşa basıldığında otomatik olarak burası çalışacak
    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        // Eğer spam koruması aktif değilse VE baktığımız bir nesne varsa
        if (!hasInteracted && currentInteractable != null)
        {
            hasInteracted = true;
            HidePrompt();
            currentInteractable.BaseInteract();

            Invoke("ResetInteraction", 0.5f);
        }
    }

    private void ResetInteraction()
    {
        hasInteracted = false;
    }

    private void CheckForInteractable()
    {
        bool isDialogActive = dialogSystem != null && dialogSystem.dialogPanel.activeSelf;

        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        LayerMask combinedMask = interactLayer | obstacleLayer;

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, combinedMask) && !isDialogActive)
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowPrompt(interactable.promptMessage);
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable = null;
            HidePrompt();
        }
    }

    public void ShowPrompt(string message)
    {
        if (promptPanel != null && promptText != null && promptCanvasGroup != null)
        {
            promptText.text = message;
            promptCanvasGroup.alpha = 1f;
            promptCanvasGroup.interactable = true;
            promptCanvasGroup.blocksRaycasts = true;
        }
    }

    public void HidePrompt()
    {
        if (promptPanel != null && promptCanvasGroup != null)
        {
            promptCanvasGroup.alpha = 0f;
            promptCanvasGroup.interactable = false;
            promptCanvasGroup.blocksRaycasts = false;
        }
    }
}