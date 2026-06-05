using UnityEngine;

public class KuruGreetingTriggerBox : MonoBehaviour
{
    [Header("Kuru Referansı")]
    [SerializeField] KuruGreeting kuruGreeting;
    [SerializeField] DoorInteraction motelDoor;
    bool hasInteracted = false;

    [Header("Girişte Kapatılacaklar")]
    [SerializeField] AudioSource audioToDisable;
    [SerializeField] MonoBehaviour[] scriptsToDisable;

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

            if (audioToDisable != null) audioToDisable.enabled = false;
            foreach (var script in scriptsToDisable)
                if (script != null)
                {
                    script.enabled = false;
                    Debug.Log($"{script.GetType().Name} devre dışı bırakıldı.");
                }
        }


        
    }
}
