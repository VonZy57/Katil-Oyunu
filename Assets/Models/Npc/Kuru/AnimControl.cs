using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Animator))]
public class LookAtController : MonoBehaviour
{
    [Header("Look At Settings")]
    public Transform objectToLookAt; // Genellikle Player
    public float headWeight;
    public float bodyWeight;
    public float lookDistance;
    
    [Header("DOTween Settings")]
    public float tweenDuration = 0.5f; // Bakışın ne kadar sürede yumuşayarak oturacağı

    [Header("Debug Info")]
    public float distanceWithPlayer;

    private Animator animator;
    
    private Transform overrideTarget; // Dışarıdan atanacak baskın hedef
    private Transform currentTarget;  // Şu an bakılan güncel hedef
    
    private Transform previousTarget; // Hedef değişimini algılamak için
    private Vector3 currentLookPosition; // Bakılan anlık pozisyon
    private Tween positionTween; // Hedefler arası pozisyon geçişi tutucu

    private float currentLookWeight = 0f;
    private bool isLooking = false;
    private Tween lookTween;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (objectToLookAt != null)
        {
            currentLookPosition = objectToLookAt.position;
        }
    }

    private void Update()
    {
        if (objectToLookAt != null)
        {
            distanceWithPlayer = Vector3.Distance(transform.position, objectToLookAt.position);
        }

        // Hedefi belirle: Eğer override varsa onu, yoksa normal nesneyi kullan
        currentTarget = overrideTarget != null ? overrideTarget : objectToLookAt;

        // Bakış şartı: Eğer bir override hedefi varsa koşulsuz bak, yoksa mesafeyi kontrol et
        bool shouldLook = currentTarget != null && (overrideTarget != null || distanceWithPlayer < lookDistance);

        // Hedef değişimi anını yakala (Pozisyon geçişi için)
        if (currentTarget != previousTarget)
        {
            positionTween?.Kill();
            
            if (currentTarget != null)
            {
                if (previousTarget == null || !isLooking) 
                {
                    currentLookPosition = currentTarget.position;
                }
                else
                {
                    // Hedef hareketliyse (örn: oyuncu yürüyorsa) pozisyonu dinamik takip edebilmek için DOVirtual kullanıyoruz.
                    Vector3 startPos = currentLookPosition;
                    positionTween = DOVirtual.Float(0f, 1f, tweenDuration, t => {
                        if (currentTarget != null) {
                            currentLookPosition = Vector3.Lerp(startPos, currentTarget.position, t);
                        }
                    }).SetEase(Ease.InOutSine);
                }
            }
            
            previousTarget = currentTarget;
        }
        
        // Eğer geçiş animasyonu bittiyse ve hedef hareketliyse, pozisyonu anlık takip etmeye devam et
        if (currentTarget != null && (positionTween == null || !positionTween.IsActive() || !positionTween.IsPlaying()))
        {
            currentLookPosition = currentTarget.position;
        }

        // Durum değiştiyse DOTween animasyonunu başlat
        if (shouldLook != isLooking)
        {
            isLooking = shouldLook;
            
            lookTween?.Kill(); // Eski hareketi iptal et
            float targetWeight = isLooking ? 1f : 0f;
            
            lookTween = DOTween.To(() => currentLookWeight, x => currentLookWeight = x, targetWeight, tweenDuration)
                               .SetEase(Ease.InOutSine);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        // currentTarget null olsa bile (hedef temizlendiğinde) kafanın aniden atmaması için sadece weight kontrolü yeterlidir.
        if (currentLookWeight > 0f)
        {
            animator.SetLookAtPosition(currentLookPosition);
            animator.SetLookAtWeight(currentLookWeight, bodyWeight, headWeight);
        }
    }

    // --- DIŞARIDAN ÇAĞRILABİLECEK METOTLAR ---

    public void SetOverrideLookTarget(Transform newTarget)
    {
        overrideTarget = newTarget;
    }

    public void ClearOverrideLookTarget()
    {
        overrideTarget = null;
    }
}