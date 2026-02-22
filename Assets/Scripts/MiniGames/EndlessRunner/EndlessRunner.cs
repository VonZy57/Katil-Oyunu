using UnityEngine;
using DG.Tweening;

public class EndlessRunner : MonoBehaviour
{
    [Header("Referanslar")]
    public Transform playerBody;
    public Transform playerCamera;
    public CharacterController characterController;
    public FirstPersonController firstPersonController;

    [Header("Hareket Ayarları")]
    public float walkSpeed = 3f;
    public float gravity = -9.81f;

    [Header("Dönüş Ayarları")]
    public float rotationDuration = 1.5f;

    [Header("Head Bob Ayarları (1. ve 2. Trigger Arası)")]
    public float headBobFrequencyBetween = 4f;
    public float headBobAmplitudeBetween = 0.05f;
    public float headBobHorizontalAmpBetween = 0.05f;

    [Header("Head Bob Ayarları (2. Trigger Sonrası)")]
    public float headBobFrequencyAfter = 1.5f;
    public float headBobAmplitudeAfter = 0.05f;
    public float headBobHorizontalAmpAfter = 0.05f;

    [Header("Trigger Referansları")]
    public Collider firstTriggerZone;
    public Collider secondTriggerZone;

    // Durum takibi
    private bool isControllingPlayer = false;
    private bool hasReachedSecondTrigger = false;
    private bool isRotating = false;
    private bool canUseInputAndHeadBob = false;

    // Orijinal rotasyon
    private Quaternion startBodyRotation;

    // Yerçekimi için dahili hız
    private Vector3 velocity;

    // A/D input için
    private PlayerControls controls;
    private Vector2 moveInput;

    // Head bob için
    private float headBobTimer = 0f;
    private Vector3 initialCameraPosition;

    // 2. trigger pozisyonu (respawn için)
    private Vector3 secondTriggerPosition;
    private Quaternion secondTriggerRotation;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    void OnEnable() => controls?.Player.Enable();
    void OnDisable() => controls?.Player.Disable();

    void Start()
    {
        if (firstPersonController == null)
            firstPersonController = FindFirstObjectByType<FirstPersonController>();

        if (characterController == null && firstPersonController != null)
            characterController = firstPersonController.GetComponent<CharacterController>();

        if (playerBody == null && firstPersonController != null)
            playerBody = firstPersonController.transform;

        if (playerCamera == null && firstPersonController != null)
            playerCamera = firstPersonController.cameraTransform;

        if (playerCamera != null)
            initialCameraPosition = playerCamera.localPosition;

        SetupTriggerZone(firstTriggerZone, true);
        SetupTriggerZone(secondTriggerZone, false);

        // Player'a obstacle collision helper ekle
        if (playerBody != null)
            SetupObstacleDetection();
    }

    void SetupTriggerZone(Collider triggerCollider, bool isFirstTrigger)
    {
        if (triggerCollider == null) return;

        var existingHelper = triggerCollider.GetComponent<EndlessRunnerTriggerHelper>();
        if (existingHelper != null)
        {
            existingHelper.endlessRunner = this;
            existingHelper.isFirstTrigger = isFirstTrigger;
            return;
        }

        var helper = triggerCollider.gameObject.AddComponent<EndlessRunnerTriggerHelper>();
        helper.endlessRunner = this;
        helper.isFirstTrigger = isFirstTrigger;
    }

    void SetupObstacleDetection()
    {
        var existingHelper = playerBody.GetComponent<EndlessRunnerObstacleHelper>();
        if (existingHelper != null)
        {
            existingHelper.endlessRunner = this;
            return;
        }

        var helper = playerBody.gameObject.AddComponent<EndlessRunnerObstacleHelper>();
        helper.endlessRunner = this;
    }

    void Update()
    {
        if (isControllingPlayer && !isRotating)
        {
            MovePlayerForward();
            HandleHeadBob();
        }
    }

    public void OnFirstTriggerEnter()
    {
        if (isControllingPlayer) return;
        TakeControlOfPlayer();
    }

    public void OnSecondTriggerEnter()
    {
        if (!isControllingPlayer || hasReachedSecondTrigger) return;

        // 2. trigger pozisyonunu kaydet (respawn için)
        secondTriggerPosition = playerBody.position;
        secondTriggerRotation = playerBody.rotation;

        hasReachedSecondTrigger = true;
        StartCoroutine(HandleSecondTrigger());
    }

    public void OnObstacleHit()
    {
        if (!canUseInputAndHeadBob) return; // Sadece 2. trigger'dan sonra aktif

        // Oyuncuyu 2. trigger pozisyonuna geri götür
        TeleportToSecondTrigger();
    }

