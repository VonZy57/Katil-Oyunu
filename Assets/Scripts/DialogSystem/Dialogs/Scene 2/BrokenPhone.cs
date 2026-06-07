using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BrokenPhone : Interactable
{
    [Header("References")]
    public Transform phoneLookAtTarget; // Telefona bakış hedefi
    public Transform lookAtTarget;      // Küçük kıza bakış hedefi
    public Transform playerBody;
    public Transform playerCamera;
    public GameObject phoneHandSet;
    public Transform playerHandSetRef;
    [SerializeField] private Vector3 initialHandSetPos;
    [SerializeField] private Quaternion initialHandSetRot;

    [Header("Rotation Settings")]
    public float rotationDuration = 2f;
    [Tooltip("Target'ın ne kadar üstüne bakılsın?")]
    public float lookOffset = 0.5f;

    [Header("Audio Settings")]
    public AudioClip phoneOpenSound;
    public AudioClip phoneCloseSound;
    public AudioClip lowSignalSound;
    private AudioSource audioSource;

    [Header("Diyalog Sesleri")]
    [SerializeField] private List<SpeakerAudio> speakerAudios;
    [SerializeField] private AudioClip clip_girl_brokenPhone;

    private DialogSystem dialogSystem;
    private MissionObjective missionObj;
    private DialogNode phoneDialogNode;
    private bool hasInteracted = false;

    void Start()
    {
        dialogSystem = FindFirstObjectByType<DialogSystem>();
        missionObj = GetComponent<MissionObjective>();

        if (phoneHandSet != null)
        {
            initialHandSetPos = phoneHandSet.transform.position;
            initialHandSetRot = phoneHandSet.transform.rotation;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        BuildDialogTree();
    }

    protected override void Update()
    {
        base.Update();

        if (!hasInteracted && missionObj != null && MissionManager.Instance.CurrentMission == missionObj.requiredMission)
            promptMessage = "E - Telephone";
        else
            promptMessage = "";
    }

    protected override void Interact()
    {
        if (hasInteracted || (missionObj != null && MissionManager.Instance.CurrentMission != missionObj.requiredMission))
            return;

        hasInteracted = true;
        promptMessage = "";

        RotateCameraToTarget(phoneLookAtTarget, rotationDuration);

        if (phoneHandSet != null && playerHandSetRef != null)
        {
            phoneHandSet.transform.DOMove(playerHandSetRef.position, 1f);
            phoneHandSet.transform.DORotateQuaternion(playerHandSetRef.rotation, 1f);
        }

        StartCoroutine(PlayPhoneOpenSequence());

        if (dialogSystem != null)
        {
            dialogSystem.SetSpeakers(speakerAudios);
            dialogSystem.StartDialog(phoneDialogNode);
            StartCoroutine(CheckDialogEnd());
        }
    }

    private IEnumerator PlayPhoneOpenSequence()
    {
        if (phoneOpenSound != null)
        {
            audioSource.PlayOneShot(phoneOpenSound);
            yield return new WaitForSeconds(phoneOpenSound.length);
        }

        if (lowSignalSound != null)
        {
            audioSource.clip = lowSignalSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void RotateCameraToTarget(Transform target, float duration)
    {
        if (target == null) return;

        playerBody?.DOLookAt(target.position, duration, AxisConstraint.Y);
        playerCamera?.DOLookAt(target.position + (Vector3.up * lookOffset), duration);
    }

    private IEnumerator CheckDialogEnd()
    {
        while (dialogSystem != null && dialogSystem.dialogPanel.activeSelf)
            yield return null;

        // Telefona geri dön, sonra kapat
        RotateCameraToTarget(phoneLookAtTarget, rotationDuration);
        yield return new WaitForSeconds(rotationDuration);

        // Düşük hat sesini durdur
        audioSource.loop = false;
        audioSource.Stop();

        if (phoneHandSet != null)
        {
            phoneHandSet.transform.DOMove(initialHandSetPos, 0.2f);
            phoneHandSet.transform.DORotateQuaternion(initialHandSetRot, 0.2f);
        }

        if (phoneCloseSound != null)
            audioSource.PlayOneShot(phoneCloseSound);

        if (missionObj != null)
            missionObj.OnInteracted();
    }

    void BuildDialogTree()
    {
        // TODO: Buradaki metni daha sonra belirle
        DialogNode phoneBusyNode = DialogBuilder.CreateNode("...", "...", "Engin");

        DialogNode girlNode = DialogBuilder.CreateEndNode(
            "Sir, that phone is broken.",
            "Abi o telefon bozuk.",
            "Little Girl"
        );

        // Kıza dön (ses telefon kapanana kadar devam eder)
        DialogOption phoneToGirl = DialogBuilder.CreateOptionWithEvent(
            "...", "...",
            girlNode,
            () => RotateCameraToTarget(lookAtTarget, rotationDuration),
            true
        );
        DialogBuilder.AddOption(phoneBusyNode, phoneToGirl);

        girlNode.voiceClip = clip_girl_brokenPhone;
        phoneDialogNode = phoneBusyNode;
    }
}
