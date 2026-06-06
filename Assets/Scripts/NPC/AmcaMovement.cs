using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections;

public class AmcaMovement : MonoBehaviour
{
    [Header("Referanslar")]
    public Animator amcaAnimator;
    public SplineContainer amcaPathSpline;
    public Transform splineStartReference;
    public Transform chairTransform;
    
    [Header("Arabaya Yürüme Referansları")]
    public SplineContainer carPathSpline;
    public Transform carSplineStartReference;
    public Transform amcaStandReference;

    [Header("Ayarlar")]
    public float walkSpeed = 1.5f; // Yaralı yürüdüğü için varsayılanı biraz düşürdüm, editörden ayarlayabilirsiniz
    public string walkInjuredTriggerParam = "WalkInjuredTrigger";
    public string sitTriggerParam = "SitDownTrigger";
    public string walkTriggerParam = "WalkTrigger";

    private SplineAnimate splineAnimate;

    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        
        splineAnimate.enabled = false;
    }

    private void LateUpdate()
    {
        // SplineAnimate çalışırken karakterin yokuşlarda öne/arkaya veya yanlara eğilmesini engelle
        if (splineAnimate != null && splineAnimate.IsPlaying)
        {
            Vector3 currentEuler = transform.eulerAngles;
            currentEuler.x = 0f;
            //currentEuler.z = 0f;
            transform.eulerAngles = currentEuler;
        }
    }

    public void StartWalkingToSeat()
    {
        StartCoroutine(WalkToSeatRoutine());
    }

    private IEnumerator WalkToSeatRoutine()
    {
        // Yaralı yürüme animasyonunu başlat
        amcaAnimator.SetTrigger(walkInjuredTriggerParam);

        // Hedef olarak atadığınız referans objesinin lokasyonunu al
        Vector3 worldStartPos = splineStartReference.position;

        // Yürüme süresini mesafe ve hıza göre hesapla
        float distance = Vector3.Distance(transform.position, worldStartPos);
        float duration = distance / walkSpeed;

        // Bulunduğu yerden Spline'ın başlangıcına dön ve yürü
        if (distance > 0.05f) // Çok yakın değilse DOTween çalışsın
        {
            transform.DOLookAt(worldStartPos, 0.3f, AxisConstraint.Y);
            yield return transform.DOMove(worldStartPos, duration).SetEase(Ease.Linear).WaitForCompletion();
        }

        // SplineAnimate ile asıl yolculuğu devral
        splineAnimate.Container = amcaPathSpline;
        splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        splineAnimate.MaxSpeed = walkSpeed;
        splineAnimate.Loop = SplineAnimate.LoopMode.Once;
        splineAnimate.Alignment = SplineAnimate.AlignmentMode.SplineElement; // Yüzünü gittiği yöne dön

        splineAnimate.enabled = true;
        splineAnimate.Restart(true);

        // Spline boyunca hareketin bitmesini bekle
        yield return new WaitUntil(() => !splineAnimate.IsPlaying);
        
        splineAnimate.enabled = false;

        // Sandalyeye varınca oturma animasyonunu tetikle
        amcaAnimator.SetTrigger(sitTriggerParam);

        // Sandalye oturma animasyonu sırasında hareket etsin
        if (chairTransform != null)
        {
            chairTransform.DOMoveZ(chairTransform.position.z -0.035f, 2f).SetEase(Ease.OutSine);
        }
    }

    public void StartStandAndWalkToCar()
    {
        StartCoroutine(StandToWalkToCarRoutine());
    }

    private IEnumerator StandToWalkToCarRoutine()
    {
        // DoTween ile ayağa kalkarken amcanın yüzünü kalkma referansına dönsün.
        if (amcaStandReference != null)
        {
            yield return transform.DOLookAt(amcaStandReference.position, 0.5f, AxisConstraint.Y).WaitForCompletion();
        }

        amcaAnimator.SetTrigger("StandUpTrigger");

        yield return new WaitForSeconds(2f); // Ayağa kalkma animasyonunun bitmesi için bekleme (animasyon süresine göre ayarla)

        //Burada otomatik breathing idle'a geçiş yapıyoruz.
        yield return new WaitForSeconds(1f); // Ayağa kalkma animasyonunun bitiminde küçük bir bekleme
        
        // İyileşmiş yürüme animasyonunu başlat
        amcaAnimator.SetTrigger(walkTriggerParam);

        // Yeni hedefin lokasyonunu al
        Vector3 worldStartPos = carSplineStartReference.position;
        float distance = Vector3.Distance(transform.position, worldStartPos);
        float duration = distance / walkSpeed;

        // Bulunduğu yerden Spline'ın başlangıcına dön ve yürü
        if (distance > 0.05f) 
        {
            transform.DOLookAt(worldStartPos, 0.3f, AxisConstraint.Y);
            yield return transform.DOMove(worldStartPos, duration).SetEase(Ease.Linear).WaitForCompletion();
        }

        // PathToCar spline'ını takibe başlasın
        splineAnimate.Container = carPathSpline;
        splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        splineAnimate.MaxSpeed = walkSpeed;
        splineAnimate.Loop = SplineAnimate.LoopMode.Once;
        splineAnimate.Alignment = SplineAnimate.AlignmentMode.SplineElement;

        splineAnimate.enabled = true;
        splineAnimate.Restart(true);

        yield return new WaitUntil(() => !splineAnimate.IsPlaying);
        
        splineAnimate.enabled = false;
    }
}
