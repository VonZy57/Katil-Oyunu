using UnityEngine;

public class PunchTrigger : MonoBehaviour
{
    [Header("Animasyon Referansları")]
    [SerializeField] private Animator amcaAnimator;

    private void OnTriggerEnter(Collider other)
    {
        // Temas edilen objede MeetAndHelpUncle scripti var mı kontrol et
        MeetAndHelpUncle amcaScript = other.GetComponent<MeetAndHelpUncle>();
        if (amcaScript != null)
        {
            if (amcaAnimator != null)
            {
                amcaAnimator.SetTrigger("HitTrigger");
            }
        }
    }
    
}
