using DG.Tweening;
using UnityEngine;

// Kapıya dönüş noktası — trigger box'a ekle
public class DoorCheckpointTrigger : MonoBehaviour
{
    public EndlessRunner endlessRunner;
    public GameObject leftLightObject;   // Sol kapı ışık objesi
    public GameObject rightLightObject;  // Sağ kapı ışık objesi
    public GameObject leftDoor;          // Sol kapı objesi
    public GameObject rightDoor;         // Sağ kapı objesi
    [Tooltip("Kapının açılma açısı (Y ekseni)")]
    public float doorOpenAngle = 90f;
    [Tooltip("Kapının açılma süresi")]
    public float doorOpenDuration = 0.4f;
    [Tooltip("true = sol (A tuşu), false = sağ (D tuşu)")]
    public bool requireLeft = true;
    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        if (endlessRunner != null)
            endlessRunner.OnDoorCheckpointReached(this, requireLeft);
    }

    public void OpenCorrectDoor()
    {
        GameObject door = requireLeft ? leftDoor : rightDoor;
        if (door != null)
        {
            Vector3 target = door.transform.localEulerAngles + new Vector3(0f, doorOpenAngle, 0f);
            door.transform.DOLocalRotate(target, doorOpenDuration).SetEase(Ease.OutQuad);
        }
    }

    public void TurnOffLights()
    {
        if (leftLightObject) leftLightObject.SetActive(false);
        if (rightLightObject) rightLightObject.SetActive(false);
    }

    public void ResetCheckpoint()
    {
        triggered = false;
        TurnOffLights();
    }
}
