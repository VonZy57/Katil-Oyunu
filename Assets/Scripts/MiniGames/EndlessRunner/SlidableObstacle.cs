using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlidableObstacle : MonoBehaviour
{
    [Header("Referanslar")]
    public EndlessRunner endlessRunner;

    [Header("UI")]
    public GameObject promptUI;
    public TextMeshProUGUI promptText;

    [Header("Slide Ayarları")]
    public float slideDuration = 0.5f;
    public float cameraDipAmount = 0.6f;
    public float cameraDownSpeed = 0.1f;
    public float cameraUpSpeed = 0.25f;
    public float timeToReact = 1.5f;

    [Header("Trigger Ayarları")]
    public Collider promptTrigger;
    public Collider failTrigger;

    [Header("Grace / Collision Ayarları")]
    public Collider obstacleCollider;
    public float failGraceDuration = 0.4f;
    public float colliderDisableDuration = 2f;

    // Durum
    private bool isInPromptZone = false;
    private bool isInFailZone = false;
    private bool hasSlid = false;
    private float promptTimer = 0f;
    private float failGraceTimer = 0f;
    private Tween slideTween;

    // Input
    private PlayerControls controls;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Player.Crouch.performed += ctx => OnSlidePressed();
    }

    void OnEnable() => controls?.Player.Enable();
    void OnDisable() => controls?.Player.Disable();

    void Start()
    {
        if (endlessRunner == null)
            endlessRunner = FindFirstObjectByType<EndlessRunner>();

        if (promptUI != null)
            promptUI.SetActive(false);

        SetupTrigger(promptTrigger, true);
        SetupTrigger(failTrigger, false);
    }

    void SetupTrigger(Collider trigger, bool isPromptTrigger)
    {
        if (trigger == null) return;
        var helper = trigger.gameObject.AddComponent<SlidableObstacleTriggerHelper>();
        helper.slidableObstacle = this;
        helper.isPromptTrigger = isPromptTrigger;
    }

    void Update()
    {
        if (isInPromptZone && !hasSlid)
        {
            promptTimer += Time.deltaTime;
            if (promptTimer >= timeToReact)
                OnFail();
        }

        if (isInFailZone && !hasSlid)
        {
            failGraceTimer += Time.deltaTime;
            if (failGraceTimer >= failGraceDuration)
                OnFail();
        }

        // S tuşu desteği
        if (Keyboard.current != null && Keyboard.current.sKey.wasPressedThisFrame)
            OnSlidePressed();
    }

    void OnSlidePressed()
    {
        if (hasSlid) return;
        if (!isInPromptZone && !isInFailZone) return;

        hasSlid = true;
        isInFailZone = false;
        HidePrompt();
        PerformSlide();

        if (obstacleCollider != null)
            StartCoroutine(DisableColliderTemporarily(obstacleCollider, colliderDisableDuration));
    }

    System.Collections.IEnumerator DisableColliderTemporarily(Collider col, float duration)
    {
        col.enabled = false;
        yield return new WaitForSeconds(duration);
        col.enabled = true;
    }

    void PerformSlide()
    {
        if (endlessRunner == null) return;

        slideTween?.Kill();
        slideTween = DOTween.To(
            () => endlessRunner.cameraSlideOffsetY,
            x => endlessRunner.cameraSlideOffsetY = x,
            -cameraDipAmount,
            cameraDownSpeed
        ).SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            slideTween = DOVirtual.DelayedCall(slideDuration, () =>
            {
                slideTween = DOTween.To(
                    () => endlessRunner.cameraSlideOffsetY,
                    x => endlessRunner.cameraSlideOffsetY = x,
                    0f,
                    cameraUpSpeed
                ).SetEase(Ease.InOutSine);
            });
        });
    }

    public void OnPromptTriggerEnter()
    {
        if (hasSlid) return;
        isInPromptZone = true;
        promptTimer = 0f;
        ShowPrompt();
    }

    public void OnPromptTriggerExit()
    {
        if (hasSlid) return;
        isInPromptZone = false;
        HidePrompt();
    }

    public void OnFailTriggerEnter()
    {
        if (hasSlid) return;
        isInFailZone = true;
        failGraceTimer = 0f;
    }

    void OnFail()
    {
        isInPromptZone = false;
        isInFailZone = false;
        hasSlid = true;

        if (endlessRunner != null)
            endlessRunner.OnObstacleHit();
    }

    void ShowPrompt()
    {
        if (promptUI != null) promptUI.SetActive(true);
        if (promptText != null) promptText.text = "Press Ctrl / S to Slide";
    }

    void HidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    public void ResetObstacle()
    {
        hasSlid = false;
        isInPromptZone = false;
        isInFailZone = false;
        promptTimer = 0f;
        failGraceTimer = 0f;
        if (failTrigger != null) failTrigger.enabled = true;
        if (obstacleCollider != null) obstacleCollider.enabled = true;
        HidePrompt();

        slideTween?.Kill();
        slideTween = null;
        if (endlessRunner != null)
            endlessRunner.cameraSlideOffsetY = 0f;
    }
}

public class SlidableObstacleTriggerHelper : MonoBehaviour
{
    [HideInInspector] public SlidableObstacle slidableObstacle;
    [HideInInspector] public bool isPromptTrigger;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (slidableObstacle == null) return;

        if (isPromptTrigger)
            slidableObstacle.OnPromptTriggerEnter();
        else
            slidableObstacle.OnFailTriggerEnter();
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (slidableObstacle == null) return;

        if (isPromptTrigger)
            slidableObstacle.OnPromptTriggerExit();
    }
}