    void TeleportToSecondTrigger()
    {
        if (characterController == null || secondTriggerZone == null) return;

        // CharacterController'ı geçici olarak kapat (teleport için gerekli)
        characterController.enabled = false;

        // Pozisyonu ve rotasyonu ayarla
        playerBody.position = secondTriggerPosition;
        playerBody.rotation = secondTriggerRotation;

        // Kamerayı sıfırla
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.identity;

        // Velocity'yi sıfırla
        velocity = Vector3.zero;

        // CharacterController'ı tekrar aç
        characterController.enabled = true;
    }

    void TakeControlOfPlayer()
    {
        isControllingPlayer = true;
        canUseInputAndHeadBob = false;

        if (firstPersonController != null)
            firstPersonController.enabled = false;

        if (playerBody != null)
        {
            Vector3 targetRotation = new Vector3(0f, -90f, 0f);
            playerBody.DORotate(targetRotation, rotationDuration).SetEase(Ease.OutQuad);
        }

        if (playerCamera != null)
        {
            playerCamera.DOLocalRotate(Vector3.zero, rotationDuration).SetEase(Ease.OutQuad);
        }
    }

    void ReleaseControlOfPlayer()
    {
        isControllingPlayer = false;

        if (firstPersonController != null)
            firstPersonController.enabled = true;
    }

    void MovePlayerForward()
    {
        if (characterController == null || !characterController.enabled) return;

        Vector3 forward = playerBody.forward * walkSpeed;

        Vector3 strafe = Vector3.zero;
        if (canUseInputAndHeadBob)
            strafe = playerBody.right * moveInput.x * walkSpeed;

        Vector3 move = (forward + strafe) * Time.deltaTime;
        characterController.Move(move);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    void HandleHeadBob()
    {
        if (playerCamera == null) return;

        float frequency = canUseInputAndHeadBob ? headBobFrequencyAfter : headBobFrequencyBetween;
        float amplitude = canUseInputAndHeadBob ? headBobAmplitudeAfter : headBobAmplitudeBetween;
        float horizontalAmp = canUseInputAndHeadBob ? headBobHorizontalAmpAfter : headBobHorizontalAmpBetween;

        if (characterController.isGrounded)
        {
            headBobTimer += Time.deltaTime * walkSpeed * frequency;
            float bobOffsetY = Mathf.Sin(headBobTimer) * amplitude;
            float bobOffsetX = Mathf.Cos(headBobTimer / 2) * horizontalAmp;
            playerCamera.localPosition = initialCameraPosition + new Vector3(bobOffsetX, bobOffsetY, 0);
        }
        else
        {
            headBobTimer = 0f;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, initialCameraPosition, Time.deltaTime * 5f);
        }
    }

    private System.Collections.IEnumerator HandleSecondTrigger()
    {
        isRotating = true;

        if (playerBody != null) startBodyRotation = playerBody.rotation;

        RotateBackwards();
        yield return new WaitForSeconds(rotationDuration);

        yield return new WaitForSeconds(0.5f);

        ReturnToOriginal();
        yield return new WaitForSeconds(rotationDuration);

        isRotating = false;
        canUseInputAndHeadBob = true;
    }

    void RotateBackwards()
    {
        if (playerBody != null)
        {
            Vector3 targetRotation = playerBody.eulerAngles + new Vector3(0f, 180f, 0f);
            playerBody.DORotate(targetRotation, rotationDuration).SetEase(Ease.OutQuad);
        }
    }

    void ReturnToOriginal()
    {
        if (playerBody != null)
        {
            playerBody.DORotateQuaternion(startBodyRotation, rotationDuration).SetEase(Ease.OutQuad);
        }
    }

    public void StopEndlessRunner()
    {
        ReleaseControlOfPlayer();
        hasReachedSecondTrigger = false;
        isRotating = false;
    }
}

public class EndlessRunnerTriggerHelper : MonoBehaviour
{
    [HideInInspector] public EndlessRunner endlessRunner;
    [HideInInspector] public bool isFirstTrigger;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (endlessRunner == null) return;

        if (isFirstTrigger)
            endlessRunner.OnFirstTriggerEnter();
        else
            endlessRunner.OnSecondTriggerEnter();
    }
}

public class EndlessRunnerObstacleHelper : MonoBehaviour
{
    [HideInInspector] public EndlessRunner endlessRunner;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag("Obstacle"))
        {
            if (endlessRunner != null)
                endlessRunner.OnObstacleHit();
        }
    }
}
