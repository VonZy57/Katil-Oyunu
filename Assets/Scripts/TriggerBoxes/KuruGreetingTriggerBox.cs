using UnityEngine;

public class KuruGreetingTriggerBox : MonoBehaviour
{
    [Header("Kuru Referansı")]
    [SerializeField] KuruGreeting kuruGreeting;
    [SerializeField] DoorInteraction motelDoor;
    bool hasInteracted = false;

    private void Start()
    {
        kuruGreeting = FindFirstObjectByType<KuruGreeting>();
        motelDoor = FindFirstObjectByType<DoorInteraction>();
        if (kuruGreeting == null)
            Debug.Log("script atanamadı");
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasInteracted)
        {
            hasInteracted = true;
            kuruGreeting.StartIntroDialog();
            motelDoor.CloseDoor();
        }
    }
}
