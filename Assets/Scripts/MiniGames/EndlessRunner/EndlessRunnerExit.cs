using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndlessRunnerExit : MonoBehaviour
{
    [Header("Referanslar")]
    public EndlessRunner endlessRunner;

    [Header("Trigger'lar")]
    public Collider entryTrigger;
    public Collider exitTrigger;

    [Header("Sahne")]
    public int sceneToLoad = 2;

    [Header("Geçiş Ayarları")]
    public Image fadeImage; // Ekranı kaplayan siyah UI resmi
    public float fadeDuration = 2f; // Kararma süresi

    [Header("Yürüyüş Ayarları")]
    public float slowWalkSpeed = 1.2f;
    public float centeringDuration = 0.6f;
    public Transform hallwayCenter;

    [Header("Kapı")]
    public GameObject door;
    public float doorOpenAngle = 220f;
    public float doorOpenDuration = 1.2f;

    [Header("FOV")]
    public float normalFOV = 60f;
    public float fovChangeDuration = 0.5f;

    // Dahili
    private bool isActive = false;
    private bool isExitActive = false;
    private AsyncOperation loadOperation;
    private Camera playerCam;
    private CharacterController characterController;
    private Transform playerBody;

    void Start()
    {
        if (endlessRunner != null)
        {
            characterController = endlessRunner.characterController;
            playerBody = endlessRunner.playerBody;
            var playerCamera = endlessRunner.playerCamera;
            if (playerCamera != null)
                playerCam = playerCamera.GetComponent<Camera>();
        }

        SetupTrigger(entryTrigger, true);
        SetupTrigger(exitTrigger, false);

        loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        loadOperation.allowSceneActivation = false;
    }

    void SetupTrigger(Collider col, bool isEntry)
    {
        if (col == null) return;
        var helper = col.gameObject.AddComponent<ExitTriggerHelper>();
        helper.exit = this;
        helper.isEntry = isEntry;
    }

    public void OnEntryTriggerEnter()
    {
        if (isActive) return;
        isActive = true;

        endlessRunner?.SlowDownForExit(slowWalkSpeed);

        if (playerCam != null)
            playerCam.DOFieldOfView(normalFOV, fovChangeDuration).SetEase(Ease.OutQuad);

        EntrySequence();
    }

    private void EntrySequence()
    {
        if (hallwayCenter == null || characterController == null)
        {
            OpenDoor();
            return;
        }

        Vector3 toCenter = hallwayCenter.position - playerBody.position;
        Vector3 lateralOffset = toCenter - Vector3.Project(toCenter, playerBody.forward);
        lateralOffset.y = 0f;

        float prevT = 0f;
        DOTween.To(() => prevT, x =>
        {
            float delta = x - prevT;
            prevT = x;
            characterController.Move(lateralOffset * delta);
        }, 1f, centeringDuration)
        .SetEase(Ease.InOutSine)
        .OnComplete(() => OpenDoor());
    }

    private void OpenDoor()
    {
        if (door == null) return;
        Vector3 targetEuler = door.transform.eulerAngles;
        targetEuler.y = doorOpenAngle;
        door.transform.DORotate(targetEuler, doorOpenDuration).SetEase(Ease.OutQuad);
    }

    public void OnExitTriggerEnter()
    {
        if (isExitActive || loadOperation == null) return;
        isExitActive = true; // Geçişi birden fazla tetiklememek için kilitliyoruz

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color color = fadeImage.color;
            color.a = 0f; // Siyah resmi başlangıçta tamamen saydam yap
            fadeImage.color = color;

            fadeImage.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                loadOperation.allowSceneActivation = true; // Ekran karardıktan sonra yeni sahneye geç
            });
        }
        else
        {
            loadOperation.allowSceneActivation = true; // Image atanmamışsa beklemeden direkt geç
        }
    }
}

public class ExitTriggerHelper : MonoBehaviour
{
    [HideInInspector] public EndlessRunnerExit exit;
    [HideInInspector] public bool isEntry;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isEntry) exit?.OnEntryTriggerEnter();
        else exit?.OnExitTriggerEnter();
    }
}
