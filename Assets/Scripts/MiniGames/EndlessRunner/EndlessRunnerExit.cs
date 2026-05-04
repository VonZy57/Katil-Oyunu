using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndlessRunnerExit : MonoBehaviour
{
    [Header("Referanslar")]
    public EndlessRunner endlessRunner;

    [Header("Trigger'lar")]
    public Collider entryTrigger;
    public Collider exitTrigger;

    [Header("Sahne")]
    public int sceneToLoad = 2;

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
    public float fovChangeSpeed = 2f;

    // Dahili
    private bool isActive = false;
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
    }

    void SetupTrigger(Collider col, bool isEntry)
    {
        if (col == null) return;
        var helper = col.gameObject.AddComponent<ExitTriggerHelper>();
        helper.exit = this;
        helper.isEntry = isEntry;
    }

    void Update()
    {
        if (!isActive) return;

        if (playerCam != null)
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, normalFOV, Time.deltaTime * fovChangeSpeed);
    }

    public void OnEntryTriggerEnter()
    {
        if (isActive) return;
        isActive = true;

        endlessRunner?.SlowDownForExit(slowWalkSpeed);

        loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);
        loadOperation.allowSceneActivation = false;

        StartCoroutine(EntrySequence());
    }

    private IEnumerator EntrySequence()
    {
        if (hallwayCenter != null && characterController != null)
        {
            Vector3 toCenter = hallwayCenter.position - playerBody.position;
            Vector3 lateralOffset = toCenter - Vector3.Project(toCenter, playerBody.forward);
            lateralOffset.y = 0f;

            float elapsed = 0f;
            Vector3 startPos = playerBody.position;
            Vector3 targetPos = startPos + lateralOffset;

            characterController.enabled = false;
            while (elapsed < centeringDuration)
            {
                elapsed += Time.deltaTime;
                playerBody.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, elapsed / centeringDuration));
                yield return null;
            }
            characterController.enabled = true;
        }

        if (door != null)
        {
            Vector3 targetEuler = door.transform.eulerAngles;
            targetEuler.y = doorOpenAngle;
            door.transform.DORotate(targetEuler, doorOpenDuration).SetEase(Ease.OutQuad);
        }
    }

    public void OnExitTriggerEnter()
    {
        if (loadOperation == null) return;
        loadOperation.allowSceneActivation = true;
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
