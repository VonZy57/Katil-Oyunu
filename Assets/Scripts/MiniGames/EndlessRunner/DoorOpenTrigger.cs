using UnityEngine;

// Kapı açma bölgesi — kapının önüne koy, trigger olarak ayarla
public class DoorOpenTrigger : MonoBehaviour
{
    public EndlessRunner endlessRunner;
    [Tooltip("Bu sol kapının trigger'ı mı?")]
    public bool isLeftDoor = true;

    void Start()
    {
        if (endlessRunner == null)
            endlessRunner = FindFirstObjectByType<EndlessRunner>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        endlessRunner?.OnDoorOpenZoneEntered(isLeftDoor);
    }
}
