using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JumpableObstacle : MonoBehaviour
{
    [Header("Referanslar")]
    public EndlessRunner endlessRunner;

    [Header("UI")]
    public GameObject promptUI;
    public TextMeshProUGUI promptText;

    [Header("Jump Ayarları")]
    public float jumpForce = 8f;
    public float jumpForwardForce = 4f;
    public float timeToReact = 1.5f;

    [Header("Trigger Ayarları")]
    public Collider promptTrigger;
    public Collider failTrigger;

    [Header("Grace / Collision Ayarları")]
    public Collider obstacleCollider;          // Oyuncuyu fiziksel olarak durduran collider
    public float failGraceDuration = 0.4f;     // Fail zone içinde tuşa basma süresi
    public float colliderDisableDuration = 1f; // Başarılı basışta collider kapalı kalma süresi

    // Durum
    private bool isInPromptZone = false;
    private bool isInFailZone = false;
    private bool hasJumped = false;
    private float promptTimer = 0f;
    private float failGraceTimer = 0f;
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

        if (promptUI != null)
            promptUI.SetActive(false);

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

        if (isInPromptZone && !hasJumped)
        {
            promptTimer += Time.deltaTime;
            if (promptTimer >= timeToReact)
                OnFail();
        }

        if (isInFailZone && !hasJumped)
        {
            failGraceTimer += Time.deltaTime;
            if (failGraceTimer >= failGraceDuration)
                OnFail();
        }
    }

    void OnJumpPressed()
    {
        if (hasJumped) return;
        if (!isInPromptZone && !isInFailZone) return;

        hasJumped = true;
        isInFailZone = false;
        HidePrompt();
        PerformJump();

        if (obstacleCollider != null)
            StartCoroutine(DisableColliderTemporarily(obstacleCollider, colliderDisableDuration));
    }

    System.Collections.IEnumerator DisableColliderTemporarily(Collider col, float duration)
    {
        col.enabled = false;
        yield return new WaitForSeconds(duration);
        col.enabled = true;
    }

    void PerformJump()
    {
        if (characterController == null) return;

        isJumping = true;
        Vector3 forward = endlessRunner != null ? endlessRunner.playerBody.forward : Vector3.forward;
        jumpVelocity = forward * jumpForwardForce + Vector3.up * jumpForce;
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

        isInFailZone = true;
        failGraceTimer = 0f;
    }

    void OnFail()
    {
        isInPromptZone = false;
        isInFailZone = false;
        hasJumped = true;

        if (endlessRunner != null)
            endlessRunner.OnObstacleHit();
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
        isInFailZone = false;
        promptTimer = 0f;
        failGraceTimer = 0f;
        isJumping = false;
        jumpVelocity = Vector3.zero;
        if (failTrigger != null) failTrigger.enabled = true;
        if (obstacleCollider != null) obstacleCollider.enabled = true;
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
