using UnityEngine;

public class KuruGreetingTriggerBox : MonoBehaviour
{
    [Header("Kuru Referansı")]
    [SerializeField] KuruGreeting kuruGreeting;
    bool hasInteracted = false;

    private void Start()
    {
        kuruGreeting = FindFirstObjectByType<KuruGreeting>();
        if (kuruGreeting == null)
            Debug.Log("script atanamadı");
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasInteracted)
        {
            hasInteracted = true;
            kuruGreeting.StartIntroDialog();
        }
    }
}
