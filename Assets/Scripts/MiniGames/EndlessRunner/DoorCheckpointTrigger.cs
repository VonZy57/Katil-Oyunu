using UnityEngine;

// Kapıya dönüş noktası — trigger box'a ekle
public class DoorCheckpointTrigger : MonoBehaviour
{
    public EndlessRunner endlessRunner;
    public GameObject leftLightObject;   // Sol kapı ışık objesi
    public GameObject rightLightObject;  // Sağ kapı ışık objesi
    [Tooltip("true = sol (A tuşu), false = sağ (D tuşu)")]
    public bool requireLeft = true;
    private bool triggered = false;

    void Start()
    {
        if (leftLightObject) leftLightObject.SetActive(false);
        if (rightLightObject) rightLightObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        // Doğru tarafın ışığını aç, yanlışı kapat
        if (leftLightObject) leftLightObject.SetActive(requireLeft);
        if (rightLightObject) rightLightObject.SetActive(!requireLeft);

        if (endlessRunner != null)
            endlessRunner.OnDoorCheckpointReached(this, requireLeft);
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
