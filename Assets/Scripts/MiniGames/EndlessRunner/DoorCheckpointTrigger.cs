using DG.Tweening;
using UnityEngine;

public class DoorCheckpointTrigger : MonoBehaviour
{
    public EndlessRunner endlessRunner;
    public GameObject door;
    [Tooltip("Kapının açılma açısı (Y ekseni)")]
    public float doorOpenAngle = 90f;
    [Tooltip("Kapının açılma süresi")]
    public float doorOpenDuration = 0.4f;
    [Tooltip("true = sol (A tuşu), false = sağ (D tuşu)")]
    public bool requireLeft = true;

    private bool triggered = false;
    private Vector3 doorInitialRotation;

    void Awake()
    {
        if (door) doorInitialRotation = door.transform.localEulerAngles;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        if (endlessRunner != null)
            endlessRunner.OnDoorCheckpointReached(this, requireLeft);
    }

    public void OpenCorrectDoor()
    {
        if (door == null) return;

        Vector3 target = door.transform.localEulerAngles + new Vector3(0f, doorOpenAngle, 0f);
        door.transform.DOLocalRotate(target, doorOpenDuration).SetEase(Ease.OutQuad)
            .OnComplete(() => SetDoorColliders(false));
    }

    void SetDoorColliders(bool enabled)
    {
        foreach (var col in door.GetComponentsInChildren<Collider>())
            col.enabled = enabled;
    }

    public void TurnOffLights() { }

    public void ResetCheckpoint()
    {
        triggered = false;

        if (door)
        {
            door.transform.DOKill();
            door.transform.localEulerAngles = doorInitialRotation;
            SetDoorColliders(true);
        }
    }
}
