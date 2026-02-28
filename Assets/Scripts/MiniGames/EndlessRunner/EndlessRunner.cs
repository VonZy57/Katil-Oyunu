using UnityEngine;
using DG.Tweening;
using System.Collections;

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

    [Header("Head Bob Ayarları")]
    public float headBobFrequencyBetween = 4f;
    public float headBobAmplitudeBetween = 0.05f;
    public float headBobHorizontalAmpBetween = 0.05f;

    [Header("Trigger Referansları")]
    public Collider firstTriggerZone;
    public Collider secondTriggerZone;

    [Header("Kararma Efekti")]
    public GameObject blackScreenObject;

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

    // Obstacle cache
    private GameObject[] cachedObstacles;
    private JumpableObstacle[] cachedJumpableObstacles;

    // Baz hız (2. trigger sonrası 1.5x için)
    private float baseWalkSpeed;

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

        baseWalkSpeed = walkSpeed;

        // Obstacle'ları cache'e al ve gizle
        cachedObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        cachedJumpableObstacles = FindObjectsByType<JumpableObstacle>(FindObjectsSortMode.None);
        SetObstaclesActive(false);
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
        TakeControlOfPlayer(IsLookingAtInteractable());
    }

    bool IsLookingAtInteractable()
    {
        if (playerCamera == null) return false;
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        return Physics.Raycast(ray, out RaycastHit hit, 10f) && hit.collider.CompareTag("Interactable");
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
        if (!canUseInputAndHeadBob) return;
        StartCoroutine(ObstacleHitSequence());
    }

    private IEnumerator ObstacleHitSequence()
    {
        canUseInputAndHeadBob = false;

        blackScreenObject?.SetActive(true);

        TeleportToSecondTrigger();

        if (cachedJumpableObstacles != null)
            foreach (var jo in cachedJumpableObstacles)
                jo?.ResetObstacle();

        yield return new WaitForSeconds(0.5f);

        blackScreenObject?.SetActive(false);

        canUseInputAndHeadBob = true;
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

    void TakeControlOfPlayer(bool rotate180 = false)
    {
        isControllingPlayer = true;
        canUseInputAndHeadBob = false;

        if (firstPersonController != null)
            firstPersonController.enabled = false;

        if (playerCamera != null)
            playerCamera.DOLocalRotate(Vector3.zero, rotationDuration).SetEase(Ease.OutQuad);

        if (rotate180 && playerBody != null)
        {
            isRotating = true;
            Vector3 targetRotation = playerBody.eulerAngles + new Vector3(0f, 180f, 0f);
            playerBody.DORotate(targetRotation, rotationDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => isRotating = false);
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

        float frequency = headBobFrequencyBetween;
        float amplitude = headBobAmplitudeBetween;
        float horizontalAmp = headBobHorizontalAmpBetween;

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

        SetObstaclesActive(true);

        yield return new WaitForSeconds(0.5f);

        ReturnToOriginal();
        yield return new WaitForSeconds(rotationDuration);

        isRotating = false;
        canUseInputAndHeadBob = true;
        walkSpeed = baseWalkSpeed * 1.5f;
    }

    void SetObstaclesActive(bool active)
    {
        if (cachedObstacles == null) return;
        foreach (var obj in cachedObstacles)
            obj?.SetActive(active);
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
        walkSpeed = baseWalkSpeed;
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
