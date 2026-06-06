using UnityEngine;
using UnityEngine.Splines;
using DG.Tweening;
using System.Collections;

public class LavukMovement : MonoBehaviour
{
    [Header("Referanslar")]
    public Animator lavukAnimator;
    public SplineContainer lavukPathSpline;
    
    [Header("Ayarlar")]
    public float walkSpeed = 2f;
    public string walkTriggerParam = "EndFightTrigger";
    public string enterCarTriggerParam = "EnterCarTrigger";

    private SplineAnimate splineAnimate;

    void Start()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        
        splineAnimate.enabled = false;
    }

    public void StartWalkingToCar()
    {
        StartCoroutine(WalkToCarRoutine());
    }

    private IEnumerator WalkToCarRoutine()
    {
        Debug.Log("Lavuk yürüme rutinine başladı.");
        // Yürüme animasyonunu başlat
        lavukAnimator.SetTrigger(walkTriggerParam);

        // Spline başlangıç noktasını (Knot 0) dünya koordinatında bul
        Vector3 localStartPos = lavukPathSpline.EvaluatePosition(0f);
        Vector3 worldStartPos = lavukPathSpline.transform.TransformPoint(localStartPos);

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
        splineAnimate.Container = lavukPathSpline;
        splineAnimate.AnimationMethod = SplineAnimate.Method.Speed;
        splineAnimate.MaxSpeed = walkSpeed;
        splineAnimate.Loop = SplineAnimate.LoopMode.Once;
        splineAnimate.Alignment = SplineAnimate.AlignmentMode.SplineElement; // Yüzünü gittiği yöne dön

        splineAnimate.enabled = true;
        splineAnimate.Restart(true);

        // Spline boyunca hareketin bitmesini bekle
        yield return new WaitUntil(() => !splineAnimate.IsPlaying);
        
        splineAnimate.enabled = false;

        // Arabaya binme animasyonunu tetikle
        lavukAnimator.SetTrigger(enterCarTriggerParam);
    }
}
