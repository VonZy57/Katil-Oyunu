using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    [Header("Hangi Görevi Tamamlayacak?")]
    // Eðer burayý boþ býrakýrsan direkt sýradaki göreve geçer.
    // Dolu býrakýrsan, sadece o görev aktifse çalýþýr (Güvenlik önlemi).
    [SerializeField] private MissionSO requiredActiveMission;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // Zaten çalýþtýysa tekrar çalýþma

        NewMission(other.gameObject);
    }

    public void NewMission(GameObject playerObj)
    {
        if (playerObj.CompareTag("Player"))
        {
            // Eðer belirli bir görev þartý varsa kontrol et
            if (requiredActiveMission != null)
            {
                if (MissionManager.Instance.CurrentMission == requiredActiveMission)
                {
                    Complete();
                }
            }
            else
            {
                // Þart yoksa direkt mevcut görevi bitir
                Complete();
            }
        }
    }

    public void Complete()
    {
        triggered = true;
        MissionManager.Instance.CompleteCurrentMission();
        // Ýstersen bu obje kendini yok edebilir:
        // Destroy(gameObject);
    }
}