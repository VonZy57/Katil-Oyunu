using System.Collections;
using DG.Tweening;
using UnityEngine;

public class BodyDragController : MonoBehaviour
{
    [Header("Referanslar")]
    public SkillCheckSystem skillCheckSystem;
    // Oyuncunun kendi hareket scriptini (örn: FirstPersonController) buraya ata
    public FirstPersonController playerMovementScript;

    [Header("Hız Ayarları")]
    public float normalSpeed = 4f;     // Ceset bırakıldığında dönülecek normal hız
    public float dragSpeed = 2f;       // Ceset taşırkenki standart hız
    public float verySlowSpeed = 0.5f; // Skill check sırasında veya kaçırıldığında düşülecek yavaş hız

    public bool isDragging { get; private set; } = false;
    private bool isInDropZone = false;
    private Coroutine dragCoroutine;

    public void StartDraggingTask()
    {
        if (isDragging) return;

        isDragging = true;
        isInDropZone = false;
        Debug.Log("Ceset taşıma görevi başladı!");

        SetPlayerSpeed(dragSpeed);

        dragCoroutine = StartCoroutine(DragRoutine());
    }

    private void SetPlayerSpeed(float targetSpeed)
    {
        if (playerMovementScript != null)
        {
            // Bu objeye (playerMovementScript) ait önceki hız tween'lerini sonlandır.
            DOTween.Kill(playerMovementScript);

            // DOTween kullanarak hızı 0.5 saniyede yumuşak bir şekilde değiştir.
            DOTween.To(() => playerMovementScript.walkSpeed, x => playerMovementScript.walkSpeed = x, targetSpeed, 0.5f)
                .SetId(playerMovementScript);
        }
    }

    IEnumerator DragRoutine()
    {
        while (isDragging && !isInDropZone)
        {
            // Skill check sorulduğu an çok yavaş hareket et
            SetPlayerSpeed(verySlowSpeed);

            bool? checkResult = null;

            skillCheckSystem.StartSkillCheck((result) => { checkResult = result; });

            yield return new WaitUntil(() => checkResult.HasValue);

            if (checkResult.Value == true)
            {
                // Başarılı olunca standart ceset taşıma hızına dön
                SetPlayerSpeed(dragSpeed);

                yield return new WaitForSeconds(2f);
            }
            else
            {
                // Başarısız olunca (sersemleme süresi boyunca) çok yavaş hareket etmeye devam et
                SetPlayerSpeed(verySlowSpeed);

                Debug.Log("Skill check kaçırıldı! Kısa bir sersemleme...");
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Banyoya girildi! Ceset bırakma alanına gir...");
        // Oyuncu alana girdiğinde
        if (other.CompareTag("DropZone") && isDragging)
        {
            isInDropZone = true;

            // 1. Skill check döngüsünü ve UI'ı tamamen durdur
            if (dragCoroutine != null)
            {
                StopCoroutine(dragCoroutine);
            }
            skillCheckSystem.ForceStop();

            // 2. Oyuncunun küvete yürüyebilmesi için hareket hızını taşıma hızına ayarla
            SetPlayerSpeed(dragSpeed);

            Debug.Log("DropZone'a girildi! Skill check bitti. Cesedi bırakmak için küvete yürüyüp etkileşime geçin.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Oyuncu DropZone'dan geri çıkarsa
        if (other.CompareTag("DropZone") && isDragging)
        {
            isInDropZone = false;

            // Oyuncu DropZone'dan çıkınca skill check tekrar başlayacağı için hızı çok yavaşlat
            SetPlayerSpeed(verySlowSpeed);

            // Skill check döngüsünü yeniden başlat
            // Önceki coroutine zaten OnTriggerEnter'da durdurulmuştu, yenisini başlatabiliriz.
            if (dragCoroutine != null)
            {
                StopCoroutine(dragCoroutine); // Güvenlik önlemi olarak, eğer bir şekilde çalışıyorsa durdur.
            }
            dragCoroutine = StartCoroutine(DragRoutine());

            Debug.Log("DropZone'dan çıkıldı! Skill check yeniden başladı.");
        }
    }

    public void DropBodyAndFinish()
    {
        if (!isDragging) return;

        isDragging = false;

        if (dragCoroutine != null)
        {
            StopCoroutine(dragCoroutine);
            dragCoroutine = null;
        }
        skillCheckSystem.ForceStop();

        // 3. Küvetle etkileşime girildi, ceset bırakıldı. Hareket hızını normal hıza döndür.
        SetPlayerSpeed(normalSpeed);

        Debug.Log("Ceset küvete bırakıldı. Görev Bitti!");
    }
}