using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class LocalizedText
{
    [TextArea(2, 5)]
    public string english;
    [TextArea(2, 5)]
    public string turkish;

    public string GetText(bool isTurkish)
    {
        return isTurkish ? turkish : english;
    }
}

public class DialogOption
{
    public LocalizedText optionText;
    public DialogNode responseNode;
    public UnityEngine.Events.UnityEvent onSelect;
    public bool isSilentOption = false; // "..." gibi sessiz cevaplar için
}

public class DialogNode
{
    public string speakerName;
    public LocalizedText dialogText;
    public List<DialogOption> optionsList;
    public bool isEndDialog;
}

public class DialogSystem : MonoBehaviour
{
    [Header("UI Referansları")]
    public GameObject dialogPanel;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogText;
    public Transform optionsContainer;
    public GameObject optionButtonPrefab;
    public List<Transform> optButtonRef;
    public Transform silentOptionButtonRef; // "..." gibi cevaplar için ayrı konum

    [Header("Dil Ayarları")]
    public bool isTurkish = false;
    // public KeyCode languageToggleKey = KeyCode.L; ARTIK İHTİYACIMIZ YOK, INPUT SYSTEM KULLANACAĞIZ

    [Header("Görsel Ayarlar")]
    public float typewriterSpeed = 0.1f;
    public bool useTypewriterEffect = true;

    private DialogNode currentNode;
    private bool isTyping = false;
    private List<GameObject> currentOptionButtons = new List<GameObject>();
    private FirstPersonController playerController;
    private bool waitingForClick = false;

    // --- YENİ INPUT SYSTEM DEĞİŞKENİ ---
    private PlayerControls controls;

    private void Awake()
    {
        // PlayerControls sınıfından bir kontrol şeması oluşturuyoruz.
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        playerController = FindFirstObjectByType<FirstPersonController>();
    }

    private void Update()
    {
        // 1. DİL DEĞİŞTİRME KONTROLÜ (Input System: ChangeLanguage)
        if (controls.Player.ChangeLanguage.triggered)
        {
            isTurkish = !isTurkish;

            if (dialogPanel.activeSelf && currentNode != null)
            {
                ShowDialog();
            }
        }

        // 2. YAZIYI HIZLICA GEÇME KONTROLÜ (Input System: Confirm - Space)
        if (isTyping && controls.Player.SpaceButton.triggered)
        {
            StopAllCoroutines();
            dialogText.text = currentNode.dialogText.GetText(isTurkish);
            isTyping = false;
            ShowOptions();
        }

        // 3. DİALOGU BİTİRME KONTROLÜ
        if (waitingForClick && controls.Player.Attack.triggered)
        {
            EndDialog();
        }
    }

    public void StartDialog(DialogNode startNode)
    {
        if (startNode == null)
        {
            return;
        }

        currentNode = startNode;
        dialogPanel.SetActive(true);
        waitingForClick = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        DisablePlayerMovement();

        ShowDialog();
    }

    private void ShowDialog()
    {
        // Konuşan karakterin ismini göster
        if (speakerNameText != null && !string.IsNullOrEmpty(currentNode.speakerName))
        {
            speakerNameText.text = currentNode.speakerName;
        }

        string textToShow = currentNode.dialogText.GetText(isTurkish);

        if (useTypewriterEffect)
        {
            StartCoroutine(TypewriterEffect(textToShow));
        }
        else
        {
            dialogText.text = textToShow;
            ShowOptions();
        }
    }

    private System.Collections.IEnumerator TypewriterEffect(string text)
    {
        isTyping = true;
        dialogText.text = "";

        for (int i = 0; i <= text.Length; i++)
        {
            dialogText.text = text.Substring(0, i);
            yield return new UnityEngine.WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
        ShowOptions();
    }

    private void ShowOptions()
    {
        ClearOptions();

        if (currentNode.isEndDialog || currentNode.optionsList.Count == 0)
        {
            waitingForClick = true;
            return;
        }

        int normalOptionIndex = 0;

        for (int i = 0; i < currentNode.optionsList.Count; i++)
        {
            DialogOption option = currentNode.optionsList[i];

            Transform parentContainer;

            // "..." gibi sessiz cevaplar için ayrı konum kullan
            if (option.isSilentOption && silentOptionButtonRef != null)
            {
                parentContainer = silentOptionButtonRef;
            }
            else
            {
                // Normal cevaplar için sıralı konumları kullan
                if (normalOptionIndex < optButtonRef.Count)
                {
                    parentContainer = optButtonRef[normalOptionIndex];
                    normalOptionIndex++;
                }
                else
                {
                    parentContainer = optionsContainer;
                }
            }

            GameObject optionButton = Instantiate(optionButtonPrefab, parentContainer);

            string optionText = option.optionText.GetText(isTurkish);
            optionButton.GetComponentInChildren<TextMeshProUGUI>().text = optionText;

            // Hover efekti için DialogOptionButton ekle
            if (optionButton.GetComponent<DialogOptionButton>() == null)
            {
                optionButton.AddComponent<DialogOptionButton>();
            }

            DialogOption capturedOption = option;
            optionButton.GetComponent<Button>().onClick.AddListener(() => OnOptionSelected(capturedOption));

            currentOptionButtons.Add(optionButton);
        }

    }

    private void OnOptionSelected(DialogOption option)
    {
        ClearOptions();

        // Eğer typewriter efekti hala çalışıyorsa, önce onu tamamla
        if (isTyping)
        {
            StopAllCoroutines();
            dialogText.text = currentNode.dialogText.GetText(isTurkish);
            isTyping = false;
        }

        option.onSelect?.Invoke();

        // Response node varsa göster
        if (option.responseNode != null)
        {
            currentNode = option.responseNode;
            ShowDialog();
        }
        else
        {
            // Response node null ise dialogu kapat
            EndDialog();
        }

    }


    private void ClearOptions()
    {
        foreach (GameObject button in currentOptionButtons)
        {
            Destroy(button);
        }
        currentOptionButtons.Clear();
    }

    public void EndDialog()
    {
        dialogPanel.SetActive(false);
        waitingForClick = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        EnablePlayerMovement();

        ClearOptions();
    }

    private void DisablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }
    }

    private void EnablePlayerMovement()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    public void SetLanguage(bool turkish)
    {
        isTurkish = turkish;
    }

    public bool GetCurrentLanguage()
    {
        return isTurkish;
    }
}