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
    
    private float currentLookWeight = 0f;
    private bool isLooking = false;
    private Tween lookTween;

    void Start()
    {
        animator = GetComponent<Animator>();
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
        if (currentTarget != null && currentLookWeight > 0f)
        {
            animator.SetLookAtPosition(currentTarget.position);
            animator.SetLookAtWeight(currentLookWeight, bodyWeight * currentLookWeight, headWeight * currentLookWeight);
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