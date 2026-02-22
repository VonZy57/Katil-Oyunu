using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JumpableObstacle : MonoBehaviour
{
    [Header("Referanslar")]
    public EndlessRunner endlessRunner;

    [Header("UI")]
    public GameObject promptUI; // "Press Space to Jump" yazısı için UI objesi
    public TextMeshProUGUI promptText;

    [Header("Jump Ayarları")]
    public float jumpForce = 8f;
    public float timeToReact = 1.5f; // Kaç saniye içinde basmalı

    [Header("Trigger Ayarları")]
    public Collider promptTrigger; // Prompt'un gösterileceği alan
    public Collider failTrigger;   // Atlamayı başaramazsa fail olacağı alan

    // Durum
    private bool isInPromptZone = false;
    private bool hasJumped = false;
    private float promptTimer = 0f;
    private CharacterController characterController;
    private Vector3 jumpVelocity;
    private bool isJumping = false;

    // Input
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.SpaceButton.performed += ctx => OnJumpPressed();
    }

    void OnEnable() => controls?.Player.Enable();
    void OnDisable() => controls?.Player.Disable();

    void Start()
    {
        if (endlessRunner == null)
            endlessRunner = FindFirstObjectByType<EndlessRunner>();

        if (endlessRunner != null && endlessRunner.characterController != null)
            characterController = endlessRunner.characterController;

        // UI'ı başta kapat
        if (promptUI != null)
            promptUI.SetActive(false);

        // Trigger'lara helper ekle
        SetupTrigger(promptTrigger, true);
        SetupTrigger(failTrigger, false);
    }

    void SetupTrigger(Collider trigger, bool isPromptTrigger)
    {
        if (trigger == null) return;

        var helper = trigger.gameObject.AddComponent<JumpableObstacleTriggerHelper>();
        helper.jumpableObstacle = this;
        helper.isPromptTrigger = isPromptTrigger;
    }

    void Update()
    {
        // Jump sırasında yerçekimi uygula
        if (isJumping && characterController != null)
        {
            jumpVelocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(jumpVelocity * Time.deltaTime);

            if (characterController.isGrounded && jumpVelocity.y < 0)
            {
                isJumping = false;
                jumpVelocity = Vector3.zero;
            }
        }

        // Prompt zone'dayken timer'ı çalıştır
        if (isInPromptZone && !hasJumped)
        {
            promptTimer += Time.deltaTime;

            // Süre dolduysa fail
            if (promptTimer >= timeToReact)
            {
                OnFail();
            }
        }
    }

    void OnJumpPressed()
    {
        if (!isInPromptZone || hasJumped) return;

        // Başarılı zıplama
        hasJumped = true;
        HidePrompt();
        PerformJump();
    }

    void PerformJump()
    {
        if (characterController == null) return;

        isJumping = true;
        jumpVelocity = new Vector3(0f, jumpForce, 0f);
    }

    public void OnPromptTriggerEnter()
    {
        if (hasJumped) return;

        isInPromptZone = true;
        promptTimer = 0f;
        ShowPrompt();
    }

    public void OnPromptTriggerExit()
    {
        if (hasJumped) return;

        isInPromptZone = false;
        HidePrompt();
    }

    public void OnFailTriggerEnter()
    {
        if (hasJumped) return;

        OnFail();
    }

    void OnFail()
    {
        isInPromptZone = false;
        HidePrompt();

        // EndlessRunner'a fail bildir
        if (endlessRunner != null)
            endlessRunner.OnObstacleHit();

        // Reset
        ResetObstacle();
    }

    void ShowPrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(true);

        if (promptText != null)
            promptText.text = "Press SPACE to Jump";
    }

    void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public void ResetObstacle()
    {
        hasJumped = false;
        isInPromptZone = false;
        promptTimer = 0f;
        isJumping = false;
        jumpVelocity = Vector3.zero;
        HidePrompt();
    }
}

public class JumpableObstacleTriggerHelper : MonoBehaviour
{
    [HideInInspector] public JumpableObstacle jumpableObstacle;
    [HideInInspector] public bool isPromptTrigger;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (jumpableObstacle == null) return;

        if (isPromptTrigger)
            jumpableObstacle.OnPromptTriggerEnter();
        else
            jumpableObstacle.OnFailTriggerEnter();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (jumpableObstacle == null) return;

        if (isPromptTrigger)
            jumpableObstacle.OnPromptTriggerExit();
    }
}
