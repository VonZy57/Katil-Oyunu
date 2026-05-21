using DG.Tweening;
using TMPro;
using UnityEngine;

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
    public float returnRotationDuration = 0.5f;

    [Header("Head Bob Ayarları")]
    public float headBobFrequencyBetween = 4f;
    public float headBobAmplitudeBetween = 0.05f;
    public float headBobHorizontalAmpBetween = 0.05f;

    [Header("Trigger Referansları")]
    public Collider firstTriggerZone;
    public Collider secondTriggerZone;

    [Header("Kararma Efekti")]
    public GameObject blackScreenObject;

    [Header("Ses Ayarları")]
    public AudioClip turnBackSound;
    public AudioClip obstacleHitSound;
    public AudioSource audioSource;

    [Header("Altyazı Ayarları")]
    public TextMeshProUGUI subtitleText;
    public DialogSystem dialogSystem;

    [Header("Kapı Dönüş Sistemi")]
    public float doorPromptTimeout = 2f;
    public float doorSlowSpeed = 1f;
    [Tooltip("Trigger'a girince kaç saniye yavaşlar")]
    public float doorSlowDuration = 0.8f;
    public GameObject promptLeft;
    public GameObject promptRight;
    public GameObject promptOpenDoor;

    [Header("FOV Ayarları")]
    public float normalFOV = 60f;
    public float runFOV = 75f;
    public float fovChangeDuration = 0.3f;

    [Header("Sis Ayarları")]
    public bool useFog = true;
    public Color fogColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    public FogMode fogMode = FogMode.Linear;
    [Tooltip("Koşarken sisin başladığı mesafe (metre)")]
    public float fogStartDistance = 5f;
    [Tooltip("Koşarken sisin tamamen kapattığı mesafe (metre)")]
    public float fogEndDistance = 20f;
    [Tooltip("Exponential mod için yoğunluk")]
    public float fogDensity = 0.05f;

    [Header("Karanlık Boşluk Sisi (Arkaya Dönünce)")]
    [Tooltip("Void sisin başladığı mesafe — 0 yapılırsa anında kapanır")]
    public float voidFogStartDistance = 0f;
    [Tooltip("Void sisin bittiği mesafe — çok küçük yapılırsa tam karanlık")]
    public float voidFogEndDistance = 2f;
    public Color voidFogColor = new Color(0f, 0f, 0f, 1f);

    // Orijinal sis ayarları (restore için)
    private bool originalFogEnabled;
    private Color originalFogColor;
    private float originalFogDensity;
    private FogMode originalFogMode;

    // Durum takibi
    private bool isControllingPlayer = false;
    private bool hasReachedSecondTrigger = false;
    private bool isRotating = false;

    private bool _canUseInputAndHeadBob = false;
    private bool CanUseInputAndHeadBob
    {
        get => _canUseInputAndHeadBob;
        set
        {
            _canUseInputAndHeadBob = value;
            if (playerCam != null)
            {
                fovTween?.Kill();
                fovTween = playerCam.DOFieldOfView(value ? runFOV : normalFOV, fovChangeDuration).SetEase(Ease.OutQuad);
            }
        }
    }

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
    [HideInInspector] public float cameraSlideOffsetY = 0f;

    // 2. trigger pozisyonu (respawn için)
    private Vector3 secondTriggerPosition;
    private Quaternion secondTriggerRotation;

    // FOV
    private Camera playerCam;
    private Tween fovTween;

    // Kapı dönüş durumu
    private bool isWaitingForDoorInput = false;
    private bool isWaitingForDoorOpen = false;
    private bool isDoorSlowed = false;
    private bool requireLeft;
    private Tween doorTimeoutTween;
    private DoorCheckpointTrigger currentDoorCheckpoint;
    private DoorCheckpointTrigger[] cachedDoorCheckpoints;

    // Obstacle cache
    private GameObject[] cachedObstacles;
    private JumpableObstacle[] cachedJumpableObstacles;
    private SlidableObstacle[] cachedSlidableObstacles;
    private FallingObject[] cachedFallingObjects;

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
        {
            initialCameraPosition = playerCamera.localPosition;
            playerCam = playerCamera.GetComponent<Camera>();
        }

        SetupTriggerZone(firstTriggerZone, true);
        SetupTriggerZone(secondTriggerZone, false);

        if (playerBody != null)
            SetupObstacleDetection();

        baseWalkSpeed = walkSpeed;

        cachedObstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        cachedJumpableObstacles = FindObjectsByType<JumpableObstacle>(FindObjectsSortMode.None);
        cachedSlidableObstacles = FindObjectsByType<SlidableObstacle>(FindObjectsSortMode.None);
        cachedFallingObjects = FindObjectsByType<FallingObject>(FindObjectsSortMode.None);
        SetObstaclesActive(false);

        cachedDoorCheckpoints = FindObjectsByType<DoorCheckpointTrigger>(FindObjectsSortMode.None);
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

        if (isWaitingForDoorInput)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                OnDoorInput(true);
            else if (Input.GetKeyDown(KeyCode.E))
                OnDoorInput(false);
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

        secondTriggerPosition = playerBody.position;
        secondTriggerRotation = playerBody.rotation;

        hasReachedSecondTrigger = true;
        HandleSecondTrigger();
    }

    public void OnObstacleHit()
    {
        if (!CanUseInputAndHeadBob) return;
        ObstacleHitSequence();
    }

    private void ObstacleHitSequence()
    {
        CanUseInputAndHeadBob = false;

        isWaitingForDoorInput = false;
        isWaitingForDoorOpen = false;
        isDoorSlowed = false;
        doorTimeoutTween?.Kill();
        doorTimeoutTween = null;
        HideDoorPrompts();
        if (promptOpenDoor) promptOpenDoor.SetActive(false);
        currentDoorCheckpoint?.TurnOffLights();
        currentDoorCheckpoint = null;

        if (obstacleHitSound != null && audioSource != null)
            audioSource.PlayOneShot(obstacleHitSound);

        blackScreenObject?.SetActive(true);
        TeleportToSecondTrigger();

        if (cachedJumpableObstacles != null)
            foreach (var jo in cachedJumpableObstacles)
                jo?.ResetObstacle();

        if (cachedSlidableObstacles != null)
            foreach (var so in cachedSlidableObstacles)
                so?.ResetObstacle();

        if (cachedFallingObjects != null)
            foreach (var fo in cachedFallingObjects)
                fo?.ResetRotation();

        ResetAllDoorCheckpoints();

        DOVirtual.DelayedCall(0.5f, () =>
        {
            blackScreenObject?.SetActive(false);
            CanUseInputAndHeadBob = true;
        });
    }

    void TeleportToSecondTrigger()
    {
        if (characterController == null || secondTriggerZone == null) return;

        characterController.enabled = false;
        playerBody.position = secondTriggerPosition;
        playerBody.rotation = secondTriggerRotation;

        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.identity;

        velocity = Vector3.zero;
        characterController.enabled = true;
    }

    void EnableVoidFog()
    {
        if (!useFog) return;
        originalFogEnabled = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogMode = RenderSettings.fogMode;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, voidFogColor, 0.3f);
        DOTween.To(() => RenderSettings.fogStartDistance, x => RenderSettings.fogStartDistance = x, voidFogStartDistance, 0.3f);
        DOTween.To(() => RenderSettings.fogEndDistance, x => RenderSettings.fogEndDistance = x, voidFogEndDistance, 0.3f);
    }

    void EnableFog()
    {
        if (!useFog) return;
        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;
        DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, fogColor, returnRotationDuration);
        DOTween.To(() => RenderSettings.fogStartDistance, x => RenderSettings.fogStartDistance = x, fogStartDistance, returnRotationDuration);
        DOTween.To(() => RenderSettings.fogEndDistance, x => RenderSettings.fogEndDistance = x, fogEndDistance, returnRotationDuration);
    }

    void DisableFog()
    {
        if (!useFog) return;
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogMode = originalFogMode;
    }

    void TakeControlOfPlayer()
    {
        isControllingPlayer = true;
        CanUseInputAndHeadBob = false;
        isRotating = true;

        if (firstPersonController != null)
            firstPersonController.enabled = false;

        if (playerCamera != null)
            playerCamera.DOLocalRotate(Vector3.zero, rotationDuration).SetEase(Ease.OutQuad);

        if (playerBody != null)
        {
            playerBody.DORotateQuaternion(Quaternion.Euler(0f, 90f, 0f), rotationDuration)
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

        bool doorSlow = isDoorSlowed || isWaitingForDoorInput || isWaitingForDoorOpen;
        float speed = doorSlow ? doorSlowSpeed : walkSpeed;
        Vector3 forward = playerBody.forward * speed;

        Vector3 strafe = Vector3.zero;
        if (CanUseInputAndHeadBob && !doorSlow)
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

        Vector3 slideBase = initialCameraPosition + Vector3.up * cameraSlideOffsetY;

        if (characterController.isGrounded)
        {
            headBobTimer += Time.deltaTime * walkSpeed * frequency;
            float bobOffsetY = Mathf.Sin(headBobTimer) * amplitude;
            float bobOffsetX = Mathf.Cos(headBobTimer / 2) * horizontalAmp;
            playerCamera.localPosition = slideBase + new Vector3(bobOffsetX, bobOffsetY, 0);
        }
        else
        {
            headBobTimer = 0f;
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, slideBase, Time.deltaTime * 5f);
        }
    }

    private void HandleSecondTrigger()
    {
        isRotating = true;
        if (playerBody != null) startBodyRotation = playerBody.rotation;

        RotateBackwards();
        EnableVoidFog();
        ShowTurnBackSubtitle();

        DOTween.Sequence()
            .AppendInterval(rotationDuration)
            .AppendCallback(() => SetObstaclesActive(true))
            .AppendInterval(0.5f)
            .AppendCallback(() =>
            {
                ReturnToOriginal();
                if (turnBackSound != null && audioSource != null)
                    audioSource.PlayOneShot(turnBackSound);
            })
            .AppendInterval(returnRotationDuration)
            .AppendCallback(() =>
            {
                isRotating = false;
                CanUseInputAndHeadBob = true;
                walkSpeed = baseWalkSpeed * 1.5f;
                EnableFog();
            });
    }

    void ShowTurnBackSubtitle()
    {
        if (subtitleText == null) return;

        bool isTurkish = false;
        if (dialogSystem != null)
            isTurkish = dialogSystem.GetCurrentLanguage();

        string trText = "Kaçma gel buraya babası kılıklı hain oğlum.";
        string enText = "Don't run away, you traitorous son, just like your father.";

        subtitleText.text = isTurkish ? trText : enText;

        DOVirtual.DelayedCall(1f, () =>
        {
            if (subtitleText != null)
                subtitleText.text = "";
        });
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
            Quaternion target = startBodyRotation * Quaternion.Euler(0f, 180f, 0f);
            playerBody.DORotateQuaternion(target, rotationDuration).SetEase(Ease.OutQuad);
        }
    }

    void ReturnToOriginal()
    {
        if (playerBody != null)
        {
            playerBody.DORotateQuaternion(startBodyRotation, returnRotationDuration).SetEase(Ease.OutQuad);
        }
    }

    public void SlowDownForExit(float speed)
    {
        walkSpeed = speed;
        CanUseInputAndHeadBob = false;
        isWaitingForDoorInput = false;
        isWaitingForDoorOpen = false;
        isDoorSlowed = false;
        doorTimeoutTween?.Kill();
        doorTimeoutTween = null;
        HideDoorPrompts();
    }

    public void StopEndlessRunner()
    {
        DisableFog();
        ReleaseControlOfPlayer();
        hasReachedSecondTrigger = false;
        isRotating = false;
        walkSpeed = baseWalkSpeed;

        cameraSlideOffsetY = 0f;

        if (playerCam != null)
        {
            fovTween?.Kill();
            playerCam.fieldOfView = normalFOV;
        }

        isWaitingForDoorInput = false;
        isWaitingForDoorOpen = false;
        isDoorSlowed = false;
        doorTimeoutTween?.Kill();
        doorTimeoutTween = null;
        HideDoorPrompts();
        if (promptOpenDoor) promptOpenDoor.SetActive(false);
        currentDoorCheckpoint?.TurnOffLights();
        currentDoorCheckpoint = null;
        ResetAllDoorCheckpoints();
    }

    public void OnDoorCheckpointReached(DoorCheckpointTrigger checkpoint, bool leftRequired)
    {
        if (!isControllingPlayer || isWaitingForDoorInput) return;

        currentDoorCheckpoint = checkpoint;
        requireLeft = leftRequired;
        isWaitingForDoorInput = true;
        isDoorSlowed = true;

        if (promptLeft) promptLeft.SetActive(true);
        if (promptRight) promptRight.SetActive(true);

        doorTimeoutTween = DOVirtual.DelayedCall(doorPromptTimeout, () =>
        {
            if (isWaitingForDoorInput)
            {
                isWaitingForDoorInput = false;
                DoorFailSequence();
            }
        });

        DOVirtual.DelayedCall(doorSlowDuration, () => isDoorSlowed = false);
    }

    void HideDoorPrompts()
    {
        if (promptLeft) promptLeft.SetActive(false);
        if (promptRight) promptRight.SetActive(false);
    }

    void OnDoorInput(bool pressedLeft)
    {
        if (!isWaitingForDoorInput) return;

        isWaitingForDoorInput = false;
        HideDoorPrompts();

        float angle = pressedLeft ? -90f : 90f;
        isRotating = true;
        Vector3 target = playerBody.eulerAngles + new Vector3(0f, angle, 0f);
        playerBody.DORotate(target, 0.35f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => isRotating = false);

        if (pressedLeft == requireLeft)
        {
            doorTimeoutTween?.Kill();
            doorTimeoutTween = null;
            currentDoorCheckpoint?.OpenCorrectDoor();
            currentDoorCheckpoint?.TurnOffLights();
            currentDoorCheckpoint = null;
        }
    }

    public void OnDoorOpenZoneEntered(bool isLeftDoor)
    {
        if (currentDoorCheckpoint == null || isWaitingForDoorOpen) return;
        if (isLeftDoor != requireLeft) return;

        isWaitingForDoorOpen = false;
        isDoorSlowed = false;

        currentDoorCheckpoint?.OpenCorrectDoor();
        currentDoorCheckpoint?.TurnOffLights();
        currentDoorCheckpoint = null;
    }

    private void DoorFailSequence()
    {
        CanUseInputAndHeadBob = false;
        HideDoorPrompts();
        currentDoorCheckpoint?.TurnOffLights();
        currentDoorCheckpoint = null;

        blackScreenObject?.SetActive(true);
        ResetAllDoorCheckpoints();
        TeleportToSecondTrigger();

        DOVirtual.DelayedCall(0.5f, () =>
        {
            blackScreenObject?.SetActive(false);
            CanUseInputAndHeadBob = true;
        });
    }

    void ResetAllDoorCheckpoints()
    {
        if (cachedDoorCheckpoints == null) return;
        foreach (var dc in cachedDoorCheckpoints)
        {
            if (dc != null)
                dc.ResetCheckpoint();
        }
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
