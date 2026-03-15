using DG.Tweening;
using TMPro;
using UnityEngine;

public class SlidableObstacle : MonoBehaviour
{
    [Header("Referanslar")]
    public EndlessRunner endlessRunner;

    [Header("UI")]
    public GameObject promptUI;
    public TextMeshProUGUI promptText;

    [Header("Slide Ayarları")]
    public float slideDuration = 1f;      // Slide süresi
    public float cameraDipAmount = 0.6f;    // Kamera kaç birim aşağı inecek
    public float cameraDownSpeed = 0.1f;    // İniş hızı
    public float cameraUpSpeed = 0.25f;     // Çıkış hızı
    public float timeToReact = 1.5f;        // Kaç saniye içinde basmalı

    [Header("Trigger Ayarları")]
    public Collider promptTrigger;
    public Collider failTrigger;

    // Durum
    private bool isInPromptZone = false;
    private bool hasSlid = false;
    private float promptTimer = 0f;
    private Vector3 originalCameraLocalPos;
    private float originalCCHeight;
    private Vector3 originalCCCenter;

    void Start()
    {
        if (endlessRunner == null)
            endlessRunner = FindFirstObjectByType<EndlessRunner>();

        if (endlessRunner != null && endlessRunner.playerCamera != null)
            originalCameraLocalPos = endlessRunner.playerCamera.localPosition;

        if (endlessRunner != null && endlessRunner.characterController != null)
        {
            originalCCHeight = endlessRunner.characterController.height;
            originalCCCenter = endlessRunner.characterController.center;
        }

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
            if (Input.GetKeyDown(KeyCode.S))
            {
                hasSlid = true;
                HidePrompt();
                PerformSlide();
                return;
            }

            promptTimer += Time.deltaTime;
            if (promptTimer >= timeToReact)
                OnFail();
        }
    }

    void PerformSlide()
    {
        if (endlessRunner?.playerCamera == null) return;

        // CC'yi kamera ile orantılı küçült
        var cc = endlessRunner.characterController;
        if (cc != null)
        {
            float newHeight = originalCCHeight - cameraDipAmount;
            cc.height = Mathf.Max(newHeight, 0.1f);
            cc.center = new Vector3(originalCCCenter.x, cc.height / 2f, originalCCCenter.z);
        }

        // Kamerayı aşağı indir, sonra geri çıkar
        float targetY = originalCameraLocalPos.y - cameraDipAmount;
        endlessRunner.playerCamera.DOLocalMoveY(targetY, cameraDownSpeed)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                DOVirtual.DelayedCall(slideDuration, () =>
                {
                    endlessRunner.playerCamera.DOLocalMoveY(originalCameraLocalPos.y, cameraUpSpeed)
                        .SetEase(Ease.OutQuad);

                    // CC'yi geri büyüt
                    if (cc != null)
                    {
                        cc.height = originalCCHeight;
                        cc.center = originalCCCenter;
                    }
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
        OnFail();
    }

    void OnFail()
    {
        isInPromptZone = false;
        HidePrompt();

        if (endlessRunner != null)
            endlessRunner.OnObstacleHit();

        ResetObstacle();
    }

    void ShowPrompt()
    {
        if (promptUI != null) promptUI.SetActive(true);
        if (promptText != null) promptText.text = "Press S to Slide";
    }

    void HidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    public void ResetObstacle()
    {
        hasSlid = false;
        isInPromptZone = false;
        promptTimer = 0f;
        HidePrompt();

        if (endlessRunner?.playerCamera != null)
        {
            endlessRunner.playerCamera.DOKill();
            endlessRunner.playerCamera.DOLocalMoveY(originalCameraLocalPos.y, cameraUpSpeed);
        }

        var cc = endlessRunner?.characterController;
        if (cc != null)
        {
            cc.height = originalCCHeight;
            cc.center = originalCCCenter;
        }
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
